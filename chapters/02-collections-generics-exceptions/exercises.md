# Chapter 2 Exercises

Do these **in code**. Create a new console app for them (`dotnet new console -o Chapter02Exercises` inside this chapter's `code/` folder), or paste them into a scratch project. Try each one before opening its hint.

---

## Part 1 — Coding exercises

### Exercise 1: Fix the slow order import

This method takes over a minute in production with 50,000 order lines and 50,000 products. Rewrite it to run in O(n + m), and **measure** both versions with `Stopwatch` on generated data (50,000 of each).

```csharp
public record Product(int Id, string Name, decimal Price, bool IsActive);
public record OrderLine(int OrderId, int ProductId, int Quantity);

public static decimal TotalForActiveProducts(List<OrderLine> lines, List<Product> products)
{
    decimal total = 0;
    foreach (var line in lines)
    {
        var product = products.FirstOrDefault(p => p.Id == line.ProductId);
        if (product != null && product.IsActive)
            total += product.Price * line.Quantity;
    }
    return total;
}
```

Edge cases to test: a line whose product doesn't exist, an inactive product, an empty list of lines, and duplicate product IDs in the product list (decide what should happen and handle it explicitly).

<details><summary>Hint</summary>

`var byId = products.ToDictionary(p => p.Id);` then `byId.TryGetValue(line.ProductId, out var product)`. `ToDictionary` throws `ArgumentException` on duplicate IDs. Either let it throw (bad data should fail loudly) or use `products.GroupBy(p => p.Id).ToDictionary(g => g.Key, g => g.First())` and log the duplicates.

</details>

### Exercise 2: Encapsulated shopping cart

Build a `ShoppingCart` class:

- Internally a `Dictionary<string, CartItem>` keyed by SKU, **case-insensitive** (`"sku-1"` and `"SKU-1"` are the same item).
- `AddItem(string sku, int quantity, decimal unitPrice)`: if the SKU already exists, increase its quantity. Throw `ArgumentOutOfRangeException` for quantity ≤ 0 or a negative price, and `ArgumentException` for an empty SKU (use the .NET 8 guard helpers).
- `RemoveItem(string sku)` returns `bool`.
- `Items` exposes `IReadOnlyCollection<CartItem>`. Prove that a caller can't add to it, even with a cast.
- `Total` property.
- An `Undo()` method that reverts the last `AddItem`/`RemoveItem` using a `Stack<T>`. `Undo()` on an empty history returns `false` instead of throwing.

<details><summary>Hint</summary>

`new Dictionary<string, CartItem>(StringComparer.OrdinalIgnoreCase)`. For `Items`, return `_items.Values.ToList().AsReadOnly()` (or a `ReadOnlyCollection`). For undo, push a small record that describes how to reverse each action, e.g. `record UndoStep(string Sku, CartItem? Previous)`.

</details>

### Exercise 3: Generic in-memory repository with constraints

Write `InMemoryRepository<TEntity, TKey>`:

- `where TEntity : class, IEntity<TKey>` and `where TKey : notnull`.
- `Add` (throws a custom `DuplicateEntityException` that includes the key and entity type name), `GetById` (returns `null` if missing), `Update`, `Remove`, and `GetAll` returning `IReadOnlyList<TEntity>`.
- Use it for `Customer` (key `int`) and `Coupon` (key `string`, case-insensitive: pass an `IEqualityComparer<TKey>` through the constructor).
- Write `DuplicateEntityException` properly: the three standard constructors plus typed properties.

<details><summary>Hint</summary>

`public interface IEntity<TKey> { TKey Id { get; } }`. Constructor: `public InMemoryRepository(IEqualityComparer<TKey>? comparer = null) => _items = new Dictionary<TKey, TEntity>(comparer);`. A `null` comparer means "use the default".

</details>

### Exercise 4: Thread-safe rate cache without duplicate API calls

You have a slow `Task<decimal> FetchRateAsync(string currency)` (simulate it with `await Task.Delay(200)` and an `Interlocked` call counter).

- Build `ExchangeRateCache.GetRateAsync(string currency)` so that **100 parallel callers** asking for `"USD"` at the same moment cause **exactly one** call to `FetchRateAsync`.
- Prove it by printing the call counter after `await Task.WhenAll(...)` over 100 tasks.
- Then break it on purpose by using `ConcurrentDictionary<string, decimal>` with `GetOrAdd` and a blocking factory, and show the counter going above 1.
- Bonus: if `FetchRateAsync` throws, the failure must **not** be cached forever. The next caller should try again.

<details><summary>Hint</summary>

`ConcurrentDictionary<string, Lazy<Task<decimal>>>` with `GetOrAdd(currency, c => new Lazy<Task<decimal>>(() => FetchRateAsync(c)))` and `await lazy.Value`. For the bonus, catch the exception, then `TryRemove(new KeyValuePair<string, Lazy<Task<decimal>>>(currency, lazy))` so you only remove the exact entry that failed.

</details>

### Exercise 5: Coupon service with a Result type

Implement `Result<T>` (success value **or** an `Error(Code, Message)`) and a `CouponService.Apply(string code, decimal orderTotal, DateOnly today)`:

- Expected failures return results: `coupon.not_found`, `coupon.expired`, `coupon.min_order_not_met`, `coupon.usage_limit_reached` (each coupon has a max usage count).
- An unexpected failure (simulate "coupon store unavailable" with a flag) **throws**.
- Write a `ToHttpStatus(Result<T>)` function that maps the codes to 200/404/422, and a top-level `try/catch` that maps exceptions to 503.
- Test it with: a valid coupon, each failure, a code with different casing, an order total of exactly the minimum, and a coupon on its exact expiry date. Decide whether that last one is valid and write the rule down.

<details><summary>Hint</summary>

Make `Result<T>` a `readonly record struct` with private constructor and static `Success`/`Failure` factories. Usage must only be incremented on success: make sure no failure path changes state.

</details>

### Exercise 6: Exceptions, stack traces and logging done right

Create three classes: `ShippingLabelClient` (throws `HttpRequestException` with `StatusCode = 503` on the first two calls and returns a label on the third), `ShippingService` and `FulfilmentController` (a plain class that plays the boundary).

1. First write the **bad** version: every layer catches, logs with `LogError(ex.Message)` and does `throw ex;`. Count the log entries for one failure, and print the stack trace method names to show what's lost.
2. Then write the **good** version:
   - `ShippingLabelClient` has no try/catch.
   - `ShippingService` retries **only** on 503, using an exception filter, at most 3 attempts, and logs each retry at **Warning**. On final failure it wraps the exception in `LabelGenerationFailedException` (custom, with `OrderId`), keeping the inner exception.
   - `FulfilmentController` catches `LabelGenerationFailedException`, logs **once** at Error with a message template (`{OrderId}`), and returns a "503" string.
3. Use `Microsoft.Extensions.Logging` with the console provider (`builder.AddSimpleConsole()`, package `Microsoft.Extensions.Logging.Console`).

<details><summary>Hint</summary>

`catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable && attempt < maxAttempts)`. Print stack frames with `new StackTrace(ex).GetFrames().Select(f => f.GetMethod()?.Name)`, as in topic 11 of the notes.

</details>

### Exercise 7: A disposable report exporter

Write `SalesReportExporter : IDisposable` that writes CSV rows to a file:

- It owns a `StreamWriter`. Dispose must flush and close it, be safe to call twice, and make later `WriteRow` calls throw `ObjectDisposedException`.
- Show the bug first: create the exporter without `using`, then try to open the same file for reading and catch the `IOException`.
- Then show that with `using var`, the file is complete and readable, **even when `WriteRow` throws halfway** (e.g. a negative amount).
- Bonus: implement `IAsyncDisposable` as well and use `await using` with `WriteRowAsync`.

<details><summary>Hint</summary>

Open the stream with `FileShare.None` so the lock is visible. For `IAsyncDisposable`: `public async ValueTask DisposeAsync() { if (_disposed) return; await _writer.DisposeAsync(); _disposed = true; }`.

</details>

---

## Part 2 — Scenario questions (5-YOE level)

Answer these out loud, as if in an interview. Aim for 1–2 minutes each, and include **trade-offs**.

1. **An endpoint that returns a customer's order history gets slower every month.** Profiling shows most time in a loop that calls `allOrders.Where(o => o.CustomerId == id).ToList()` for each customer in a batch report. Explain the complexity problem, the in-memory fix (`ToLookup`/`GroupBy`), and why the real fix is probably in the database query.

2. **Production occasionally shows `InvalidOperationException: Operations that change non-concurrent collections must have exclusive access`** from a singleton `ProductCatalogService` that holds a `Dictionary<string, Product>` refreshed every 10 minutes by a timer. What's happening? Compare three fixes: `lock`, `ConcurrentDictionary`, and building a new `FrozenDictionary` and swapping the reference. Which would you pick and why?

3. **Your payment webhook endpoint receives the same event several times** (the provider retries on timeout). A teammate proposes a `static HashSet<string>` of processed event IDs. Why does that fail with 3 app instances and after a restart? What would you do instead?

4. **The logs for a single failed checkout contain five error entries with different messages**, and one of them shows the controller as the source of a timeout. Walk through how you'd find the cause and how you'd change the team's exception-handling conventions.

5. **A senior colleague says "exceptions are slow, so we should replace all exceptions with Result types".** Respond. Where do you agree, where do you disagree, and what does the boundary (API response, logging, transactions) look like in your preferred design?

6. **After a deployment, the app runs out of SQL connections** ("Timeout expired... obtaining a connection from the pool") under moderate load, and `netstat` shows thousands of sockets in `TIME_WAIT` to a partner API. Which coding mistakes cause each symptom, and how would you confirm and fix them?

7. **A job processes a priority queue of shipments.** Customers complain that within the same shipping tier, orders placed earlier are shipped later. Explain why, and fix it without changing the business rule.

8. **A library you maintain exposes `public List<Invoice> Invoices { get; }`.** Consumers depend on it. You want to stop external code from modifying it. How do you change the API, what breaks for consumers, and how would you roll it out (consider `IReadOnlyList<T>` covariance, `[Obsolete]`, and versioning)?
