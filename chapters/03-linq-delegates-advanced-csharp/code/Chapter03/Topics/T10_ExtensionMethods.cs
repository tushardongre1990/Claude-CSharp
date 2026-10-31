using System.Globalization;

namespace Chapter03.Topics;

public record Invoice(string Number, string Customer, decimal Amount, DateOnly DueDate, bool IsPaid)
{
    // An instance method with the same name as an extension method below
    public string Describe() => $"Invoice {Number} (instance method)";
}

// Extension methods live in a static, non-generic, top-level class
public static class InvoiceExtensions
{
    // 'this Invoice invoice' makes it callable as invoice.IsOverdue(today)
    public static bool IsOverdue(this Invoice invoice, DateOnly today) =>
        !invoice.IsPaid && invoice.DueDate < today;

    // Never called: the instance method Invoice.Describe() always wins
    public static string Describe(this Invoice invoice) => $"Invoice {invoice.Number} (extension method)";
}

public static class MoneyExtensions
{
    private static readonly CultureInfo India = CultureInfo.GetCultureInfo("en-IN");

    public static string ToRupees(this decimal amount) => amount.ToString("C", India);
}

public static class StringExtensions
{
    // Works on null: an extension method is just a static call, so there is no NullReferenceException
    public static string OrDefault(this string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;

    public static string Mask(this string value, int visibleAtEnd = 4) =>
        value.Length <= visibleAtEnd ? value : new string('*', value.Length - visibleAtEnd) + value[^visibleAtEnd..];
}

// Extension methods on interfaces: this is exactly how LINQ itself is built
public static class QueryExtensions
{
    // Apply a filter only when a condition is true, e.g. an optional search field
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool condition, Func<T, bool> predicate) =>
        condition ? source.Where(predicate) : source;

    // IQueryable version keeps the filter translatable to SQL
    public static IQueryable<T> Page<T>(this IQueryable<T> source, int page, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        return source.Skip((page - 1) * pageSize).Take(pageSize);
    }
}

public static class T10_ExtensionMethods
{
    public static void Run()
    {
        var today = new DateOnly(2026, 11, 1);
        var invoices = new List<Invoice>
        {
            new("INV-1001", "Asha", 12_500m, new(2026, 10, 15), IsPaid: false),
            new("INV-1002", "Ravi", 3_200m,  new(2026, 10, 20), IsPaid: true),
            new("INV-1003", "Meera", 845_000.5m, new(2026, 11, 30), IsPaid: false),
        };

        foreach (var inv in invoices)
            Console.WriteLine($"{inv.Number}: {inv.Amount.ToRupees(),15}  overdue: {inv.IsOverdue(today)}");

        // It is only syntax: these two lines compile to the same static call
        bool a = invoices[0].IsOverdue(today);
        bool b = InvoiceExtensions.IsOverdue(invoices[0], today);
        Console.WriteLine($"Extension call == static call: {a == b}");

        // Instance methods win over extension methods with the same signature
        Console.WriteLine(invoices[0].Describe());

        // Null receiver: allowed, because nothing is dereferenced before the method body runs
        string? missingName = null;
        Console.WriteLine($"Customer: {missingName.OrDefault("Guest")}");
        Console.WriteLine($"Card: {"4111111111111111".Mask()}");

        // WhereIf with an optional filter
        string? customerFilter = null;
        var filtered = invoices.WhereIf(customerFilter is not null, i => i.Customer == customerFilter);
        Console.WriteLine($"WhereIf with no filter: {filtered.Count()} invoices");
        customerFilter = "Ravi";
        filtered = invoices.WhereIf(customerFilter is not null, i => i.Customer == customerFilter);
        Console.WriteLine($"WhereIf with filter 'Ravi': {filtered.Count()} invoice");

        // Page<T> on IQueryable (AsQueryable lets us use it on an in-memory list here)
        var pageTwo = invoices.AsQueryable().Page(page: 2, pageSize: 2);
        Console.WriteLine($"Page 2: {string.Join(", ", pageTwo.Select(i => i.Number))}");
        try { invoices.AsQueryable().Page(page: 0, pageSize: 2); }
        catch (ArgumentOutOfRangeException ex) { Console.WriteLine($"Page 0: {ex.GetType().Name} for '{ex.ParamName}'"); }
    }
}
