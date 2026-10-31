namespace Chapter03.Topics;

public static class T04_LinqBasics
{
    private record OrderLine(string Sku, int Quantity, decimal UnitPrice);
    private record Order(int Id, string Customer, string City, string Status, DateOnly PlacedOn, List<OrderLine> Lines)
    {
        public decimal Total => Lines.Sum(l => l.Quantity * l.UnitPrice);
    }

    private static readonly List<Order> Orders =
    [
        new(1, "Asha",  "Pune",  "Paid",      new(2026, 10, 1), [new("KB-01", 1, 2_500m), new("MS-02", 2, 800m)]),
        new(2, "Ravi",  "Delhi", "Cancelled", new(2026, 10, 2), [new("MON-27", 1, 18_000m)]),
        new(3, "Asha",  "Pune",  "Paid",      new(2026, 10, 5), [new("MS-02", 1, 800m)]),
        new(4, "Meera", "Delhi", "Paid",      new(2026, 10, 5), [new("LAP-14", 1, 72_000m), new("KB-01", 1, 2_500m)]),
        new(5, "John",  "Pune",  "Pending",   new(2026, 10, 7), []),
    ];

    public static void Run()
    {
        // ---- Where: filter ----
        var paidOrders = Orders.Where(o => o.Status == "Paid");
        Console.WriteLine($"Paid orders: {string.Join(", ", paidOrders.Select(o => o.Id))}");

        // ---- Select: transform each item (here into a DTO-like anonymous object) ----
        var summaries = Orders
            .Where(o => o.Status == "Paid")
            .Select(o => new { o.Id, o.Customer, Total = o.Total });
        foreach (var s in summaries)
            Console.WriteLine($"  #{s.Id} {s.Customer,-6} {s.Total,8}");

        // Select with index: number the rows of a report
        var numbered = Orders.Select((o, index) => $"{index + 1}. order {o.Id}");
        Console.WriteLine($"Numbered: {string.Join(" | ", numbered)}");

        // ---- Select vs SelectMany ----
        IEnumerable<List<OrderLine>> nested = Orders.Select(o => o.Lines);     // a sequence of LISTS
        IEnumerable<OrderLine> flat = Orders.SelectMany(o => o.Lines);         // ONE flat sequence of lines
        Console.WriteLine($"Select gives {nested.Count()} lists; SelectMany gives {flat.Count()} lines");

        // Which SKUs were sold (in paid orders), without duplicates?
        var soldSkus = Orders.Where(o => o.Status == "Paid")
                             .SelectMany(o => o.Lines)
                             .Select(l => l.Sku)
                             .Distinct();
        Console.WriteLine($"Distinct SKUs sold: {string.Join(", ", soldSkus)}");

        // SelectMany with a result selector keeps the parent: (order id, sku) pairs for a CSV export
        var exportRows = Orders.SelectMany(o => o.Lines, (o, line) => $"{o.Id}:{line.Sku}");
        Console.WriteLine($"Export rows: {string.Join(", ", exportRows)}");

        // ---- OrderBy / ThenBy ----
        var sorted = Orders.OrderBy(o => o.City)
                           .ThenByDescending(o => o.Total);
        Console.WriteLine("Sorted by city, then total (highest first):");
        foreach (var o in sorted)
            Console.WriteLine($"  {o.City,-6} #{o.Id} {o.Total,8}");

        // Mistake: two OrderBy calls. The second one becomes the NEW primary sort; because OrderBy is stable,
        // the first survives only as a tie-breaker. So this equals OrderBy(Id).ThenBy(City), the reverse of the intent.
        // Intended: by city, then by id. Got: by id first, cities mixed up.
        var wrong = Orders.OrderBy(o => o.City).OrderBy(o => o.Id);
        var right = Orders.OrderBy(o => o.City).ThenBy(o => o.Id);
        Console.WriteLine($"OrderBy + OrderBy (wrong): {string.Join(", ", wrong.Select(o => $"{o.City}#{o.Id}"))}");
        Console.WriteLine($"OrderBy + ThenBy (right):  {string.Join(", ", right.Select(o => $"{o.City}#{o.Id}"))}");

        // OrderBy is stable: orders 3 and 4 share a date and keep their original order
        var byDate = Orders.OrderBy(o => o.PlacedOn).Select(o => $"{o.PlacedOn:MM-dd}#{o.Id}");
        Console.WriteLine($"By date (stable): {string.Join(", ", byDate)}");

        // .NET 7: Order() / OrderDescending() for simple values
        var amounts = Orders.Select(o => o.Total).OrderDescending();
        Console.WriteLine($"Totals, highest first: {string.Join(", ", amounts)}");

        // ---- Paging: Skip / Take (how page 2 of an order list works) ----
        int page = 2, pageSize = 2;
        var pageTwo = Orders.OrderBy(o => o.Id).Skip((page - 1) * pageSize).Take(pageSize);
        Console.WriteLine($"Page {page} (size {pageSize}): {string.Join(", ", pageTwo.Select(o => o.Id))}");

        // .NET 6: Chunk splits into batches, e.g. send emails 2 at a time
        foreach (var batch in Orders.Select(o => o.Id).Chunk(2))
            Console.WriteLine($"  Email batch: [{string.Join(", ", batch)}]");

        // ---- Query syntax: the same query, SQL-like form. The compiler turns it into method calls. ----
        var delhiPaid = from o in Orders
                        where o.City == "Delhi" && o.Status == "Paid"
                        orderby o.Total descending
                        select new { o.Id, o.Total };
        Console.WriteLine($"Query syntax, Delhi paid: {string.Join(", ", delhiPaid)}");
    }
}
