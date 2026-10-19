namespace Chapter02.Topics;

public static class T09_ExceptionBasics
{
    // Guard clauses: reject bad input at the start of a public method
    private static decimal ApplyDiscount(decimal orderTotal, decimal percent, string? couponCode)
    {
        ArgumentNullException.ThrowIfNull(couponCode);                // .NET 6+
        ArgumentOutOfRangeException.ThrowIfNegative(orderTotal);      // .NET 8+
        if (percent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(percent), percent, "Discount must be between 0 and 100.");
        return orderTotal - orderTotal * percent / 100;
    }

    private static void TryDiscount(decimal total, decimal percent, string? code)
    {
        try
        {
            Console.WriteLine($"  ApplyDiscount({total}, {percent}, {code ?? "null"}) = {ApplyDiscount(total, percent, code)}");
        }
        // Order matters: most specific first. ArgumentNullException and ArgumentOutOfRangeException
        // both derive from ArgumentException. Putting catch (ArgumentException) first is compile error CS0160.
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"  ArgumentNullException for '{ex.ParamName}'");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"  ArgumentOutOfRangeException for '{ex.ParamName}'");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"  Other ArgumentException: {ex.Message}");
        }
    }

    // finally runs whether the try block returns normally, returns early, or throws
    private static string ReserveStock(string sku, bool gatewayFails)
    {
        Console.WriteLine($"  [{sku}] lock warehouse row");
        try
        {
            if (gatewayFails) throw new TimeoutException("Inventory service timed out.");
            return "reserved";                     // finally still runs before the method returns
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine($"  [{sku}] caught: {ex.Message}");
            return "retry later";
        }
        finally
        {
            Console.WriteLine($"  [{sku}] finally: release warehouse row");
        }
    }

    // DANGER: an exception thrown inside finally replaces the original one
    private static void UploadInvoice()
    {
        try
        {
            throw new IOException("Disk full while writing invoice INV-42.");   // the REAL problem
        }
        finally
        {
            throw new InvalidOperationException("Cleanup failed: temp file already deleted.");
        }
    }

    public static void Run()
    {
        Console.WriteLine("Guard clauses and catch order:");
        TryDiscount(1_000m, 10m, "DIWALI10");
        TryDiscount(1_000m, 10m, null);
        TryDiscount(-5m, 10m, "DIWALI10");
        TryDiscount(1_000m, 150m, "DIWALI10");

        Console.WriteLine("try / catch / finally:");
        Console.WriteLine($"  result: {ReserveStock("SKU-1", gatewayFails: false)}");
        Console.WriteLine($"  result: {ReserveStock("SKU-2", gatewayFails: true)}");

        Console.WriteLine("Exception thrown from finally:");
        try { UploadInvoice(); }
        catch (Exception ex) { Console.WriteLine($"  caller sees: {ex.GetType().Name}: {ex.Message}  <- the IOException is lost"); }

        // Wrapping: add context, keep the cause in InnerException
        try
        {
            try { _ = int.Parse("12O0"); }   // letter O instead of zero, from a CSV import
            catch (FormatException ex) { throw new InvalidDataException("Row 7 of price-list.csv has an invalid price.", ex); }
        }
        catch (InvalidDataException ex)
        {
            Console.WriteLine($"Wrapped: {ex.Message} | Inner: {ex.InnerException?.GetType().Name}");
        }

        // The hierarchy
        Console.WriteLine($"Base chain of ArgumentNullException: {BaseChain(typeof(ArgumentNullException))}");
        Console.WriteLine($"Base chain of FileNotFoundException: {BaseChain(typeof(FileNotFoundException))}");
        Console.WriteLine($"Base chain of TaskCanceledException: {BaseChain(typeof(TaskCanceledException))}");
    }

    private static string BaseChain(Type t)
    {
        var names = new List<string>();
        for (Type? current = t; current != null && current != typeof(object); current = current.BaseType)
            names.Add(current.Name);
        return string.Join(" -> ", names);
    }
}
