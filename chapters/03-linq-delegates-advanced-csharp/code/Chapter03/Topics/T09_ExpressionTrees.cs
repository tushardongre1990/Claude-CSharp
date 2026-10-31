using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Chapter03.Topics;

public static class T09_ExpressionTrees
{
    // Filters from a product search form. Any of them may be empty.
    private record ProductSearch(string? Category, decimal? MinPrice, decimal? MaxPrice);

    // Builds p => (p.IsActive) && (p.Category == category) && (p.Price >= min) && (p.Price <= max)
    // using only the filters the user actually filled in.
    private static Expression<Func<Product, bool>> BuildFilter(ProductSearch search)
    {
        ParameterExpression p = Expression.Parameter(typeof(Product), "p");
        Expression body = Expression.Property(p, nameof(Product.IsActive));          // p.IsActive

        if (!string.IsNullOrWhiteSpace(search.Category))
            body = Expression.AndAlso(body,
                Expression.Equal(Expression.Property(p, nameof(Product.Category)),
                                 Expression.Constant(search.Category)));               // && p.Category == "..."

        if (search.MinPrice is decimal min)
            body = Expression.AndAlso(body,
                Expression.GreaterThanOrEqual(Expression.Property(p, nameof(Product.Price)),
                                              Expression.Constant(min)));               // && p.Price >= min

        if (search.MaxPrice is decimal max)
            body = Expression.AndAlso(body,
                Expression.LessThanOrEqual(Expression.Property(p, nameof(Product.Price)),
                                           Expression.Constant(max)));                  // && p.Price <= max

        return Expression.Lambda<Func<Product, bool>>(body, p);
    }

    public static void Run()
    {
        // ---- 1. Same lambda, two very different results ----
        Func<Product, bool> asCode = p => p.Price > 50_000m;                      // compiled IL: can only be run
        Expression<Func<Product, bool>> asData = p => p.Price > 50_000m;          // a tree of objects: can be read

        Console.WriteLine($"Func:       {asCode}");
        Console.WriteLine($"Expression: {asData}");

        // ---- 2. Reading the tree ----
        var comparison = (BinaryExpression)asData.Body;
        var left = (MemberExpression)comparison.Left;
        var right = (ConstantExpression)comparison.Right;
        Console.WriteLine($"Body node type: {comparison.NodeType}");
        Console.WriteLine($"Left:  {left.NodeType} -> property '{left.Member.Name}' on parameter '{asData.Parameters[0].Name}'");
        Console.WriteLine($"Right: {right.NodeType} -> value {right.Value}");

        // ---- 3. Compile() turns the data back into a runnable delegate ----
        Func<Product, bool> compiled = asData.Compile();
        var laptop = new Product { Name = "ThinkPad X1", Price = 145_000m };
        Console.WriteLine($"Compiled expression on 145000 laptop: {compiled(laptop)}");

        // ---- 4. Building a filter at runtime and sending it to the database ----
        var (db, sql) = ShopDb.CreateSeeded();
        using var _ = db;

        var searches = new[]
        {
            new ProductSearch("Laptops", MinPrice: 50_000m, MaxPrice: null),
            new ProductSearch(null, MinPrice: null, MaxPrice: 3_000m),
        };
        foreach (var search in searches)
        {
            Expression<Func<Product, bool>> filter = BuildFilter(search);
            Console.WriteLine($"Built filter: {filter}");
            var names = db.Products.Where(filter).Select(p => p.Name).ToList();
            Console.WriteLine($"  Results: {string.Join(", ", names)}");
            Console.WriteLine($"  SQL WHERE: {sql.Commands[^1].Split("WHERE")[1].Trim()}");
        }

        // The usual simpler alternative: chain Where calls on IQueryable. EF Core combines them with AND.
        var form = new ProductSearch("Accessories", null, 1_000m);
        IQueryable<Product> q = db.Products.Where(p => p.IsActive);
        if (form.Category is not null) q = q.Where(p => p.Category == form.Category);
        if (form.MinPrice is not null) q = q.Where(p => p.Price >= form.MinPrice);
        if (form.MaxPrice is not null) q = q.Where(p => p.Price <= form.MaxPrice);
        Console.WriteLine($"Chained Where results: {string.Join(", ", q.Select(p => p.Name))}");
        Console.WriteLine($"  SQL WHERE: {sql.Commands[^1].Split("WHERE")[1].Trim()}");
    }
}
