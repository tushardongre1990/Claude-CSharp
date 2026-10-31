namespace Chapter03.Topics;

public static class T05_GroupingJoinsAggregation
{
    private record Customer(int Id, string Name, string Tier);
    private record Order(int Id, int CustomerId, string City, decimal Total, string Status);
    private record Coupon(string Code, decimal PercentOff);

    private static readonly List<Customer> Customers =
    [
        new(1, "Asha", "Gold"),
        new(2, "Ravi", "Silver"),
        new(3, "Meera", "Gold"),
        new(4, "John", "Silver"),      // John has no orders
    ];

    private static readonly List<Order> Orders =
    [
        new(101, 1, "Pune",  4_100m,  "Paid"),
        new(102, 2, "Delhi", 18_000m, "Cancelled"),
        new(103, 1, "Pune",  800m,    "Paid"),
        new(104, 3, "Delhi", 74_500m, "Paid"),
        new(105, 3, "Delhi", 1_200m,  "Paid"),
        new(106, 9, "Agra",  300m,    "Paid"),  // customer 9 does not exist (bad data)
    ];

    public static void Run()
    {
        // ---- GroupBy: revenue per city ----
        var revenueByCity = Orders
            .Where(o => o.Status == "Paid")
            .GroupBy(o => o.City)
            .Select(g => new { City = g.Key, Orders = g.Count(), Revenue = g.Sum(o => o.Total) })
            .OrderByDescending(x => x.Revenue);
        Console.WriteLine("Revenue by city:");
        foreach (var row in revenueByCity)
            Console.WriteLine($"  {row.City,-6} orders {row.Orders}  revenue {row.Revenue}");

        // Group by two keys with an anonymous type (or a tuple)
        var byCityAndStatus = Orders.GroupBy(o => new { o.City, o.Status })
                                    .Select(g => $"{g.Key.City}/{g.Key.Status}={g.Count()}");
        Console.WriteLine($"By city and status: {string.Join(", ", byCityAndStatus)}");

        // ToLookup: like GroupBy, but runs immediately and lets you look up a key (missing key = empty, no exception)
        ILookup<int, Order> ordersByCustomer = Orders.ToLookup(o => o.CustomerId);
        Console.WriteLine($"Lookup: customer 1 has {ordersByCustomer[1].Count()} orders, customer 4 has {ordersByCustomer[4].Count()}");

        // .NET 9: CountBy and AggregateBy skip building the groups
        var countByStatus = Orders.CountBy(o => o.Status);
        Console.WriteLine($"CountBy status (.NET 9): {string.Join(", ", countByStatus.Select(kv => $"{kv.Key}={kv.Value}"))}");
        var revenueByCustomer = Orders.Where(o => o.Status == "Paid")
                                      .AggregateBy(o => o.CustomerId, seed: 0m, (sum, o) => sum + o.Total);
        Console.WriteLine($"AggregateBy customer (.NET 9): {string.Join(", ", revenueByCustomer.Select(kv => $"{kv.Key}={kv.Value}"))}");

        // ---- Join: inner join. Orders without a matching customer DISAPPEAR. ----
        var joined = Orders.Join(Customers,
                                 order => order.CustomerId,       // key from the left side
                                 customer => customer.Id,         // key from the right side
                                 (order, customer) => $"{order.Id}:{customer.Name}");
        Console.WriteLine($"Inner join: {string.Join(", ", joined)}   (order 106 is gone)");

        // ---- GroupJoin: each customer with ALL their orders (possibly none) ----
        var customerOrders = Customers.GroupJoin(Orders,
                                                 c => c.Id,
                                                 o => o.CustomerId,
                                                 (c, orders) => new { c.Name, Count = orders.Count(), Spent = orders.Sum(o => o.Total) });
        Console.WriteLine("GroupJoin (every customer, even with zero orders):");
        foreach (var c in customerOrders)
            Console.WriteLine($"  {c.Name,-6} orders {c.Count}  spent {c.Spent}");

        // Classic LEFT JOIN: GroupJoin + SelectMany + DefaultIfEmpty
        var leftJoinClassic = Customers
            .GroupJoin(Orders, c => c.Id, o => o.CustomerId, (c, orders) => new { c, orders })
            .SelectMany(x => x.orders.DefaultIfEmpty(), (x, o) => $"{x.c.Name}:{(o is null ? "none" : o.Id.ToString())}");
        Console.WriteLine($"Left join (classic): {string.Join(", ", leftJoinClassic)}");

        // .NET 10: LeftJoin does the same in one call
        var leftJoinNet10 = Customers.LeftJoin(Orders, c => c.Id, o => o.CustomerId,
                                               (c, o) => $"{c.Name}:{(o is null ? "none" : o.Id.ToString())}");
        Console.WriteLine($"Left join (.NET 10):  {string.Join(", ", leftJoinNet10)}");

        // ---- Aggregation ----
        var paid = Orders.Where(o => o.Status == "Paid").ToList();
        Console.WriteLine($"Paid: count {paid.Count}, sum {paid.Sum(o => o.Total)}, avg {paid.Average(o => o.Total):0.00}, min {paid.Min(o => o.Total)}, max {paid.Max(o => o.Total)}");

        // .NET 6: MaxBy / MinBy return the ITEM, not the value
        Order biggest = paid.MaxBy(o => o.Total)!;
        Console.WriteLine($"MaxBy: order {biggest.Id} ({biggest.Total}); Max: {paid.Max(o => o.Total)}");

        // Aggregate: fold a sequence into one value. Apply coupons one after another (compounding).
        var coupons = new List<Coupon> { new("WELCOME10", 10m), new("FESTIVE5", 5m) };
        decimal finalPrice = coupons.Aggregate(1_000m, (price, coupon) => price * (1 - coupon.PercentOff / 100m));
        Console.WriteLine($"1000 after compounding coupons: {finalPrice}");

        // ---- Empty sequences: Sum is safe, Average/Max throw ----
        var noOrders = new List<Order>();
        Console.WriteLine($"Empty Sum: {noOrders.Sum(o => o.Total)}");
        try { noOrders.Average(o => o.Total); }
        catch (InvalidOperationException ex) { Console.WriteLine($"Empty Average: {ex.GetType().Name}: {ex.Message}"); }
        // Fix: project to a nullable type. Max/Average of an empty nullable sequence return null.
        decimal? maxOrNull = noOrders.Max(o => (decimal?)o.Total);
        Console.WriteLine($"Empty Max as decimal?: {(maxOrNull is null ? "null" : maxOrNull)}");
        try { coupons.Take(0).Aggregate((a, b) => a); }
        catch (InvalidOperationException ex) { Console.WriteLine($"Aggregate without seed on empty: {ex.GetType().Name}"); }
    }
}
