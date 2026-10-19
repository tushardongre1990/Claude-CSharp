using Microsoft.Extensions.Logging;

namespace Chapter02.Topics;

// A tiny logger provider that prints one compact line per log entry, synchronously,
// so the demo output is easy to read. In a real app you would use the built-in
// console provider, Serilog, or OpenTelemetry instead.
public sealed class CompactConsoleLoggerProvider : ILoggerProvider
{
    public static int EntriesWritten;
    public ILogger CreateLogger(string categoryName) => new CompactLogger(categoryName.Split('.')[^1]);
    public void Dispose() { }

    private sealed class CompactLogger(string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            EntriesWritten++;
            string ex = exception is null ? "no exception attached" : $"exception: {exception.GetType().Name}, has stack trace: {exception.StackTrace is not null}";
            Console.WriteLine($"    LOG {logLevel} [{category}] {formatter(state, exception)} ({ex})");
        }
    }
}

public class PaymentGateway
{
    public void Charge(int orderId, decimal amount) =>
        throw new HttpRequestException($"Gateway returned 502 for order {orderId}.");
}

// ---- Anti-pattern 1: swallow ----
public class SwallowingPaymentService(PaymentGateway gateway)
{
    public bool Pay(int orderId, decimal amount)
    {
        try { gateway.Charge(orderId, amount); return true; }
        catch { return true; }   // "it's fine" -> the order is marked paid but no money was taken
    }
}

// ---- Anti-pattern 2: log and rethrow at every layer ----
public class NoisyPaymentService(PaymentGateway gateway, ILogger<NoisyPaymentService> logger)
{
    public void Pay(int orderId, decimal amount)
    {
        try { gateway.Charge(orderId, amount); }
        catch (Exception ex) { logger.LogError(ex, "Payment failed"); throw; }
    }
}

public class NoisyCheckoutService(NoisyPaymentService payments, ILogger<NoisyCheckoutService> logger)
{
    public void Checkout(int orderId, decimal amount)
    {
        try { payments.Pay(orderId, amount); }
        catch (Exception ex) { logger.LogError(ex.Message); throw; }   // also loses the stack trace in the log
    }
}

// ---- Good: lower layers add context (or do nothing); ONE boundary logs ----
public class PaymentService(PaymentGateway gateway)
{
    public void Pay(int orderId, decimal amount)
    {
        try { gateway.Charge(orderId, amount); }
        catch (HttpRequestException ex)
        {
            // Wrap to add business context. Do not log here: the boundary will.
            throw new PaymentFailedException($"Payment of {amount} for order {orderId} failed.", ex);
        }
    }
}

public class CheckoutService(PaymentService payments)
{
    public void Checkout(int orderId, decimal amount) => payments.Pay(orderId, amount);   // nothing to add, so no try/catch
}

public static class T14_LoggingExceptions
{
    public static void Run()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddProvider(new CompactConsoleLoggerProvider()));
        var gateway = new PaymentGateway();

        Console.WriteLine("1) Swallowing:");
        CompactConsoleLoggerProvider.EntriesWritten = 0;
        bool paid = new SwallowingPaymentService(gateway).Pay(1001, 2_500m);
        Console.WriteLine($"   Pay returned {paid}; log entries: {CompactConsoleLoggerProvider.EntriesWritten}. Nobody knows it failed.");

        Console.WriteLine("2) Log-and-rethrow in every layer:");
        CompactConsoleLoggerProvider.EntriesWritten = 0;
        var noisy = new NoisyCheckoutService(
            new NoisyPaymentService(gateway, loggerFactory.CreateLogger<NoisyPaymentService>()),
            loggerFactory.CreateLogger<NoisyCheckoutService>());
        var endpointLogger = loggerFactory.CreateLogger("CheckoutEndpoint");
        try { noisy.Checkout(1002, 799m); }
        catch (Exception ex) { endpointLogger.LogError(ex, "Unhandled error in checkout"); }
        Console.WriteLine($"   ONE failure produced {CompactConsoleLoggerProvider.EntriesWritten} error log entries.");

        Console.WriteLine("3) Wrap with context, log once at the boundary (structured):");
        CompactConsoleLoggerProvider.EntriesWritten = 0;
        var checkout = new CheckoutService(new PaymentService(gateway));
        int orderId = 1003;
        try { checkout.Checkout(orderId, 4_100m); }
        catch (PaymentFailedException ex)
        {
            // Message TEMPLATE with named placeholders, not string interpolation.
            // The exception object is the FIRST argument, so the full stack trace and inner exception are kept.
            endpointLogger.LogError(ex, "Checkout failed for order {OrderId}", orderId);
            Console.WriteLine($"   Inner cause kept: {ex.InnerException?.Message}");
        }
        Console.WriteLine($"   Log entries: {CompactConsoleLoggerProvider.EntriesWritten}.");

        Console.WriteLine("4) Cancellation is not an error:");
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        try { cts.Token.ThrowIfCancellationRequested(); }
        catch (OperationCanceledException)
        {
            endpointLogger.LogInformation("Report export cancelled by the user");
        }
    }
}
