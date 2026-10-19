using System.Diagnostics;

namespace Chapter02.Topics;

public static class T05_ChoosingACollection
{
    public static void Run()
    {
        // Scenario: an import job receives 20,000 customer ids and must skip
        // the ones that are blocked. There are 20,000 blocked ids.
        const int n = 20_000;
        var blockedList = Enumerable.Range(0, n).Select(i => i * 2).ToList();   // even ids are blocked
        var blockedSet = blockedList.ToHashSet();
        var incoming = Enumerable.Range(0, n).ToArray();

        // Warm up once so JIT compilation is not counted in the timing
        _ = CountBlocked(incoming[..100], blockedList.Contains);
        _ = CountBlocked(incoming[..100], blockedSet.Contains);

        var sw = Stopwatch.StartNew();
        int hitsList = CountBlocked(incoming, blockedList.Contains);   // O(n) per lookup -> O(n * m) overall
        var listTime = sw.Elapsed;

        sw.Restart();
        int hitsSet = CountBlocked(incoming, blockedSet.Contains);     // O(1) average per lookup
        var setTime = sw.Elapsed;

        Console.WriteLine($"Blocked found: List {hitsList}, HashSet {hitsSet}");
        Console.WriteLine($"List.Contains x {n:N0}:    {listTime.TotalMilliseconds,8:N2} ms");
        Console.WriteLine($"HashSet.Contains x {n:N0}: {setTime.TotalMilliseconds,8:N2} ms");

        // ---- Memory: bytes allocated to hold 100,000 ints ----
        const int count = 100_000;
        long Measure(Func<object> build)
        {
            long before = GC.GetAllocatedBytesForCurrentThread();
            var keepAlive = build();
            long after = GC.GetAllocatedBytesForCurrentThread();
            GC.KeepAlive(keepAlive);
            return after - before;
        }

        long arrayBytes = Measure(() => new int[count]);
        long listGrowBytes = Measure(() => { var l = new List<int>(); for (int i = 0; i < count; i++) l.Add(i); return l; });
        long listSizedBytes = Measure(() => { var l = new List<int>(count); for (int i = 0; i < count; i++) l.Add(i); return l; });
        long setBytes = Measure(() => { var s = new HashSet<int>(count); for (int i = 0; i < count; i++) s.Add(i); return s; });
        long dictBytes = Measure(() => { var d = new Dictionary<int, int>(count); for (int i = 0; i < count; i++) d[i] = i; return d; });

        Console.WriteLine($"Allocated for {count:N0} ints:");
        Console.WriteLine($"  int[]                       {arrayBytes,10:N0} bytes");
        Console.WriteLine($"  List<int> (grows)           {listGrowBytes,10:N0} bytes");
        Console.WriteLine($"  List<int>(capacity)         {listSizedBytes,10:N0} bytes");
        Console.WriteLine($"  HashSet<int>(capacity)      {setBytes,10:N0} bytes");
        Console.WriteLine($"  Dictionary<int,int>(cap.)   {dictBytes,10:N0} bytes");
    }

    private static int CountBlocked(int[] ids, Func<int, bool> isBlocked)
    {
        int hits = 0;
        foreach (var id in ids)
            if (isBlocked(id)) hits++;
        return hits;
    }
}
