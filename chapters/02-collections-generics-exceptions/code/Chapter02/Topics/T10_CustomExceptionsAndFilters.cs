using System.Net;

namespace Chapter02.Topics;

// A custom exception: a clear name, extra data callers can act on, and the standard constructors
public class InsufficientStockException : Exception
{
    public string Sku { get; } = "";
    public int Requested { get; }
    public int Available { get; }

    public InsufficientStockException() { }
    public InsufficientStockException(string message) : base(message) { }
    public InsufficientStockException(string message, Exception innerException) : base(message, innerException) { }

    public InsufficientStockException(string sku, int requested, int available)
        : base($"Cannot reserve {requested} of {sku}; only {available} left.")
    {
        Sku = sku;
        Requested = requested;
        Available = available;
    }
}

public static class T10_CustomExceptionsAndFilters
{
    private static readonly Dictionary<string, int> Stock = new() { ["SKU-1"] = 3 };

    private static void Reserve(string sku, int quantity)
    {
        int available = Stock.GetValueOrDefault(sku);
        if (quantity > available) throw new InsufficientStockException(sku, quantity, available);
        Stock[sku] = available - quantity;
    }

    // Simulates a shipping-rate API that is down for the first two calls
    private static int _rateCalls;
    private static decimal GetShippingRate(string pincode)
    {
        _rateCalls++;
        if (_rateCalls <= 2) throw new HttpRequestException("Service Unavailable", null, HttpStatusCode.ServiceUnavailable);
        if (pincode == "000000") throw new HttpRequestException("Bad Request", null, HttpStatusCode.BadRequest);
        return 49m;
    }

    private static decimal GetShippingRateWithRetry(string pincode, int maxAttempts)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return GetShippingRate(pincode);
            }
            // The filter decides BEFORE catching: only 503s are retried, and only while attempts remain.
            // A 400 Bad Request is not caught here at all, so it flows to the caller untouched.
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable && attempt < maxAttempts)
            {
                Console.WriteLine($"  attempt {attempt}: 503, retrying");
            }
        }
    }

    // A filter that always returns false: it lets you look at (log) the exception without catching it
    private static bool LogAndContinue(Exception ex, string where)
    {
        Console.WriteLine($"  filter in {where}: saw {ex.GetType().Name} (stack not unwound yet)");
        return false;
    }

    private static void ChargeCard()
    {
        try
        {
            throw new InvalidOperationException("Card declined.");
        }
        finally
        {
            Console.WriteLine("  ChargeCard finally: release payment lock");
        }
    }

    public static void Run()
    {
        try { Reserve("SKU-1", 5); }
        catch (InsufficientStockException ex)
        {
            Console.WriteLine($"{ex.Message} -> API can reply: \"Only {ex.Available} of {ex.Sku} available\"");
        }

        Console.WriteLine("Retry only on 503 with an exception filter:");
        _rateCalls = 0;
        Console.WriteLine($"  rate = {GetShippingRateWithRetry("411001", maxAttempts: 3)}");

        try { GetShippingRateWithRetry("000000", maxAttempts: 3); }
        catch (HttpRequestException ex) { Console.WriteLine($"  400 is not retried: caller got StatusCode {(int?)ex.StatusCode}"); }

        Console.WriteLine("Order of execution: filter runs BEFORE inner finally blocks:");
        try
        {
            ChargeCard();
        }
        catch (Exception ex) when (LogAndContinue(ex, "Run"))
        {
            Console.WriteLine("  never printed: the filter returned false");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"  catch block: {ex.Message}");
        }
    }
}
