namespace Chapter03.Topics;

public static class T01_Delegates
{
    // A custom delegate type: "any method that takes a subtotal and returns a discount"
    public delegate decimal DiscountRule(decimal subtotal);

    // Three ordinary methods that match the DiscountRule shape
    private static decimal NoDiscount(decimal subtotal) => 0m;
    private static decimal FestiveSale(decimal subtotal) => subtotal * 0.10m;
    private static decimal FlatHundredOverThousand(decimal subtotal) => subtotal >= 1_000m ? 100m : 0m;

    // The checkout code doesn't know which rule it gets: the caller decides
    private static decimal Checkout(decimal subtotal, DiscountRule rule)
    {
        decimal discount = rule(subtotal);
        return subtotal - discount;
    }

    public static void Run()
    {
        // ---- 1. A delegate variable holds a reference to a method ----
        DiscountRule rule = FestiveSale;                 // method group conversion: no () after the name
        Console.WriteLine($"Festive sale on 2000: pay {Checkout(2_000m, rule)}");
        Console.WriteLine($"Flat 100 on 2000: pay {Checkout(2_000m, FlatHundredOverThousand)}");
        Console.WriteLine($"Flat 100 on 800:  pay {Checkout(800m, FlatHundredOverThousand)}");

        // Choosing behaviour at runtime, e.g. from configuration
        string campaign = "festive";
        DiscountRule chosen = campaign switch
        {
            "festive" => FestiveSale,
            "flat100" => FlatHundredOverThousand,
            _ => NoDiscount,
        };
        Console.WriteLine($"Campaign '{campaign}' uses {chosen.Method.Name}");

        // ---- 2. Built-in delegate types: Func, Action, Predicate ----
        Func<decimal, decimal> gst = amount => amount * 0.18m;            // input decimal, returns decimal
        Func<int, int, decimal> lineTotal = (qty, paise) => qty * paise / 100m; // two inputs, one result
        Action<string> sendSms = message => Console.WriteLine($"  SMS: {message}"); // returns nothing
        Predicate<decimal> isHighValue = amount => amount > 50_000m;      // returns bool

        Console.WriteLine($"GST on 1000: {gst(1_000m)}");
        Console.WriteLine($"3 x 4999 paise = {lineTotal(3, 4_999)}");
        sendSms("Your order #1042 has shipped");
        Console.WriteLine($"Is 75000 high value? {isHighValue(75_000m)}");

        // Predicate<T> and Func<T,bool> have the same shape but are different types
        Func<decimal, bool> asFunc = amount => amount > 50_000m;
        // Predicate<decimal> p = asFunc;          // compile error CS0029: no conversion between delegate types
        Predicate<decimal> converted = new Predicate<decimal>(asFunc);   // wrap it explicitly
        Console.WriteLine($"Converted predicate on 10: {converted(10m)}");

        // ---- 3. Multicast: one delegate, many methods ----
        Action<int> onOrderShipped = id => Console.WriteLine($"  Email sent for order {id}");
        onOrderShipped += id => Console.WriteLine($"  SMS sent for order {id}");
        onOrderShipped += id => Console.WriteLine($"  Loyalty points added for order {id}");
        Console.WriteLine($"Invocation list has {onOrderShipped.GetInvocationList().Length} methods:");
        onOrderShipped(1042);

        // With a return value, only the LAST method's result is returned
        Func<decimal, decimal> discounts = FestiveSale;
        discounts += FlatHundredOverThousand;
        Console.WriteLine($"Multicast Func result on 2000: {discounts(2_000m)} (only the last method's result)");

        // If one method throws, the rest do not run
        Action<int> fragile = id => Console.WriteLine($"  Step 1 for {id}");
        fragile += id => throw new InvalidOperationException("SMS gateway down");
        fragile += id => Console.WriteLine($"  Step 3 for {id}");
        try { fragile(7); }
        catch (InvalidOperationException ex) { Console.WriteLine($"  Stopped: {ex.Message}. Step 3 never ran."); }

        // Calling each method yourself lets every subscriber run
        foreach (Action<int> handler in fragile.GetInvocationList().Cast<Action<int>>())
        {
            try { handler(8); }
            catch (Exception ex) { Console.WriteLine($"  Handler failed ({ex.Message}), continuing"); }
        }

        // ---- 4. Removing and null ----
        Action<string>? audit = null;
        audit?.Invoke("nothing subscribed");        // ?.Invoke avoids NullReferenceException
        Action<string> log = msg => Console.WriteLine($"  AUDIT: {msg}");
        audit += log;
        audit -= log;                               // removing the only method makes it null again
        Console.WriteLine($"After += then -=, audit is null: {audit is null}");
    }
}
