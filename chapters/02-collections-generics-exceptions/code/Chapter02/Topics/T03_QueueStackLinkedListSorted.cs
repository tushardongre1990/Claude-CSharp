namespace Chapter02.Topics;

public static class T03_QueueStackLinkedListSorted
{
    private record EmailNotification(string To, string Subject);
    private record CartAction(string Description, string Sku, int Quantity);
    private enum ShippingTier { Express = 1, Standard = 2, Economy = 3 }

    // A tiny "recently viewed products" list: newest first, keeps the last N.
    // LinkedList + Dictionary gives O(1) "move to front" and O(1) lookup.
    private class RecentlyViewed(int capacity)
    {
        private readonly LinkedList<string> _order = new();
        private readonly Dictionary<string, LinkedListNode<string>> _nodes = new();

        public void View(string sku)
        {
            if (_nodes.TryGetValue(sku, out var existing))
            {
                _order.Remove(existing);          // O(1): we already have the node
                _order.AddFirst(existing);
                return;
            }
            _nodes[sku] = _order.AddFirst(sku);
            if (_order.Count > capacity)
            {
                _nodes.Remove(_order.Last!.Value);
                _order.RemoveLast();              // O(1)
            }
        }

        public override string ToString() => string.Join(", ", _order);
    }

    public static void Run()
    {
        // ---- Queue<T>: first in, first out (FIFO) ----
        var outbox = new Queue<EmailNotification>();
        outbox.Enqueue(new("asha@shop.com", "Order #1 confirmed"));
        outbox.Enqueue(new("ravi@shop.com", "Order #2 confirmed"));
        outbox.Enqueue(new("asha@shop.com", "Order #1 shipped"));
        Console.WriteLine($"Next email (Peek, not removed): {outbox.Peek().Subject}");
        while (outbox.TryDequeue(out var email))
            Console.WriteLine($"  sending: {email.Subject} -> {email.To}");

        try { outbox.Dequeue(); }
        catch (InvalidOperationException) { Console.WriteLine("Dequeue on empty queue: InvalidOperationException (use TryDequeue)"); }

        // ---- Stack<T>: last in, first out (LIFO). Undo in a shopping cart ----
        var undo = new Stack<CartAction>();
        undo.Push(new("Add", "SKU-1", 1));
        undo.Push(new("Add", "SKU-2", 2));
        undo.Push(new("Remove", "SKU-1", 1));
        var last = undo.Pop();
        Console.WriteLine($"Undo: '{last.Description} {last.Sku}'. Next undo would be '{undo.Peek().Description} {undo.Peek().Sku}'");

        // ---- LinkedList<T>: O(1) insert/remove when you already hold the node ----
        var recent = new RecentlyViewed(capacity: 3);
        foreach (var sku in new[] { "Laptop", "Mouse", "Keyboard", "Mouse", "Monitor" })
            recent.View(sku);
        Console.WriteLine($"Recently viewed (newest first, max 3): {recent}");

        // ---- SortedDictionary<TKey,TValue>: keys always kept in sorted order ----
        var dailySales = new SortedDictionary<DateOnly, decimal>
        {
            [new DateOnly(2026, 10, 3)] = 18_000m,
            [new DateOnly(2026, 10, 1)] = 12_500m,
            [new DateOnly(2026, 10, 2)] = 9_750m,
        };
        Console.WriteLine("Daily sales report (inserted out of order, printed sorted):");
        foreach (var (day, total) in dailySales)
            Console.WriteLine($"  {day:yyyy-MM-dd}: {total,8:N0}");

        // ---- PriorityQueue<TElement,TPriority> (.NET 6+): smallest priority comes out first ----
        var dispatch = new PriorityQueue<string, ShippingTier>();
        dispatch.Enqueue("Order #11", ShippingTier.Economy);
        dispatch.Enqueue("Order #12", ShippingTier.Express);
        dispatch.Enqueue("Order #13", ShippingTier.Standard);
        dispatch.Enqueue("Order #14", ShippingTier.Express);
        Console.Write("Packing order:");
        while (dispatch.TryDequeue(out var order, out var tier))
            Console.Write($" {order}({tier})");
        Console.WriteLine();

        // Equal priorities do NOT come out in insertion order (a heap is not "stable")
        var sameTier = new PriorityQueue<string, ShippingTier>();
        for (int i = 1; i <= 6; i++) sameTier.Enqueue($"#{i}", ShippingTier.Standard);
        Console.Write("Six Standard orders, inserted #1..#6, come out as:");
        while (sameTier.TryDequeue(out var order, out _)) Console.Write($" {order}");
        Console.WriteLine();

        // Fix: add a sequence number as a tie-breaker, so the priority is (tier, arrival order)
        var fair = new PriorityQueue<string, (ShippingTier Tier, long Seq)>();
        long seq = 0;
        for (int i = 1; i <= 6; i++) fair.Enqueue($"#{i}", (ShippingTier.Standard, seq++));
        Console.Write("With (tier, sequence) as the priority:");
        while (fair.TryDequeue(out var order, out _)) Console.Write($" {order}");
        Console.WriteLine();
    }
}
