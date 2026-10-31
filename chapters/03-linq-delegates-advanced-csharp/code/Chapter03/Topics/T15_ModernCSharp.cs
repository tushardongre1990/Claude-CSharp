using System.Diagnostics.CodeAnalysis;

namespace Chapter03.Topics;   // file-scoped namespace (C# 10): no braces, one less indentation level

public static class T15_ModernCSharp
{
    public interface ITaxCalculator { decimal TaxFor(decimal amount); }
    public sealed class GstCalculator : ITaxCalculator { public decimal TaxFor(decimal amount) => amount * 0.18m; }

    // ---- Primary constructor on a class (C# 12): typical DI-style service ----
    public sealed class InvoiceService(ITaxCalculator taxCalculator, string currency)
    {
        // The parameters are in scope in the whole class body
        public string Format(decimal amount) =>
            $"{amount + taxCalculator.TaxFor(amount):0.00} {currency} (incl. tax)";
    }

    // GOTCHA: primary constructor parameters are NOT readonly fields. Any member can reassign them.
    public sealed class RetryPolicy(int maxAttempts)
    {
        public bool ShouldRetry(int attempt) => attempt < maxAttempts;
        public void Misconfigure() => maxAttempts = 0;          // compiles! silently changes behaviour
    }

    // FIX: copy into a readonly field (or a get-only property) once
    public sealed class SafeRetryPolicy(int maxAttempts)
    {
        private readonly int _maxAttempts = maxAttempts > 0
            ? maxAttempts
            : throw new ArgumentOutOfRangeException(nameof(maxAttempts));

        public bool ShouldRetry(int attempt) => attempt < _maxAttempts;
        // public void Misconfigure() => _maxAttempts = 0;      // CS0191: readonly field can't be assigned
    }

    // ---- record vs class primary constructors ----
    // record: the compiler creates public init properties, equality, ToString, Deconstruct, with-expressions
    public sealed record ProductDto(string Sku, string Name, decimal Price);

    // ---- required and init members (C# 11 / C# 9) ----
    public sealed class CreateOrderRequest
    {
        public required string CustomerEmail { get; init; }    // must be set in the object initializer
        public required List<string> Skus { get; init; }
        public string? CouponCode { get; init; }               // optional
        public DateTimeOffset RequestedAt { get; init; } = DateTimeOffset.UnixEpoch;

        public CreateOrderRequest() { }

        [SetsRequiredMembers]                                  // tells the compiler this constructor sets them all
        public CreateOrderRequest(string email, List<string> skus) => (CustomerEmail, Skus) = (email, skus);
    }

    public static void Run()
    {
        // ---- Primary constructors ----
        var invoices = new InvoiceService(new GstCalculator(), "INR");
        Console.WriteLine($"Invoice: {invoices.Format(1_000m)}");

        var policy = new RetryPolicy(3);
        Console.WriteLine($"RetryPolicy: retry attempt 2? {policy.ShouldRetry(2)}");
        policy.Misconfigure();
        Console.WriteLine($"After Misconfigure(): retry attempt 2? {policy.ShouldRetry(2)}  <- parameter was mutated");

        var safe = new SafeRetryPolicy(3);
        Console.WriteLine($"SafeRetryPolicy: retry attempt 2? {safe.ShouldRetry(2)}");
        try { _ = new SafeRetryPolicy(0); }
        catch (ArgumentOutOfRangeException ex) { Console.WriteLine($"SafeRetryPolicy(0): {ex.GetType().Name} for '{ex.ParamName}'"); }

        var dto = new ProductDto("KB-01", "Keyboard", 2_500m);
        var discounted = dto with { Price = 1_999m };          // non-destructive copy
        Console.WriteLine($"Record: {dto}");
        Console.WriteLine($"With:   {discounted}; equal by value to a new copy? {dto == new ProductDto("KB-01", "Keyboard", 2_500m)}");

        // ---- required / init ----
        var request = new CreateOrderRequest { CustomerEmail = "asha@shop.in", Skus = ["KB-01", "MS-02"] };
        // var broken = new CreateOrderRequest { Skus = [] };   // CS9035: required member 'CustomerEmail' must be set
        // request.CustomerEmail = "x@y.z";                      // CS8852: init-only property can't be assigned here
        Console.WriteLine($"Request: {request.CustomerEmail}, {request.Skus.Count} SKUs, coupon {request.CouponCode ?? "none"}");
        var viaCtor = new CreateOrderRequest("ravi@shop.in", ["LAP-14"]);
        Console.WriteLine($"Via [SetsRequiredMembers] constructor: {viaCtor.CustomerEmail}");

        // Note: 'required' is a compile-time check only. A JSON deserializer can still leave it null
        // unless the serializer enforces it (System.Text.Json does enforce 'required' since .NET 7).
        try { JsonSerializer.Deserialize<CreateOrderRequest>("""{ "Skus": ["KB-01"] }"""); }   // JsonSerializer comes from GlobalUsings.cs
        catch (JsonException ex) { Console.WriteLine($"System.Text.Json and missing required: {ex.Message}"); }

        // ---- Collection expressions (C# 12) and the spread operator .. ----
        string[] baseRoles = ["Customer"];
        string[] staffRoles = [.. baseRoles, "Support"];
        List<string> adminRoles = [.. staffRoles, "Admin"];
        HashSet<string> uniqueRoles = [.. adminRoles, "Admin", "Customer"];   // duplicates dropped by the set
        int[] empty = [];
        Console.WriteLine($"Roles: staff [{string.Join(", ", staffRoles)}], admin [{string.Join(", ", adminRoles)}], unique {uniqueRoles.Count}, empty {empty.Length}");

        // ---- Raw string literals (C# 11): JSON or SQL without escaping ----
        string sql = """
            SELECT Id, Total
            FROM Orders
            WHERE Status = 'Paid'
            """;
        Console.WriteLine($"Raw SQL has {sql.Split('\n').Length} lines, first: {sql.Split('\n')[0]}");
    }
}
