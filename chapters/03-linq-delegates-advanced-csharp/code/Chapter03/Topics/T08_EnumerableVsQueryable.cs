using Microsoft.EntityFrameworkCore;

namespace Chapter03.Topics;

public static class T08_EnumerableVsQueryable
{
    // A plain C# method. EF Core cannot turn this into SQL.
    private static bool IsPremiumName(string name) => name.StartsWith("Mac") || name.StartsWith("Think");

    // BAD repository helper: the parameter type IEnumerable<Product> forces in-memory LINQ
    private static int CountActive(IEnumerable<Product> products) => products.Count(p => p.IsActive);

    // GOOD: IQueryable<Product> keeps the query translatable to SQL
    private static int CountActive(IQueryable<Product> products) => products.Count(p => p.IsActive);

    private static void PrintSql(SqlCapture sql, string label)
    {
        Console.WriteLine($"{label}:");
        foreach (var command in sql.Commands)
            Console.WriteLine("    " + command.ReplaceLineEndings("\n    "));
        sql.Commands.Clear();
    }

    public static void Run()
    {
        var (db, sql) = ShopDb.CreateSeeded();
        using var _ = db;

        // ---- 1. IQueryable: the whole query becomes SQL and runs in the database ----
        IQueryable<string> query = db.Products
            .Where(p => p.Category == "Laptops" && p.Price > 50_000m)
            .OrderBy(p => p.Price)
            .Select(p => p.Name);
        Console.WriteLine($"Query type: {query.GetType().Name}");
        Console.WriteLine($"Result: {string.Join(", ", query.ToList())}");
        PrintSql(sql, "  SQL sent (filter, sort and column list are all in SQL)");

        // ---- 2. AsEnumerable() switches to in-memory LINQ: the database returns EVERY row ----
        var inMemory = db.Products
            .AsEnumerable()                                     // from here on: LINQ to Objects
            .Where(p => p.Category == "Laptops" && p.Price > 50_000m)
            .Select(p => p.Name)
            .ToList();
        Console.WriteLine($"Result: {string.Join(", ", inMemory)}");
        PrintSql(sql, "  SQL sent after AsEnumerable() (no WHERE: all rows and all columns)");

        // ---- 2b. Only what comes AFTER the switch runs in memory. Operators composed before it stay in SQL. ----
        IEnumerable<Product> laptops = db.Products.Where(p => p.Category == "Laptops");   // still IQueryable underneath
        var expensiveLaptops = laptops.Where(p => p.Price > 50_000m).Count();              // Enumerable.Where + Enumerable.Count
        Console.WriteLine($"Expensive laptops = {expensiveLaptops}");
        PrintSql(sql, "  SQL sent (category filter in SQL; price filter and count in memory)");

        // ---- 3. The same thing happens silently through a parameter type ----
        Console.WriteLine($"CountActive(IEnumerable) = {CountActive((IEnumerable<Product>)db.Products)}");
        PrintSql(sql, "  SQL sent");
        Console.WriteLine($"CountActive(IQueryable)  = {CountActive(db.Products)}");
        PrintSql(sql, "  SQL sent");

        // ---- 4. A Func<T,bool> variable also forces in-memory filtering ----
        Func<Product, bool> isCheapFunc = p => p.Price < 5_000m;
        var cheap = db.Products.Where(isCheapFunc).Count();     // picks Enumerable.Where (Func), not Queryable.Where (Expression)
        Console.WriteLine($"Where(Func) count = {cheap}");
        PrintSql(sql, "  SQL sent");

        // ---- 5. Untranslatable code inside the query: EF Core throws instead of guessing ----
        try
        {
            db.Products.Where(p => IsPremiumName(p.Name)).ToList();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Custom method in Where: {ex.GetType().Name}:");
            Console.WriteLine("    " + ex.Message.ReplaceLineEndings("\n    "));
        }

        // ---- 6. ToQueryString(): see the SQL without running it (handy for debugging) ----
        var page = db.Products.Where(p => p.IsActive).OrderBy(p => p.Id).Skip(1).Take(2);
        Console.WriteLine("ToQueryString() for Skip(1).Take(2):");
        Console.WriteLine("    " + page.ToQueryString().ReplaceLineEndings("\n    "));
    }
}
