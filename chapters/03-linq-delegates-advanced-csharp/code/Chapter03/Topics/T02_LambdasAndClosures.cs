namespace Chapter03.Topics;

public static class T02_LambdasAndClosures
{
    // Returns a function that "remembers" the rate it was created with
    private static Func<decimal, decimal> CreateTaxCalculator(decimal rate)
    {
        return amount => amount * rate;            // 'rate' is captured: the lambda keeps it alive
    }

    // Returns a function that remembers and CHANGES a counter
    private static Func<string> CreateInvoiceNumberGenerator(string prefix)
    {
        int next = 1000;
        return () => $"{prefix}-{next++}";          // 'next' lives on after this method returns
    }

    public static void Run()
    {
        // ---- 1. Lambda syntax ----
        Func<decimal, decimal> addShipping = total => total + 49m;          // expression lambda
        Func<decimal, string, decimal> convert = (amount, currency) =>      // statement lambda
        {
            decimal rate = currency == "USD" ? 0.012m : 1m;
            return Math.Round(amount * rate, 2);
        };
        var isWeekend = (DateOnly d) => d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday; // C# 10: natural type
        Console.WriteLine($"Add shipping: {addShipping(500m)}, INR->USD: {convert(10_000m, "USD")}, 2026-11-01 weekend? {isWeekend(new DateOnly(2026, 11, 1))}");
        Console.WriteLine($"Natural type of isWeekend: {isWeekend.GetType().Name}");

        // ---- 2. Closures: a lambda captures variables, not values ----
        var gst = CreateTaxCalculator(0.18m);
        var vat = CreateTaxCalculator(0.20m);
        Console.WriteLine($"GST on 1000: {gst(1_000m)}, VAT on 1000: {vat(1_000m)}");

        var nextInvoice = CreateInvoiceNumberGenerator("INV");
        Console.WriteLine($"Invoices: {nextInvoice()}, {nextInvoice()}, {nextInvoice()}");

        decimal threshold = 1_000m;
        Func<decimal, bool> isBigOrder = total => total > threshold;
        Console.WriteLine($"Is 1500 big (threshold 1000)? {isBigOrder(1_500m)}");
        threshold = 2_000m;                         // the lambda sees the NEW value
        Console.WriteLine($"Is 1500 big (threshold changed to 2000)? {isBigOrder(1_500m)}");

        // ---- 3. The loop-variable capture trap ----
        // A 'for' loop has ONE variable i shared by every iteration.
        var retryJobs = new List<Action>();
        for (int i = 0; i < 3; i++)
            retryJobs.Add(() => Console.Write($"attempt {i}; "));
        Console.Write("for loop, captured i:      ");
        foreach (var job in retryJobs) job();
        Console.WriteLine();

        // Fix: copy into a variable declared INSIDE the loop body (a new one per iteration)
        retryJobs.Clear();
        for (int i = 0; i < 3; i++)
        {
            int attempt = i;
            retryJobs.Add(() => Console.Write($"attempt {attempt}; "));
        }
        Console.Write("for loop, copied variable: ");
        foreach (var job in retryJobs) job();
        Console.WriteLine();

        // foreach gets a fresh variable per iteration since C# 5, so it is safe
        var warehouses = new[] { "Pune", "Delhi", "Chennai" };
        var syncJobs = new List<Action>();
        foreach (var warehouse in warehouses)
            syncJobs.Add(() => Console.Write($"sync {warehouse}; "));
        Console.Write("foreach loop:              ");
        foreach (var job in syncJobs) job();
        Console.WriteLine();

        // ---- 4. static lambdas (C# 9): the compiler refuses any capture ----
        Func<decimal, decimal> roundToRupee = static amount => Math.Round(amount, 0, MidpointRounding.AwayFromZero);
        // Func<decimal, bool> bad = static t => t > threshold;   // compile error CS8820: static lambda can't capture
        Console.WriteLine($"Static lambda rounding 249.50: {roundToRupee(249.50m)}");

        // ---- 5. Lambda parameters with defaults (C# 12) ----
        var formatPrice = (decimal amount, string currency = "INR") => $"{amount:0.00} {currency}";
        Console.WriteLine($"Default parameter: {formatPrice(99m)}, explicit: {formatPrice(5m, "USD")}");
    }
}
