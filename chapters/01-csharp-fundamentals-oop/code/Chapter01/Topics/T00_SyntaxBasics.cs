namespace Chapter01.Topics;

public static class T00_SyntaxBasics
{
    public static void Run()
    {
        // --- Statements, expressions and method calls ---
        decimal cartTotal = 2400m;                  // a statement: declares a variable
        bool freeShipping = cartTotal >= 500;       // 'cartTotal >= 500' is an expression: it produces a value
        Console.WriteLine($"Cart total {cartTotal}, free shipping? {freeShipping}");   // a method call

        // --- if / else if / else ---
        string tier;
        if (cartTotal >= 5000)
            tier = "Gold";
        else if (cartTotal >= 2000)
            tier = "Silver";
        else
            tier = "Standard";
        Console.WriteLine($"Customer tier: {tier}");

        // --- Logical operators: && (and), || (or), ! (not) ---
        bool isPrimeMember = true;
        bool alreadyUsedCoupon = false;
        bool getsDiscount = (isPrimeMember && !alreadyUsedCoupon) || cartTotal > 10_000;
        Console.WriteLine($"Gets discount? {getsDiscount}");

        // --- switch statement and switch expression ---
        string status = "Shipped";
        switch (status)
        {
            case "Pending":
                Console.WriteLine("Waiting for payment");
                break;
            case "Shipped":
                Console.WriteLine("On the way");
                break;
            default:
                Console.WriteLine("Unknown status");
                break;
        }

        string eta = status switch
        {
            "Pending" => "Not dispatched yet",
            "Shipped" => "2 days",
            _ => "Unknown",                         // _ means "anything else"
        };
        Console.WriteLine($"ETA: {eta}");

        // --- for: when you need the position (index) ---
        string[] items = { "Keyboard", "Mouse", "Monitor" };
        for (int i = 0; i < items.Length; i++)
            Console.WriteLine($"Line {i + 1}: {items[i]}");

        // --- foreach: when you just need each item ---
        decimal[] prices = { 1500m, 499m, 12000m };
        decimal sum = 0;
        foreach (var price in prices)
            sum += price;
        Console.WriteLine($"Sum of prices: {sum}");

        // --- while: repeat until a condition changes (e.g. retrying an email) ---
        int attempt = 0;
        bool sent = false;
        while (!sent && attempt < 3)
        {
            attempt++;
            sent = attempt == 2;                    // pretend the 2nd attempt succeeds
            Console.WriteLine($"Sending email, attempt {attempt}: {(sent ? "ok" : "failed")}");
        }

        // --- continue (skip this item) and break (stop the loop) ---
        foreach (var price in prices)
        {
            if (price < 1000)
                continue;
            if (price > 10_000)
            {
                Console.WriteLine($"High-value item {price} needs manual approval, stopping");
                break;
            }
            Console.WriteLine($"Auto-approved item: {price}");
        }

        // --- is / as: safe type checks ---
        object payload = "ORD-1042";
        if (payload is string orderNumber)          // "if it's a string, call it orderNumber"
            Console.WriteLine($"payload is a string: {orderNumber}");
        var asList = payload as List<string>;       // 'as' gives null instead of throwing
        Console.WriteLine($"payload as List<string> = {(asList is null ? "null" : "a list")}");
    }
}
