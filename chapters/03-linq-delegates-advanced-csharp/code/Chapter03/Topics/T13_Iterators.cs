namespace Chapter03.Topics;

public static class T13_Iterators
{
    private record Customer(int Id, string Email);

    // Pretend REST API that returns customers one page at a time (3 pages of 2)
    private static List<Customer> FetchPage(int pageNumber)
    {
        Console.WriteLine($"    [API] GET /customers?page={pageNumber}");
        return pageNumber switch
        {
            1 => [new(1, "a@shop.in"), new(2, "b@shop.in")],
            2 => [new(3, "c@shop.in"), new(4, "d@shop.in")],
            3 => [new(5, "e@shop.in")],
            _ => [],
        };
    }

    // The caller sees one flat sequence; pages are fetched only when needed
    private static IEnumerable<Customer> GetAllCustomers()
    {
        Console.WriteLine("    (iterator body started)");
        int page = 1;
        while (true)
        {
            var batch = FetchPage(page);
            if (batch.Count == 0)
                yield break;                    // end of the sequence
            foreach (var customer in batch)
                yield return customer;          // hand back one item and PAUSE here
            page++;
        }
    }

    // An endless sequence is fine: the caller decides how many to take
    private static IEnumerable<string> InvoiceNumbers(int start)
    {
        for (int n = start; ; n++)
            yield return $"INV-{n:D5}";
    }

    // TRAP: argument check inside an iterator runs late (on the first MoveNext, not at the call)
    private static IEnumerable<string> ReadCsvLinesLazyCheck(string? path)
    {
        ArgumentNullException.ThrowIfNull(path);
        yield return $"line 1 of {path}";
    }

    // FIX: validate eagerly in a normal method, then return a local iterator function
    private static IEnumerable<string> ReadCsvLines(string? path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return Iterate();

        IEnumerable<string> Iterate()
        {
            yield return $"line 1 of {path}";
        }
    }

    // finally inside an iterator runs when the caller stops early (foreach calls Dispose)
    private static IEnumerable<int> ReadExportFile()
    {
        Console.WriteLine("    file opened");
        try
        {
            for (int row = 1; row <= 1_000; row++)
                yield return row;
        }
        finally
        {
            Console.WriteLine("    file closed (finally ran)");
        }
    }

    public static void Run()
    {
        // ---- 1. Lazy paging ----
        Console.WriteLine("Calling GetAllCustomers():");
        var customers = GetAllCustomers();
        Console.WriteLine("    returned immediately; nothing fetched yet");

        Console.WriteLine("Take the first 3 customers:");
        foreach (var c in customers.Take(3))
            Console.WriteLine($"    got customer {c.Id}");
        Console.WriteLine("    page 3 was never requested");

        Console.WriteLine("Count all customers:");
        Console.WriteLine($"    total {customers.Count()} (the iterator ran again from the start)");

        // ---- 2. Endless sequence + Take ----
        Console.WriteLine($"Next invoice numbers: {string.Join(", ", InvoiceNumbers(42).Take(3))}");

        // ---- 3. Late validation trap ----
        IEnumerable<string> lazy = ReadCsvLinesLazyCheck(null);       // no exception here!
        Console.WriteLine("Lazy-check iterator created with null path, no exception yet");
        try { lazy.First(); }
        catch (ArgumentNullException) { Console.WriteLine("  ...ArgumentNullException only when enumerated"); }

        try { ReadCsvLines(null); }
        catch (ArgumentNullException) { Console.WriteLine("Eager-check version threw at the call site"); }

        // ---- 4. Stopping early still runs finally ----
        Console.WriteLine("Read rows until row 2:");
        foreach (var row in ReadExportFile())
        {
            Console.WriteLine($"    row {row}");
            if (row == 2) break;
        }

        // ---- 5. What the compiler builds: a class that implements IEnumerable<T> and IEnumerator<T> ----
        var iteratorType = GetAllCustomers().GetType();
        Console.WriteLine($"Compiler-generated type: {iteratorType.Name}");
        Console.WriteLine($"Implements IEnumerator<Customer>: {iteratorType.GetInterfaces().Any(i => i == typeof(IEnumerator<Customer>))}");
    }
}
