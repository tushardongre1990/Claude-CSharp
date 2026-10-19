namespace Chapter02.Topics;

public static class T01_ArraysAndLists
{
    private record Order(int Id, string Status, decimal Total);

    public static void Run()
    {
        // ---- Arrays: fixed size, chosen at creation ----
        decimal[] monthlyRevenue = new decimal[12];   // 12 slots, all start at 0
        monthlyRevenue[0] = 125_000m;                  // January (index 0)
        monthlyRevenue[11] = 310_500m;                 // December (index 11)
        Console.WriteLine($"Months: {monthlyRevenue.Length}, Jan: {monthlyRevenue[0]}, Feb: {monthlyRevenue[1]}, Dec: {monthlyRevenue[11]}");

        try
        {
            monthlyRevenue[12] = 1m;                   // there is no 13th month
        }
        catch (IndexOutOfRangeException ex)
        {
            Console.WriteLine($"Array index 12: {ex.GetType().Name}");
        }

        // Arrays are reference types: assignment copies the reference, not the data
        decimal[] sameArray = monthlyRevenue;
        sameArray[1] = 99m;
        Console.WriteLine($"After changing sameArray[1], monthlyRevenue[1] = {monthlyRevenue[1]}");

        // ---- List<T>: grows automatically ----
        var orderIds = new List<int>();
        var capacities = new List<int> { orderIds.Capacity };
        for (int i = 1; i <= 17; i++)
        {
            orderIds.Add(i);
            if (orderIds.Capacity != capacities[^1]) capacities.Add(orderIds.Capacity);
        }
        Console.WriteLine($"Count: {orderIds.Count}, capacity changes: {string.Join(" -> ", capacities)}");

        // If you know the size, set the capacity up front: one allocation, no copying
        var preSized = new List<int>(capacity: 10_000);
        Console.WriteLine($"Pre-sized list: Count {preSized.Count}, Capacity {preSized.Capacity}");

        // ---- Common bug: removing items inside foreach ----
        var orders = new List<Order>
        {
            new(1, "Paid", 500m),
            new(2, "Cancelled", 120m),
            new(3, "Paid", 75m),
            new(4, "Cancelled", 60m),
        };

        try
        {
            foreach (var order in orders)
                if (order.Status == "Cancelled")
                    orders.Remove(order);              // changes the list while foreach is reading it
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Remove inside foreach: {ex.GetType().Name}: {ex.Message}");
        }

        // Fix 1: RemoveAll (one pass, no exception)
        orders.Add(new(5, "Cancelled", 10m));
        int removed = orders.RemoveAll(o => o.Status == "Cancelled");
        Console.WriteLine($"RemoveAll removed {removed}; left: {string.Join(", ", orders.Select(o => o.Id))}");

        // Fix 2: loop backwards with an index (removing does not shift the items you have not visited yet)
        var invoiceIds = new List<int> { 10, 11, 12, 13, 14 };
        for (int i = invoiceIds.Count - 1; i >= 0; i--)
            if (invoiceIds[i] % 2 == 0) invoiceIds.RemoveAt(i);
        Console.WriteLine($"Odd invoice ids left: {string.Join(", ", invoiceIds)}");

        // ---- Useful List<T> members ----
        var prices = new List<decimal> { 499m, 99m, 1299m, 99m };
        Console.WriteLine($"IndexOf(99): {prices.IndexOf(99m)}, Contains(1299): {prices.Contains(1299m)}, Find(>1000): {prices.Find(p => p > 1000m)}");
        prices.Sort();
        Console.WriteLine($"Sorted: {string.Join(", ", prices)}, BinarySearch(499): index {prices.BinarySearch(499m)}");

        // Collection expressions (C# 12): one syntax for arrays and lists
        int[] warehouseIds = [101, 102, 103];
        List<string> currencies = ["INR", "USD", "EUR"];
        Console.WriteLine($"Warehouses: {warehouseIds.Length}, currencies: {string.Join("/", currencies)}");

        // Empty list vs null: return an empty list, not null, so callers can just loop
        List<Order> noOrders = [];
        Console.WriteLine($"Empty list count: {noOrders.Count}");
    }
}
