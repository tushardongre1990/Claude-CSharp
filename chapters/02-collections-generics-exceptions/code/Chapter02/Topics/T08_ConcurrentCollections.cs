using System.Collections.Concurrent;

namespace Chapter02.Topics;

public static class T08_ConcurrentCollections
{
    private record Shipment(int OrderId, string Carrier);

    public static void Run()
    {
        // ---- ConcurrentDictionary: count product views coming from many requests at once ----
        string[] products = ["Laptop", "Mouse", "Keyboard"];
        var views = new ConcurrentDictionary<string, int>();
        Parallel.For(0, 30_000, i =>
        {
            string sku = products[i % products.Length];
            views.AddOrUpdate(sku, addValue: 1, updateValueFactory: (_, current) => current + 1);   // atomic per key
        });
        Console.WriteLine($"Views: {string.Join(", ", views.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}={kv.Value}"))} (expected 10000 each)");

        // The same counting with a plain Dictionary needs a lock
        var viewsLocked = new Dictionary<string, int>();
        var gate = new object();
        Parallel.For(0, 30_000, i =>
        {
            string sku = products[i % products.Length];
            lock (gate)
            {
                viewsLocked[sku] = viewsLocked.GetValueOrDefault(sku) + 1;
            }
        });
        Console.WriteLine($"Dictionary + lock total: {viewsLocked.Values.Sum()}");

        // ---- GetOrAdd: the value factory CAN run more than once ----
        // Two threads ask for the same missing key at the same moment. The barrier forces both
        // factories to be running at the same time, which is exactly the race that happens under load.
        var exchangeRates = new ConcurrentDictionary<string, decimal>();
        int factoryRuns = 0;
        using var barrier = new Barrier(2);
        decimal FetchRateFromApi(string currency)
        {
            Interlocked.Increment(ref factoryRuns);
            barrier.SignalAndWait();          // wait until the other thread is also inside the factory
            return 83.25m;
        }
        var t1 = Task.Run(() => exchangeRates.GetOrAdd("USD", FetchRateFromApi));
        var t2 = Task.Run(() => exchangeRates.GetOrAdd("USD", FetchRateFromApi));
        Task.WaitAll(t1, t2);
        Console.WriteLine($"GetOrAdd: factory ran {factoryRuns} times; both callers got {t1.Result} and {t2.Result}; stored {exchangeRates["USD"]}");

        // Fix: store Lazy<T>. Only one Lazy wins the race, and Lazy runs its factory once.
        var lazyRates = new ConcurrentDictionary<string, Lazy<decimal>>();
        int lazyRuns = 0;
        Parallel.For(0, 100, _ =>
        {
            var rate = lazyRates.GetOrAdd("USD", c => new Lazy<decimal>(() =>
            {
                Interlocked.Increment(ref lazyRuns);
                return 83.25m;
            })).Value;
        });
        Console.WriteLine($"GetOrAdd with Lazy<T>: expensive call ran {lazyRuns} time(s) for 100 callers");

        // ---- ConcurrentQueue: many producers, no lock needed ----
        var auditEvents = new ConcurrentQueue<string>();
        Parallel.For(0, 1_000, i => auditEvents.Enqueue($"order-{i}-viewed"));
        int drained = 0;
        while (auditEvents.TryDequeue(out _)) drained++;
        Console.WriteLine($"ConcurrentQueue: enqueued by many threads, drained {drained}");

        // ---- BlockingCollection: bounded producer/consumer ----
        // The producer blocks when 5 items are waiting, so a slow consumer cannot cause unbounded memory growth.
        using var labelQueue = new BlockingCollection<Shipment>(boundedCapacity: 5);
        int printed = 0;
        var consumer = Task.Run(() =>
        {
            foreach (var shipment in labelQueue.GetConsumingEnumerable())   // blocks while empty, ends after CompleteAdding
                Interlocked.Increment(ref printed);
        });
        for (int orderId = 1; orderId <= 20; orderId++)
            labelQueue.Add(new Shipment(orderId, orderId % 2 == 0 ? "BlueDart" : "Delhivery"));
        labelQueue.CompleteAdding();       // "no more items": lets the consumer loop finish
        consumer.Wait();
        Console.WriteLine($"BlockingCollection: printed {printed} shipping labels, IsCompleted = {labelQueue.IsCompleted}");

        // ---- Thread-safe does not mean your multi-step logic is atomic ----
        var reservations = new ConcurrentDictionary<string, int> { ["SKU-1"] = 1 };
        int successfulReservations = 0;
        Parallel.For(0, 50, _ =>
        {
            // Atomic check-and-decrement: retry with TryUpdate until we win or stock is gone
            while (true)
            {
                int current = reservations["SKU-1"];
                if (current == 0) return;
                if (reservations.TryUpdate("SKU-1", current - 1, current))
                {
                    Interlocked.Increment(ref successfulReservations);
                    return;
                }
            }
        });
        Console.WriteLine($"Last unit of SKU-1: {successfulReservations} reservation succeeded out of 50 attempts, stock now {reservations["SKU-1"]}");
    }
}
