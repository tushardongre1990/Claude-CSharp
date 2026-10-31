namespace Chapter03.Topics;

public static class T12_PatternMatching
{
    // Payment types for type patterns
    private abstract record Payment(decimal Amount);
    private sealed record CardPayment(decimal Amount, string Network, bool IsInternational) : Payment(Amount);
    private sealed record UpiPayment(decimal Amount, string Vpa) : Payment(Amount);
    private sealed record WalletPayment(decimal Amount, string Provider) : Payment(Amount);
    private sealed record GiftCardPayment(decimal Amount) : Payment(Amount);

    private record Address(string Country, string City);
    private record Parcel(decimal WeightKg, Address Destination, bool IsFragile);

    private enum OrderState { Created, Paid, Shipped, Delivered, Cancelled }

    // ---- Type patterns + property patterns + relational patterns ----
    private static decimal GatewayFee(Payment payment) => payment switch
    {
        CardPayment { IsInternational: true } c => c.Amount * 0.035m,
        CardPayment { Network: "Amex" } c => c.Amount * 0.025m,
        CardPayment c => c.Amount * 0.018m,
        UpiPayment { Amount: <= 2_000m } => 0m,                 // small UPI payments are free
        UpiPayment u => u.Amount * 0.003m,
        WalletPayment w => w.Amount * 0.01m,
        null => throw new ArgumentNullException(nameof(payment)),
        _ => throw new NotSupportedException($"No fee rule for {payment.GetType().Name}"),
    };

    // ---- Nested property patterns, 'and' / 'or', relational patterns ----
    private static decimal ShippingCost(Parcel parcel) => parcel switch
    {
        { WeightKg: <= 0 } => throw new ArgumentOutOfRangeException(nameof(parcel), "Weight must be positive"),
        { Destination.Country: not "IN" } => 2_500m,                       // extended property pattern (C# 10)
        { Destination.City: "Mumbai" or "Delhi", WeightKg: < 1 } => 40m,
        { WeightKg: < 1 } => 60m,
        { WeightKg: >= 1 and < 5, IsFragile: true } => 180m,
        { WeightKg: >= 1 and < 5 } => 120m,
        _ => 120m + (parcel.WeightKg - 5) * 20m,
    };

    // ---- Tuple patterns: a tiny order state machine ----
    private static OrderState Transition(OrderState state, string action) => (state, action) switch
    {
        (OrderState.Created, "pay") => OrderState.Paid,
        (OrderState.Paid, "ship") => OrderState.Shipped,
        (OrderState.Shipped, "deliver") => OrderState.Delivered,
        (OrderState.Created or OrderState.Paid, "cancel") => OrderState.Cancelled,
        _ => throw new InvalidOperationException($"Cannot '{action}' an order that is {state}"),
    };

    // ---- List patterns (C# 11): parse an admin command split into words ----
    private static string HandleCommand(string[] words) => words switch
    {
        [] => "Empty command",
        ["refund", var orderId] => $"Full refund for order {orderId}",
        ["refund", var orderId, var amount] => $"Partial refund of {amount} for order {orderId}",
        ["resend", "invoice", .. var orderIds] when orderIds.Length > 0 => $"Resend invoices for {string.Join(", ", orderIds)}",
        [var verb, ..] => $"Unknown command '{verb}'",
    };

    public static void Run()
    {
        // ---- 'is' patterns ----
        object incoming = new UpiPayment(1_500m, "asha@okbank");
        if (incoming is UpiPayment { Vpa: var vpa } upi)              // type test + cast + read a property
            Console.WriteLine($"UPI payment of {upi.Amount} from {vpa}");

        string? couponCode = null;
        Console.WriteLine($"Coupon is not null: {couponCode is not null}");   // 'is not null' can't be fooled by an overloaded !=

        int httpStatus = 503;
        bool shouldRetry = httpStatus is 408 or 429 or (>= 500 and <= 599);
        Console.WriteLine($"Retry on {httpStatus}? {shouldRetry}");

        // ---- switch expression over types ----
        Payment[] payments =
        [
            new CardPayment(10_000m, "Visa", IsInternational: true),
            new CardPayment(10_000m, "Amex", IsInternational: false),
            new CardPayment(10_000m, "Visa", IsInternational: false),
            new UpiPayment(1_500m, "ravi@okbank"),
            new UpiPayment(10_000m, "ravi@okbank"),
            new WalletPayment(10_000m, "Paytm"),
        ];
        foreach (var p in payments)
            Console.WriteLine($"  {p.GetType().Name,-14} {p.Amount,7}  fee {GatewayFee(p)}");
        try { GatewayFee(new GiftCardPayment(500m)); }
        catch (NotSupportedException ex) { Console.WriteLine($"  {ex.Message}"); }

        // ---- property and relational patterns ----
        var parcels = new[]
        {
            new Parcel(0.5m, new("IN", "Mumbai"), false),
            new Parcel(0.5m, new("IN", "Pune"), false),
            new Parcel(3m,   new("IN", "Pune"), true),
            new Parcel(8m,   new("IN", "Pune"), false),
            new Parcel(2m,   new("US", "Austin"), false),
        };
        foreach (var parcel in parcels)
            Console.WriteLine($"  {parcel.WeightKg}kg to {parcel.Destination.City,-7} fragile={parcel.IsFragile,-5} => {ShippingCost(parcel)}");
        try { ShippingCost(new Parcel(-1m, new("IN", "Pune"), false)); }
        catch (ArgumentOutOfRangeException) { Console.WriteLine("  Negative weight rejected"); }

        // ---- tuple patterns ----
        var state = OrderState.Created;
        foreach (var action in new[] { "pay", "ship", "deliver" })
        {
            state = Transition(state, action);
            Console.Write($"{action} -> {state}; ");
        }
        Console.WriteLine();
        try { Transition(OrderState.Shipped, "cancel"); }
        catch (InvalidOperationException ex) { Console.WriteLine(ex.Message); }

        // ---- list patterns ----
        string[] commands = ["refund 1042", "refund 1042 500", "resend invoice 1 2 3", "resend invoice", "ship 7", ""];
        foreach (var c in commands)
            Console.WriteLine($"  '{c}' => {HandleCommand(c.Split(' ', StringSplitOptions.RemoveEmptyEntries))}");
    }
}
