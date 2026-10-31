# Chapter 3 Exercises

Do these **in code**. Create a new console app for them (`dotnet new console -o Chapter03Exercises` inside this chapter's `code/` folder), or paste them into a scratch project. Try each one before opening its hint.

---

## Part 1 — Coding exercises

### Exercise 1: Sales report with LINQ

Given this data:

```csharp
public record Customer(int Id, string Name, string City);
public record OrderLine(string Sku, int Quantity, decimal UnitPrice);
public record Order(int Id, int CustomerId, DateOnly PlacedOn, string Status, List<OrderLine> Lines);
```

Write LINQ queries (method syntax) that produce:

1. Revenue per city for **paid** orders in October 2026, highest first.
2. The top 3 SKUs by quantity sold, with total quantity and revenue.
3. Every customer with their number of paid orders and total spent, **including customers with no orders** (show 0). Do it once with `GroupJoin` and once with .NET 10 `LeftJoin`.
4. The single biggest order per customer (use `MaxBy`).
5. Orders whose customer doesn't exist (bad data), so it can be reported instead of silently dropped.

Edge cases to test: an order with no lines, a month with no orders (your report must not throw), a customer with only cancelled orders.

<details><summary>Hint</summary>

Line revenue is `l.Quantity * l.UnitPrice`; order total is `o.Lines.Sum(...)`, which is `0` for an empty order. For (2), `SelectMany(o => o.Lines).GroupBy(l => l.Sku)`. For (3) with `GroupJoin`, filter paid orders **before** the join, or the counts include cancelled ones. For (5), `orders.Where(o => !customerIds.Contains(o.CustomerId))` with `customerIds` as a `HashSet<int>`. Use nullable projections for any `Max`/`Average` over possibly empty data.

</details>

### Exercise 2: Notification pipeline with delegates (no events)

Build a `NotificationDispatcher` that holds a list of `Func<OrderShipped, Task>` handlers:

- `Register(Func<OrderShipped, Task> handler)` returns an `IDisposable`; disposing it unregisters that handler.
- `DispatchAsync(OrderShipped message)` runs **every** handler, even if some throw, and returns a summary: how many succeeded and the exceptions from those that failed.
- Write three handlers: email, SMS (throws `HttpRequestException` every third call), loyalty points.
- Show that unregistering via `Dispose()` works, and that registering the same lambda twice runs it twice.

<details><summary>Hint</summary>

Store handlers in a `List<Func<...>>` guarded by a `Lock` (or copy-on-write with an immutable list). `Register` can return a small private class implementing `IDisposable` whose `Dispose` removes the exact delegate instance it was given. Loop with `try/catch` per handler; collect exceptions into a list (or an `AggregateException`).

</details>

### Exercise 3: Fix the closure bugs

This code is meant to schedule a retry for each failed payment, but it's wrong in two ways. Find both bugs, explain them, and fix them. Prove the fix with output.

```csharp
var failedPaymentIds = new List<string> { "PAY-1", "PAY-2", "PAY-3" };
var retries = new List<Func<string>>();
for (int i = 0; i < failedPaymentIds.Count; i++)
    retries.Add(() => $"Retrying {failedPaymentIds[i]}");

int maxAttempts = 3;
Func<int, bool> canRetry = attempt => attempt < maxAttempts;
maxAttempts = 0;   // a later config reload

foreach (var r in retries) Console.WriteLine(r());
Console.WriteLine(canRetry(1));
```

<details><summary>Hint</summary>

The first loop throws `ArgumentOutOfRangeException`, because every lambda sees `i == 3`. The second "bug" is the closure reading the **current** `maxAttempts`: decide whether you want that (live config) or a snapshot (copy into a local before creating the lambda). Explain which is correct for a retry policy.

</details>

### Exercise 4: Dynamic product search on EF Core

Using the `ShopDbContext` from topic 8 (copy `ShopDb.cs` into your project), build:

```csharp
public record ProductSearch(string? NameContains, string? Category, decimal? MinPrice, decimal? MaxPrice,
                            string SortBy = "name", bool Descending = false, int Page = 1, int PageSize = 20);

Task<IReadOnlyList<ProductDto>> SearchAsync(ShopDbContext db, ProductSearch search);
```

- Only filters that are filled in are applied.
- `SortBy` is one of `name`, `price`, `category`. Anything else → `ArgumentException`. Always add `ThenBy(p => p.Id)` so paging is deterministic.
- Project to `ProductDto(int Id, string Name, decimal Price)` **inside** the query.
- Validate `Page >= 1` and `1 <= PageSize <= 100`.
- Print `ToQueryString()` for three different searches and confirm: there's a `WHERE` only for the filters you used, the values are **parameters** (not literals), and there's `LIMIT`/`OFFSET`.
- Then write a second version that builds the filter as a single `Expression<Func<Product, bool>>` by hand. Compare its SQL with the first version and explain the difference.

<details><summary>Hint</summary>

Start with `IQueryable<Product> q = db.Products;` and reassign: `if (search.Category is not null) q = q.Where(p => p.Category == search.Category);`. For sorting, a `switch` expression returning `IOrderedQueryable<Product>`. For the hand-built version, `Expression.Constant` gives literals; to get parameters, put the value in a small object (`var holder = new { Value = min };`) and use `Expression.Property(Expression.Constant(holder), "Value")`.

</details>

### Exercise 5: Lazy CSV importer with `yield return`

Write `IEnumerable<OrderImportRow> ReadOrders(string path)` that:

- validates `path` **eagerly** (throws `ArgumentException` at the call site for null/empty, `FileNotFoundException` if missing),
- reads lines lazily with `File.ReadLines`, skips the header and blank lines,
- parses `OrderId,Sku,Quantity,UnitPrice` and **yields** each row,
- for an invalid line, yields nothing for it and records the line number in an `errors` list passed in by the caller (no `yield` inside `try/catch`: find a way around CS1626).

Then: process a generated 200,000-line file with `ReadOrders(path).Chunk(1000)`, "inserting" each batch (just count them), and print peak memory with `GC.GetTotalMemory(false)` compared with a version using `File.ReadAllLines` + `ToList()`. Report your numbers and where you measured them.

<details><summary>Hint</summary>

Use the wrapper + local iterator pattern for eager validation. To avoid CS1626, move parsing into a `bool TryParseRow(string line, out OrderImportRow row)` method and keep the `yield return` outside any `try/catch`. Generate the test file in code before measuring.

</details>

### Exercise 6: Payment fee rules with pattern matching

Model payments as records: `CardPayment(Amount, Network, IsInternational, IsCorporate)`, `UpiPayment(Amount)`, `NetBankingPayment(Amount, Bank)`, `WalletPayment(Amount, Provider)`.

Rules:

- International cards: 3.5%, minimum ₹10.
- Corporate Amex: 2.75%. Other Amex: 2.5%. Other cards: 1.8%.
- UPI up to ₹2,000 free, above that 0.3%.
- Net banking: flat ₹15 for "HDFC" or "ICICI", ₹20 otherwise.
- Wallets: 1%, capped at ₹100.
- Amount ≤ 0 → `ArgumentOutOfRangeException`. Unknown payment type → `NotSupportedException` with the type name.

Implement it as one switch expression, round fees to 2 decimal places (`MidpointRounding.AwayFromZero`), and write a table-driven test (a list of `(Payment, expectedFee)` tuples) covering every rule and boundary (exactly ₹2,000 UPI, a wallet fee exactly at the cap, a tiny international payment that hits the minimum).

<details><summary>Hint</summary>

Put `{ Amount: <= 0 }` first. Use `Math.Max(10m, ...)` and `Math.Min(100m, ...)` for minimum and cap. Make sure the corporate Amex arm comes before the general Amex arm, and check whether the compiler reports any unreachable arms.

</details>

### Exercise 7: `[Sensitive]` log masking with caching

Extend topic 14's `ToSafeLogString`:

- support nested objects (an `Order` with a `Customer` that has `[Sensitive]` fields) and collections of objects,
- cache the `PropertyInfo[]` and "is sensitive" flags per type in a `ConcurrentDictionary<Type, ...>`,
- add a `[Sensitive(ShowLast = 4)]` option that shows only the last 4 characters,
- measure 100,000 calls with and without the cache using `Stopwatch` and report both timings (say where you measured them).

<details><summary>Hint</summary>

Treat strings, numbers, enums, `DateTime`/`DateOnly` and `decimal` as leaf values. Guard against cycles (an `Order` pointing back to its `Customer` that points to the `Order`) with a `HashSet<object>` using `ReferenceEqualityComparer.Instance`, or with a depth limit.

</details>

### Exercise 8: Modernise a class with C# 12–14 features

Rewrite this .NET Framework-style class using primary constructors, `required`/`init`, collection expressions, the `field` keyword and null-conditional assignment, **without changing behaviour**. Say which features need .NET 8, 9 or 10.

```csharp
public class ShipmentService
{
    private readonly ICourierClient _courier;
    private readonly ILogger<ShipmentService> _logger;
    private readonly object _sync = new object();
    private int _shipmentsToday;

    public ShipmentService(ICourierClient courier, ILogger<ShipmentService> logger)
    {
        _courier = courier ?? throw new ArgumentNullException(nameof(courier));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int Ship(Order order)
    {
        if (order.Customer != null)
            order.Customer.LastShippedAt = DateTime.UtcNow;
        var tags = new List<string>();
        tags.Add("standard");
        if (order.IsFragile) tags.Add("fragile");
        _courier.Book(order.Id, tags.ToArray());
        lock (_sync) { return ++_shipmentsToday; }
    }
}

public class Address
{
    private string _pinCode = "";
    public string PinCode
    {
        get { return _pinCode; }
        set { _pinCode = value.Trim(); }
    }
}
```

<details><summary>Hint</summary>

With a primary constructor you lose the null checks unless you keep them: `private readonly ICourierClient _courier = courier ?? throw new ArgumentNullException(nameof(courier));`. `_sync` becomes `Lock` (.NET 9). `order.Customer?.LastShippedAt = ...` needs C# 14. The tags list becomes `string[] tags = order.IsFragile ? ["standard", "fragile"] : ["standard"];`.

</details>

---

## Part 2 — 5-YOE scenario questions

Answer these as you would in an interview: state what you'd check first, what you think is happening, and how you'd fix and prevent it.

1. **The slow product endpoint.** `GET /api/products?category=laptops&page=2` takes 20 seconds in production and the API pod's memory jumps by 1.5 GB per request. Locally it takes 50 ms. The repository method's signature is `IEnumerable<Product> GetAll()`. What's happening, how do you prove it, and how do you fix it without breaking the other 12 callers of `GetAll()`?

2. **Report numbers that don't add up.** Finance says the "revenue by customer" report shows ₹3.2 lakh less than the payments system for last month. The report uses `orders.Join(customers, ...)`. What are the likely causes, how do you find the missing orders, and what do you change?

3. **The leaking dashboard.** A long-running API's memory grows steadily over the day until Kubernetes kills the pod. A heap dump shows 400,000 `LiveSalesWidget` objects, each referenced by a delegate inside a singleton `OrderEventsPublisher`. Explain the cause, the immediate fix, and how you'd redesign it.

4. **Three queries instead of one.** In the SQL logs you see the same `SELECT ... FROM Orders WHERE CustomerId = @p` three times per request. The code is:
   ```csharp
   var orders = _repo.GetOrdersForCustomer(id);   // returns IEnumerable<Order>
   var count = orders.Count();
   var total = orders.Sum(o => o.Total);
   var latest = orders.OrderByDescending(o => o.PlacedOn).FirstOrDefault();
   ```
   Why does this happen, what are the risks beyond performance, and what would you change in both the caller and the repository's signature?

5. **"Could not be translated" after an upgrade.** After upgrading a service from EF Core 2.2 to EF Core 8, a query that used to work now throws `InvalidOperationException: The LINQ expression ... could not be translated`. The query calls a private `IsEligibleForDiscount(Customer c)` method inside `Where`. Why did it work before, why does it fail now, and what are your options (with trade-offs)?

6. **A switch expression in production.** Logs show `System.Runtime.CompilerServices.SwitchExpressionException: Non-exhaustive switch expression failed to match its input.` (the unmatched value was `5`) from your order status mapping, right after a new status `OnHold` was added by another team. How did this get past the compiler, and how do you make this class of bug fail at build time or at least fail clearly?

7. **Should we upgrade to use `field` and `Lock`?** Your team runs on .NET 8. A developer wants to use the C# 14 `field` keyword and `System.Threading.Lock` in a shared NuGet library used by 6 services, two of them still on .NET 6. What do you advise, and why?

8. **Reflection-based mapping is slow.** A home-grown mapper uses `GetProperties()` and `PropertyInfo.GetValue/SetValue` for every entity-to-DTO mapping. Profiling shows it's 30% of CPU on the busiest endpoint. What options do you have (from quick fixes to a redesign), and what would you pick? (Mention licensing if you'd consider a library.)
