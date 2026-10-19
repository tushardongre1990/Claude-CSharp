using System.Collections.ObjectModel;

namespace Chapter02.Topics;

public static class T04_CollectionInterfaces
{
    private record OrderLine(string Sku, int Quantity, decimal UnitPrice);

    // BAD: exposes the real List, so any caller can change the order behind its back
    private class LeakyOrder
    {
        public List<OrderLine> Lines { get; } = new();
    }

    // GOOD: the list is private; callers get a read-only view and must use AddLine
    private class Order
    {
        private readonly List<OrderLine> _lines = new();
        public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();   // a ReadOnlyCollection wrapper
        public decimal Total => _lines.Sum(l => l.Quantity * l.UnitPrice);

        public void AddLine(OrderLine line)
        {
            if (line.Quantity <= 0) throw new ArgumentOutOfRangeException(nameof(line), "Quantity must be positive.");
            _lines.Add(line);
        }
    }

    private static int _databaseCalls;

    // Pretends to stream invoices from a database. 'yield return' makes it lazy:
    // nothing runs until someone loops over the result, and it runs again on every loop.
    private static IEnumerable<decimal> StreamInvoiceTotals()
    {
        _databaseCalls++;
        yield return 1_000m;
        yield return 2_500m;
        yield return 400m;
    }

    // Accept the smallest interface you need: any collection (array, list, set, query) works
    private static decimal SumTotals(IEnumerable<decimal> totals)
    {
        decimal sum = 0;
        foreach (var t in totals) sum += t;
        return sum;
    }

    public static void Run()
    {
        decimal[] fromArray = [100m, 200m];
        List<decimal> fromList = [300m];
        HashSet<decimal> fromSet = [400m, 500m];
        Console.WriteLine($"SumTotals works with array {SumTotals(fromArray)}, list {SumTotals(fromList)}, set {SumTotals(fromSet)}");

        // ---- Leaky vs encapsulated ----
        var leaky = new LeakyOrder();
        leaky.Lines.Add(new("SKU-1", -5, 100m));        // nothing stops a negative quantity
        Console.WriteLine($"LeakyOrder accepted quantity {leaky.Lines[0].Quantity}");

        var order = new Order();
        order.AddLine(new("SKU-1", 2, 250m));
        IReadOnlyList<OrderLine> view = order.Lines;
        Console.WriteLine($"Order: {view.Count} line(s), first SKU {view[0].Sku}, total {order.Total}");

        // A caller tries to cheat by casting the read-only view back to a list
        if (order.Lines is IList<OrderLine> asList)
        {
            try { asList.Add(new("SKU-FREE", 1, 0m)); }
            catch (NotSupportedException) { Console.WriteLine("Cast to IList and Add: NotSupportedException (AsReadOnly wrapper blocks it)"); }
        }

        // If you return the List itself typed as IReadOnlyList, a cast gets the real list back
        List<OrderLine> realList = [new("SKU-1", 1, 10m)];
        IReadOnlyList<OrderLine> weakView = realList;
        ((List<OrderLine>)weakView).Add(new("SKU-HACK", 1, 0m));
        Console.WriteLine($"Plain List typed as IReadOnlyList: after cast + Add, Count = {realList.Count}");

        // ---- Arrays implement IList<T>, but Add is not supported ----
        IList<string> currencies = new[] { "INR", "USD" };
        try { currencies.Add("EUR"); }
        catch (NotSupportedException) { Console.WriteLine("Array as IList<T>.Add: NotSupportedException (fixed size)"); }

        // ---- Multiple enumeration of a lazy IEnumerable ----
        _databaseCalls = 0;
        IEnumerable<decimal> invoices = StreamInvoiceTotals();
        Console.WriteLine($"After calling StreamInvoiceTotals(): database calls = {_databaseCalls} (nothing ran yet)");
        decimal total = invoices.Sum();
        int count = invoices.Count();
        Console.WriteLine($"Sum {total}, Count {count}: database calls = {_databaseCalls} (ran twice!)");

        _databaseCalls = 0;
        List<decimal> materialized = StreamInvoiceTotals().ToList();
        Console.WriteLine($"With ToList() first: Sum {materialized.Sum()}, Count {materialized.Count}, database calls = {_databaseCalls}");

        // ---- ReadOnlyCollection is a view, not a copy ----
        var source = new List<string> { "Pending" };
        ReadOnlyCollection<string> readOnlyView = source.AsReadOnly();
        source.Add("Shipped");
        Console.WriteLine($"Read-only view sees later changes to the source: {string.Join(", ", readOnlyView)}");
    }
}
