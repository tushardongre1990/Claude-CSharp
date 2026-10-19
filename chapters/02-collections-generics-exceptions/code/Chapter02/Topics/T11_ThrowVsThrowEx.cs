using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Chapter02.Topics;

public class PaymentFailedException(string message, Exception inner) : Exception(message, inner);

public static class T11_ThrowVsThrowEx
{
    // NoInlining keeps each method as its own stack frame so the demo prints the same in Debug and Release

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CallGateway() => throw new TimeoutException("Gateway did not answer in 30s.");

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ChargeCard() => CallGateway();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void PlaceOrder_Rethrow()
    {
        try { ChargeCard(); }
        catch (TimeoutException)
        {
            throw;            // rethrows the SAME exception, stack trace kept
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void PlaceOrder_ThrowEx()
    {
        try { ChargeCard(); }
        catch (TimeoutException ex)
        {
#pragma warning disable CA2200 // the analyzer warns about exactly this bug
            throw ex;         // throws it AGAIN from here: stack trace restarts at this line
#pragma warning restore CA2200
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void PlaceOrder_Wrap()
    {
        try { ChargeCard(); }
        catch (TimeoutException ex)
        {
            throw new PaymentFailedException("Payment for order #1001 failed.", ex);   // new exception, original inside
        }
    }

    // Capture now, rethrow later (e.g. after cleanup or on another thread), keeping the original trace
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void PlaceOrder_CaptureAndRethrowLater()
    {
        ExceptionDispatchInfo? captured = null;
        try { ChargeCard(); }
        catch (TimeoutException ex) { captured = ExceptionDispatchInfo.Capture(ex); }

        Console.WriteLine("  (cleanup ran between catch and rethrow)");
        captured?.Throw();
    }

    // Prints only method names from the stack trace (file paths and line numbers differ per machine)
    private static string Frames(Exception ex) =>
        string.Join(" <- ", new StackTrace(ex).GetFrames()
            .Select(f => f.GetMethod()?.Name ?? "?")
            .Where(n => !n.StartsWith("<") && n != "Throw"));

    private static void Show(string label, Action placeOrder)
    {
        Console.WriteLine(label);
        try { placeOrder(); }
        catch (Exception ex)
        {
            Console.WriteLine($"  {ex.GetType().Name} frames: {Frames(ex)}");
            if (ex.InnerException is { } inner)
                Console.WriteLine($"  Inner {inner.GetType().Name} frames: {Frames(inner)}");
        }
    }

    public static void Run()
    {
        Show("throw;", PlaceOrder_Rethrow);
        Show("throw ex;", PlaceOrder_ThrowEx);
        Show("throw new ...(msg, ex);", PlaceOrder_Wrap);
        Show("ExceptionDispatchInfo.Capture(ex).Throw():", PlaceOrder_CaptureAndRethrowLater);
    }
}
