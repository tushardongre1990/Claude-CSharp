namespace Chapter03.Topics;

// ---- C# 14 extension members: an 'extension block' can add properties and static members ----
public static class OrderTotalsExtensions
{
    extension(IEnumerable<decimal> amounts)
    {
        // An extension PROPERTY (impossible with classic 'this' extension methods)
        public decimal GrandTotal => amounts.Sum();

        // Extension methods can live in the block too
        public IEnumerable<decimal> AboveThreshold(decimal threshold) => amounts.Where(a => a > threshold);
    }

    extension(decimal)
    {
        // A static extension member: called as decimal.FromPaise(...)
        public static decimal FromPaise(long paise) => paise / 100m;
    }
}

public static class T16_NewestCSharp
{
    // ---- C# 13: params with collection types other than arrays ----
    // The compiler can store the caller's arguments in stack or other optimized storage, which usually avoids
    // the heap array that 'params string[]' needs (a compiler implementation choice, not a guarantee).
    // string.Join has a ReadOnlySpan overload since .NET 9.
    private static string Tags(params ReadOnlySpan<string?> tags) => string.Join(",", tags);
    private static int CountNonEmpty(params IEnumerable<string?> skus) => skus.Count(s => !string.IsNullOrWhiteSpace(s));

    // ---- .NET 9 / C# 13: System.Threading.Lock ----
    private sealed class InvoiceNumberGenerator
    {
        private readonly Lock _lock = new();                  // a dedicated lock type, instead of 'new object()'
        private int _next = 1;

        public int Next()
        {
            lock (_lock)                                       // the compiler uses Lock.EnterScope() here
                return _next++;
        }
    }

    // ---- C# 14: the 'field' keyword ----
    private sealed class Customer
    {
        // Before C# 14 you needed a private _email field just to trim/lowercase in the setter.
        public string Email
        {
            get;
            set => field = value?.Trim().ToLowerInvariant() ?? throw new ArgumentNullException(nameof(value));
        } = "";

        public DateTimeOffset? LastOrderAt { get; set; }
    }

    private sealed class Cart
    {
        public Customer? Owner { get; set; }
        public List<string>? AppliedCoupons { get; set; }
    }

    public static void Run()
    {
        // params collections
        Console.WriteLine($"Tags: {Tags("urgent", "fragile", "cod")}");
        Console.WriteLine($"Non-empty SKUs: {CountNonEmpty("KB-01", "", null, "MS-02")}");
        Console.WriteLine($"Tags with no arguments: '{Tags()}'");

        // Lock
        var generator = new InvoiceNumberGenerator();
        var numbers = new System.Collections.Concurrent.ConcurrentBag<int>();
        Parallel.For(0, 1_000, _ => numbers.Add(generator.Next()));
        Console.WriteLine($"1000 parallel calls: {numbers.Distinct().Count()} unique numbers, max {numbers.Max()}");

        // field keyword
        var customer = new Customer { Email = "  Asha@Shop.IN " };
        Console.WriteLine($"Email stored as: '{customer.Email}'");

        // Null-conditional assignment (C# 14): assign only if the left side's target is not null
        var guestCart = new Cart { Owner = null };
        var memberCart = new Cart { Owner = customer, AppliedCoupons = [] };
        var now = new DateTimeOffset(2026, 11, 1, 10, 30, 0, TimeSpan.FromHours(5.5));

        guestCart.Owner?.LastOrderAt = now;                    // Owner is null: nothing happens, no exception
        memberCart.Owner?.LastOrderAt = now;                   // Owner exists: assigned
        memberCart.AppliedCoupons?.Add("WELCOME10");
        Console.WriteLine($"Guest owner is null: {guestCart.Owner is null}; member LastOrderAt: {memberCart.Owner!.LastOrderAt:yyyy-MM-dd HH:mm zzz}");
        Console.WriteLine($"Member coupons: {string.Join(", ", memberCart.AppliedCoupons!)}");

        // Extension members
        decimal[] paymentAmounts = [1_200m, 450m, 9_999m];
        Console.WriteLine($"Extension property GrandTotal: {paymentAmounts.GrandTotal}");
        Console.WriteLine($"Extension method AboveThreshold(1000): {string.Join(", ", paymentAmounts.AboveThreshold(1_000m))}");
        Console.WriteLine($"Static extension decimal.FromPaise(129950): {decimal.FromPaise(129_950)}");
    }
}
