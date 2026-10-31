namespace Chapter03.Topics;

public static class T07_DeferredExecution
{
    private record Shipment(string TrackingId, string Status, int WeightGrams);

    private static int _apiCalls;

    // Pretends to call a courier API. Each enumeration calls it again.
    private static IEnumerable<Shipment> FetchShipmentsFromCourierApi()
    {
        _apiCalls++;
        yield return new("TRK-1", "InTransit", 500);
        yield return new("TRK-2", "Delivered", 1_200);
        yield return new("TRK-3", "InTransit", 3_000);
    }

    public static void Run()
    {
        // ---- 1. Defining a query does NOT run it ----
        var shipments = new List<Shipment> { new("TRK-1", "InTransit", 500), new("TRK-2", "Delivered", 1_200) };
        var inTransit = shipments.Where(s => s.Status == "InTransit");   // nothing runs here
        shipments.Add(new("TRK-3", "InTransit", 3_000));                  // added AFTER the query was defined
        Console.WriteLine($"Deferred: {string.Join(", ", inTransit.Select(s => s.TrackingId))} (TRK-3 is included)");

        var snapshot = shipments.Where(s => s.Status == "InTransit").ToList();   // runs NOW
        shipments.Add(new("TRK-4", "InTransit", 100));
        Console.WriteLine($"ToList snapshot: {string.Join(", ", snapshot.Select(s => s.TrackingId))} (TRK-4 is not)");

        // ---- 2. Streaming: each item flows through the whole pipeline before the next one starts ----
        Console.WriteLine("Pipeline order (Where -> Select -> foreach):");
        var pipeline = shipments
            .Where(s => { Console.WriteLine($"  Where  {s.TrackingId}"); return s.Status == "InTransit"; })
            .Select(s => { Console.WriteLine($"  Select {s.TrackingId}"); return s.WeightGrams; });
        Console.WriteLine("  (query defined, nothing printed yet)");
        foreach (var grams in pipeline.Take(2))
            Console.WriteLine($"  foreach got {grams}g");
        Console.WriteLine("  (Take(2) stopped early: TRK-4 was never read)");

        // ---- 3. Some operators must read EVERYTHING before giving the first item (buffering) ----
        Console.WriteLine("OrderBy reads all items before yielding the first:");
        var sorted = shipments
            .Where(s => { Console.WriteLine($"  Where  {s.TrackingId}"); return true; })
            .OrderBy(s => s.WeightGrams);
        Console.WriteLine($"  First after OrderBy: {sorted.First().TrackingId}");

        // ---- 4. Multiple enumeration: the source runs once per enumeration ----
        _apiCalls = 0;
        IEnumerable<Shipment> fromApi = FetchShipmentsFromCourierApi();
        int count = fromApi.Count();
        int heavy = fromApi.Count(s => s.WeightGrams > 1_000);
        var first = fromApi.First();
        Console.WriteLine($"Count {count}, heavy {heavy}, first {first.TrackingId}: courier API called {_apiCalls} times");

        _apiCalls = 0;
        List<Shipment> loaded = FetchShipmentsFromCourierApi().ToList();
        Console.WriteLine($"After ToList(): Count {loaded.Count}, heavy {loaded.Count(s => s.WeightGrams > 1_000)}, first {loaded[0].TrackingId}: API called {_apiCalls} time");

        // ---- 5. Captured variables are read when the query RUNS ----
        int minWeight = 1_000;
        var heavyQuery = shipments.Where(s => s.WeightGrams >= minWeight);
        minWeight = 0;   // changed before enumeration
        Console.WriteLine($"Captured minWeight changed to 0 before running: {heavyQuery.Count()} matches (not 2)");

        // ---- 6. Exceptions also happen at enumeration time, not at definition time ----
        var parsed = new[] { "500", "abc", "750" }.Select(int.Parse);   // no error yet
        Console.WriteLine("Select(int.Parse) defined without error");
        try { Console.WriteLine(parsed.Sum()); }
        catch (FormatException) { Console.WriteLine("FormatException thrown when Sum() enumerated the query"); }

        // Immediate operators: ToList, ToArray, ToDictionary, ToHashSet, Count, Sum, First, Any, Max ...
        var byId = shipments.ToDictionary(s => s.TrackingId);
        Console.WriteLine($"ToDictionary ran immediately: {byId.Count} entries");
    }
}
