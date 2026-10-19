using System.Collections;
using System.Numerics;

namespace Chapter02.Topics;

// Every stored entity has an Id
public interface IEntity
{
    int Id { get; }
}

public record Customer(int Id, string Email) : IEntity;
public record Invoice(int Id, decimal Amount) : IEntity;

// ---- A generic class with a constraint ----
// One repository class works for Customer, Invoice, Product... and the compiler checks the type.
public class InMemoryRepository<TEntity> where TEntity : class, IEntity
{
    private readonly Dictionary<int, TEntity> _items = new();

    public void Add(TEntity entity) => _items.Add(entity.Id, entity);   // allowed because TEntity : IEntity
    public TEntity? GetById(int id) => _items.GetValueOrDefault(id);    // null if missing (allowed because TEntity : class)
    public IReadOnlyCollection<TEntity> GetAll() => _items.Values;
}

// ---- Static fields are per closed type: Cache<Customer> and Cache<Invoice> are different classes ----
public static class TypeCache<T>
{
    public static int Hits;
    public static readonly string TypeName = typeof(T).Name;   // computed once per T
}

public static class T06_Generics
{
    // ---- A generic method with an interface constraint ----
    private static T Highest<T>(IEnumerable<T> items) where T : IComparable<T>
    {
        using var e = items.GetEnumerator();
        if (!e.MoveNext()) throw new ArgumentException("Sequence is empty.", nameof(items));
        T best = e.Current;
        while (e.MoveNext())
            if (e.Current.CompareTo(best) > 0) best = e.Current;
        return best;
    }

    // ---- new() constraint: the method can create T ----
    private static T CreateDefaultSettings<T>() where T : new() => new T();

    private class EmailSettings
    {
        public string SmtpHost { get; set; } = "smtp.local";
        public int Port { get; set; } = 25;
    }

    // ---- Generic math (.NET 7+): one Sum for int, decimal, double... ----
    private static T SumAll<T>(IEnumerable<T> values) where T : INumber<T>
    {
        T total = T.Zero;
        foreach (var v in values) total += v;
        return total;
    }

    // ---- default(T): 0 for numbers, null for reference types ----
    private static T FirstOrFallback<T>(IReadOnlyList<T> items) => items.Count > 0 ? items[0] : default!;

    public static void Run()
    {
        var customers = new InMemoryRepository<Customer>();
        customers.Add(new Customer(1, "asha@shop.com"));
        customers.Add(new Customer(2, "ravi@shop.com"));
        var invoices = new InMemoryRepository<Invoice>();
        invoices.Add(new Invoice(501, 1_250m));

        Console.WriteLine($"Customer 2: {customers.GetById(2)?.Email}, Customer 9: {customers.GetById(9)?.Email ?? "(null)"}, invoices: {invoices.GetAll().Count}");
        // customers.Add(new Invoice(1, 5m));        // compile error: Invoice is not a Customer
        // var bad = new InMemoryRepository<int>();  // compile error: int is not a class and not IEntity

        Console.WriteLine($"Highest invoice amount: {Highest(new[] { 1_250m, 980m, 4_100m })}");
        Console.WriteLine($"Latest order date: {Highest(new[] { new DateOnly(2026, 1, 5), new DateOnly(2026, 3, 1) }):yyyy-MM-dd}");

        var settings = CreateDefaultSettings<EmailSettings>();
        Console.WriteLine($"new() constraint: {settings.SmtpHost}:{settings.Port}");

        Console.WriteLine($"SumAll<int>: {SumAll(new[] { 3, 4, 5 })}, SumAll<decimal>: {SumAll(new[] { 10.25m, 0.75m })}");

        Console.WriteLine($"default(T): int -> {FirstOrFallback(Array.Empty<int>())}, string -> {FirstOrFallback(Array.Empty<string>()) ?? "null"}");

        TypeCache<Customer>.Hits++;
        TypeCache<Customer>.Hits++;
        TypeCache<Invoice>.Hits++;
        Console.WriteLine($"Static per closed type: {TypeCache<Customer>.TypeName} hits {TypeCache<Customer>.Hits}, {TypeCache<Invoice>.TypeName} hits {TypeCache<Invoice>.Hits}");

        // ---- Why generics exist: the old non-generic ArrayList ----
        var oldStyle = new ArrayList { 100, 200, "three hundred" };   // anything goes in, as object (ints are boxed)
        try
        {
            int sum = 0;
            foreach (object o in oldStyle) sum += (int)o;              // the string fails only at runtime
        }
        catch (InvalidCastException) { Console.WriteLine("ArrayList: InvalidCastException at runtime. List<int> would be a compile error."); }

        // Reified generics: the runtime knows the real type argument
        Console.WriteLine($"Runtime types: {typeof(List<int>).Name}<{typeof(List<int>).GetGenericArguments()[0].Name}> vs open type {typeof(List<>).Name}, IsGenericTypeDefinition = {typeof(List<>).IsGenericTypeDefinition}");
    }
}
