# Chapter 3 Quiz — Answers

Don't read this until you've written your own answers in [quiz.md](quiz.md). All Part B outputs were verified by running the code on .NET 10.

## Part A — Concepts

### Delegates, lambdas, events

1. A type-safe reference to a method: an object (deriving from `MulticastDelegate`) that stores the **method** to call and the **target** object to call it on (`null` for static methods). A multicast delegate stores a list of these (the invocation list).
2. `Action` returns `void`; `Func` returns a value; `Predicate<T>` takes one `T` and returns `bool`. In `Func<Order, int, decimal>` the **last** type, `decimal`, is the return type; `Order` and `int` are the parameters.
3. It calls all three in order and returns **only the last method's result**. If the second throws, the third never runs and the exception propagates to the caller (so there's no return value at all).
4. Each lambda expression compiles to its own generated method, so the new lambda isn't equal to the one that was added (delegates are equal only with the same method and target). Nothing is removed (verified on .NET 10). Store the delegate in a field/variable and use it for both `+=` and `-=`.
5. A lambda that uses a variable from its surrounding scope. **The variable itself** is captured, not a snapshot of its value. The compiler moves it into a hidden heap object shared by the method and the lambda, so changes on either side are visible to both, and it lives as long as the delegate.
6. A `for` loop declares one variable for the whole loop; every lambda created in the loop captures that same variable, so when they run later they all see its final value (e.g. `3` for `i < 3`). `foreach` has declared a fresh variable per iteration since C# 5 (*language rule*). Fix `for` by copying `i` into a local declared inside the loop body.
7. A lambda marked `static` (C# 9) that can't capture locals or `this`; the compiler errors (CS8820) if it tries. It guarantees no closure allocation (useful in hot paths) and documents intent. Often combined with "state" overloads like `GetOrAdd(key, static (k, arg) => ..., arg)`.
8. An `event` is a delegate field that outside code can only `+=` and `-=` (CS0070 otherwise). Only the declaring class can raise or reset it. A public delegate field can be invoked by anyone (fake events) or overwritten with `=`, wiping all other subscribers.
9. Synchronously, one after another, on the thread that raised it. The publisher's method doesn't return until all handlers finish. If a handler throws, the remaining handlers don't run and the exception comes out of the publisher's method, possibly after it already did its work (e.g. saved the order), leading to a 500 and a client retry.
10. The publisher's delegate holds a reference to each subscriber (the delegate's target). If the publisher lives longer (a singleton `PriceFeed`) and short-lived subscribers (per-connection or per-request objects) never unsubscribe, they can never be garbage collected and memory grows until restart. Fix: unsubscribe in `Dispose()`, use weak events, or avoid cross-lifetime events entirely (DI-resolved handlers, `Channel<T>`, messaging).
11. No. C# events are in-process and in-memory: they don't reach other services or other instances of your service, and they're lost on a crash or restart. Use a message broker (Azure Service Bus, RabbitMQ) with the outbox pattern so the message is reliably published after the order transaction commits.

### LINQ operators

12. `Select` maps each item to one result; with a collection-returning selector you get a sequence of collections (`IEnumerable<List<OrderLine>>`). `SelectMany` maps each item to a sequence and flattens them into one sequence: all order lines across all orders, e.g. to list every SKU sold.
13. The second `OrderBy` becomes the new **primary** sort. Because `OrderBy` is stable, the city ordering survives only as a tie-breaker among equal totals, so you get `OrderBy(Total).ThenBy(City)`: the reverse of the intended priority. (That tie-breaking is only for in-memory LINQ. On a database query, ties have no defined order at all.) Use `OrderBy(o => o.City).ThenBy(o => o.Total)`.
14. `Enumerable.OrderBy` (LINQ to Objects) is documented as **stable** (equal keys keep their original order). `List<T>.Sort` (and `Array.Sort`) is **not** stable. For EF Core queries the sort happens in SQL, where the order of ties is undefined, so always finish with `ThenBy` on a unique key when the order must be deterministic.
15. `OrderBy(uniqueKey).Skip((page - 1) * size).Take(size)`. Without a deterministic order the source can return rows in any order, so the same row can appear on two pages or on none, especially on a database. For deep pages on big tables, keyset paging (`Where(o => o.Id > lastId).OrderBy(o => o.Id).Take(size)`) is faster.
16. `Join` is an inner join: one result per matching pair, unmatched items dropped. `GroupJoin` gives one result per left item with the (possibly empty) collection of its matches. Left join before .NET 10: `GroupJoin(...)` then `SelectMany(x => x.matches.DefaultIfEmpty(), (x, m) => ...)`, where `m` is `null` for no match. In .NET 10: `left.LeftJoin(right, l => l.Key, r => r.Key, (l, r) => ...)`, which EF Core 10 can translate.
17. About O(n + m). It builds a hash lookup of the inner sequence once (O(m)), then probes it for each outer item (O(1) average each, O(n) total). A nested loop with `First(...)` inside would be O(n × m).
18. `Sum` → `0`. `Average` and `Max` → throw `InvalidOperationException: Sequence contains no elements`. Make `Max` safe by projecting to a nullable type: `list.Max(x => (decimal?)x)` returns `null` for an empty list (or check `Any()` first).
19. `GroupBy` is deferred and returns a sequence of `IGrouping<TKey, T>`. `ToLookup` runs immediately and returns an `ILookup<TKey, T>` that you can index by key; a missing key returns an empty sequence instead of throwing.
20. It throws `OverflowException`. `Enumerable.Sum` for `int` and `long` uses checked arithmetic (verified on .NET 10). Sum into `long` (`Sum(x => (long)x)`) or use `decimal` for money.
21. `First` returns the first match, throws if none. `FirstOrDefault` returns `default` if none. `Single` requires exactly one: throws on none and on more than one. `SingleOrDefault` returns `default` on none but **still throws** on more than one.
22. `0`, because `default(int)` is 0. It can't be told apart from a real `0` (e.g. "no delay"). Fix: `FirstOrDefault(ms => ms > 5000, defaultValue: -1)` (.NET 6+) or project to `int?` (`Select(x => (int?)x).FirstOrDefault(...)` returns `null`).
23. `true` (vacuous truth: no item breaks the rule). Bug: "if all payments succeeded, mark the order paid" marks orders with **zero** payments as paid. Use `payments.Any() && payments.All(...)`.
24. `Any()` stops at the first item; `Count()` may have to read everything. In EF Core, `Any()` becomes `EXISTS (...)` (the database can stop at the first row) and `Count() > 0` becomes `SELECT COUNT(*)` (counts all matching rows). On a `List<T>`, just use the `.Count` property.

### Deferred execution, IQueryable, expression trees

25. A query is only a description until it's enumerated; then it runs, and it runs again on every enumeration, with the source data and captured variables as they are at that moment. Deferred: `Where`, `Select`, `SelectMany`, `OrderBy`, `GroupBy`, `Skip`, `Take`, `Join`, `Distinct`. Immediate: `ToList`, `ToArray`, `ToDictionary`, `ToHashSet`, `Count`, `Sum`, `Average`, `Max`, `First`, `Single`, `Any`, `All`.
26. Streaming operators pass each item along as soon as it's read and can stop early (`Where`, `Select`, `Take`). Buffering operators must read the whole input before returning the first item (`OrderBy`, `GroupBy`, `Reverse`), so they hold everything in memory.
27. Each one enumerates the `IEnumerable` again, so the database query runs **three times**: three round trips, three times the load, and the numbers may disagree if data changes in between. Fix: `ToList()` once (or compute the aggregates in one database query). Tools: Rider/ReSharper warning, analyzer CA1851.
28. The query is deferred, so it runs when the caller enumerates it, after the `using` block disposed the `DbContext`: `ObjectDisposedException`. Fix: materialize inside the method (`return db.Orders.Where(...).ToList();`) and return `IReadOnlyList<Order>`, or let DI manage the context so it lives for the whole request.
29. `IEnumerable<T>` runs LINQ to Objects in memory; `Enumerable.Where` takes a compiled `Func<T, bool>`. `IQueryable<T>` holds an expression tree and a provider; `Queryable.Where` takes an `Expression<Func<T, bool>>` that the provider (EF Core) translates to SQL. `IQueryable<T>` inherits `IEnumerable<T>`.
30. By the **static (compile-time) type** of the expression `Where` is called on: `IQueryable<T>` binds to `Queryable.Where`, `IEnumerable<T>` binds to `Enumerable.Where`. Silent ways into memory: (1) `AsEnumerable()` or `ToList()` too early; (2) passing the query to a method with an `IEnumerable<T>` parameter or storing it in an `IEnumerable<T>` variable; (3) passing a `Func<T, bool>` variable to `Where`. In each case every operator **after** that point runs in C#, and the SQL contains only what was composed while the query was still `IQueryable`. Directly on a `DbSet`, that's every row and column with no `WHERE`. After `db.Products.Where(category)`, the category filter is still in the SQL and only the later operators run in memory.
31. Since EF Core 3.0 it throws `InvalidOperationException` ("The LINQ expression ... could not be translated"), except in the top-level projection (the final `Select` that shapes the results), where it can run C# code on the returned rows. A `Select` in the middle of the query or in a subquery must still be translatable. Before 3.0 it silently evaluated the untranslatable part in memory (only logging a warning), which caused hidden full-table loads.
32. For: callers can compose filters, sorting, paging and projections that all go to SQL efficiently. Against: it leaks EF Core into upper layers, callers can write untranslatable or N+1 queries, the query's lifetime is tied to the `DbContext`, and it's hard to know what SQL a repository method will produce or to test it. Common compromise: return materialized `IReadOnlyList<T>`/DTOs with explicit filter/paging parameters or specifications; compose `IQueryable` only inside the data layer.
33. Code represented as data: a tree of nodes (parameters, member access, operators, method calls) that can be inspected, translated or compiled. `Func<T, bool>` is compiled IL that can only be invoked. `Expression<Func<T, bool>>` is the description of that lambda, which a provider can translate to SQL, or which you can `Compile()` into a `Func`.
34. (1) The compiler turns lambdas passed to `Queryable` methods into expression-tree-building code. (2) Each operator (`Where`, `OrderBy`, `Select`) wraps the previous tree in a method-call node, producing one tree for the whole query. (3) On enumeration, EF Core checks its cache for a query with the same tree shape; if missing, it runs a pipeline of expression visitors that translate members to columns, operators to SQL operators, and captured variables to parameters, then generates SQL for the provider. (4) It runs the SQL with parameter values and materializes rows into objects (and tracks them if tracking is on).
35. A statement-bodied lambda `{ ... }` (CS0834) and the null-conditional operator `?.` (CS8072). (Also assignments and several newer constructs.)
36. `Expression.Constant(value)` puts the value into the SQL text as a literal. When the value varies a lot (prices, ids, search terms), each new value gives a different expression tree and SQL string. That hurts two separate caches: EF Core's query cache misses, so it re-translates each time, and SQL Server's plan cache fills with one-use plans (plan cache pollution, more CPU for compilation). Captured variables become parameters and share one translation and one plan. Fixed structural constants are fine. Better: chain `Where` calls on `IQueryable` (automatic parameters), or when building trees, reference the value through a captured object/closure so EF Core parameterizes it.

### Extension methods, tuples, pattern matching, iterators

37. A `static` method in a `static`, non-generic, non-nested class, with `this` before the first parameter; the namespace must be imported at the call site. `invoice.IsOverdue(today)` becomes `InvoiceExtensions.IsOverdue(invoice, today)`, a plain static call.
38. The instance method, always, with no warning. The extension is only considered when no applicable instance method exists.
39. Yes. It's a static call with `null` passed as the first argument, so nothing throws until the method body dereferences that argument. A method like `OrDefault(this string? value, string fallback)` can handle `null` safely.
40. No. The method is chosen at compile time from the declared type of the expression. An extension for `CardPayment` isn't called through a `Payment` variable, and extension methods can't be overridden.
41. `System.Tuple` (.NET 4) is an immutable **class** (heap allocated) with read-only `Item1..` properties. `System.ValueTuple` (C# 7) is a mutable **struct** with public fields, supports element names and deconstruction syntax, and avoids heap allocation.
42. No. Names are compile-time only (recorded in a `TupleElementNames` attribute on signatures); the runtime type is `ValueTuple<int, decimal>` with fields `Item1`, `Item2`. `System.Text.Json` serializes public properties by default and `ValueTuple` has only fields, so the output is `{}` (verified on .NET 10). Even with `IncludeFields = true` you get `Item1`/`Item2`, not your names. Return a record or DTO from APIs.
43. Add `public void Deconstruct(out string city, out string pin)` to the class (or as an extension method). Positional records get one automatically.
44. The compiler warns (CS8509, or CS8524 for unnamed enum values) if it can see an unhandled input, and at runtime an unmatched value throws `SwitchExpressionException`. Protect with a final `_ => throw new NotSupportedException($"...")` arm that names the unexpected case.
45. Arms are checked top to bottom and the first match wins. `CardPayment c` matches every card, including international ones, so the international rule is never reached for them and they get the domestic fee. Here the compiler reports CS8510 (unreachable arm) because the later arm is fully covered; with partial overlaps it can't always tell. Put specific rules first.
46. An enum variable can hold any value of its underlying type, e.g. `(OrderState)7` read from a database or JSON. Covering all named values doesn't cover those, so the compiler warns CS8524 and suggests the unnamed value. Add a `_ => throw` arm.
47. When the set of types is **closed** and the logic belongs to one consumer rather than to the types (a fee table owned by finance, mapping domain errors to HTTP status codes, parsing). When the set of types is **open** and new ones are added often, put the behaviour on the types (virtual method) or in strategies chosen by DI, so adding a type doesn't require editing every switch (Open/Closed Principle).
48. It makes the method an iterator: it hands one item to the caller and pauses, resuming at the same point on the next request. Calling the method runs **none** of its body; it only creates the compiler-generated state machine object. The body starts on the first `MoveNext()` and restarts for each new enumeration.
49. The validation runs on the first `MoveNext`, not when the method is called, so a bad argument surfaces later and somewhere else, which makes bugs confusing. Fix: a normal (non-iterator) method that validates and then returns a local iterator function (the pattern .NET's LINQ operators use).
50. Yes. `foreach` calls `Dispose()` on the enumerator when it exits for any reason (end, `break`, exception), and `Dispose` runs any pending `finally` blocks in the iterator. If you enumerate manually and never dispose, the `finally` doesn't run.

### Attributes, reflection, modern C#

51. A class deriving from `System.Attribute` that attaches metadata to code; it's stored in the assembly. It does **nothing by itself**: some code (framework or yours) must read it with reflection or a source generator and act on it. `[Sensitive]` masks nothing unless your logging code checks for it.
52. Slower than direct calls and allocates (each attribute read creates a new instance); no compile-time checking (string names fail at runtime); breaks encapsulation (can read private members); unfriendly to trimming and Native AOT. Source generators do the work at compile time (System.Text.Json source gen, `[LoggerMessage]`, `[GeneratedRegex]`), so there's no runtime cost and the code is trim/AOT-safe. Otherwise cache reflection results per type.
53. Constructor parameters declared on the class itself (`class InvoiceService(ITaxCalculator tax)`), in scope throughout the body (C# 12). Gotcha: when members use them, they're captured into hidden fields that are **not `readonly`**, so any method can reassign them. Also, initializing a property from a parameter and using the parameter elsewhere stores it twice (warning CS9124). Copy into a `readonly` field when it matters.
54. `init` means the property can be set only during object initialization. `required` means the caller **must** set it in the object initializer (CS9035 otherwise) unless the constructor has `[SetsRequiredMembers]`. `required` is compile-time only; the runtime doesn't enforce it, but `System.Text.Json` (since .NET 7) throws `JsonException` when a required property is missing from the JSON.
55. A `Program.cs` with statements directly at the top level (C# 9); the compiler generates the `Program` class and `Main`. The generated class is `internal`, so on .NET 8 and earlier, integration tests using `WebApplicationFactory<Program>` need `public partial class Program { }` (or `InternalsVisibleTo` for the test project). In .NET 10 ASP.NET Core projects that use top-level statements, a source generator (`PublicProgramSourceGenerator`) emits that declaration automatically (not in console apps or libraries) (verified: `typeof(Program).IsPublic` is `True` on `net10.0`, `False` on `net8.0`).
56. It's a collection expression with the spread operator (C# 12): it creates a new collection containing all items of `baseRoles` followed by `"Admin"`. The target type (array, `List<T>`, `HashSet<T>`, span...) decides what's built.
57. In C# 14, `field` inside a property accessor refers to the compiler-generated backing field. Use case: `public string Email { get; set => field = value.Trim().ToLowerInvariant(); }`: normalization or validation in a setter without declaring a private field.
58. A dedicated lock type (.NET 9) whose only job is mutual exclusion; `lock (_lock)` compiles to `using (_lock.EnterScope())` instead of `Monitor.Enter/Exit`. It's clearer, can't be mixed up with a general object, and is optimized by the runtime. If you convert it to `object` and lock on that, it falls back to `Monitor` locking and the compiler warns CS9216.
59. Extension **properties** (`amounts.GrandTotal`), **static** extension members called on the type (`decimal.FromPaise(...)`), and extension operators, all declared in an `extension(Receiver r) { ... }` block. Classic extensions only allow instance-style methods.
60. C# 14 null-conditional assignment: if `cart.Owner` is null nothing happens (and the right side isn't evaluated); otherwise `LastOrderAt` is assigned. `?.` works with `=` and compound assignment like `+=`, but **not** with `++`/`--` (`cart.Owner?.OrderCount++` is error CS1059, verified on .NET 10).

## Part B — What does this print?

**B1.** `444`. All three lambdas capture the same `for` variable `i`, which is `4` after the loop ends (the value that failed `i <= 3`).

**B2.** `50`. Both methods run, but a multicast `Func` returns only the **last** method's result: `5 * 10`. The `x + 1` result is thrown away.

**B3.** `2`. The query is deferred, so it runs at `Count()` and sees `900m` added after it was defined: `250` and `900` match.

**B4.**
```text
0
True
```
`FirstOrDefault` on `List<int>` with no match returns `default(int)` = 0. `All` on an empty list is `true`.

**B5.** `XA1`. Calling `Numbers()` runs nothing, so `X` prints first. `First()` runs the body up to the first `yield return` (prints `A`) and returns `1`. It never resumes, so `B` and `C` never print.

**B6.** `threw`. Two matches: `SingleOrDefault` returns `null` only for zero matches and throws `InvalidOperationException` for more than one.

**B7.**
```text
True
(KB-01, 2)
```
Tuple `==` compares elements in order and ignores names. `ToString()` shows no names because names don't exist at runtime.

**B8.** `low low mid`. `5000` isn't `< 5000` and isn't `> 5000`, so it falls to `_`. `1000` isn't `> 1000`. `4999` is in the `> 1000 and < 5000` range. (A real boundary bug: decide whether 5000 is "mid" or "high" and use `>=`.)

**B9.** `2,6,9`. The lambda reads `minQty` when the query runs, and by then it's `1`.

**B10.** `[1:order][caught]`. The first handler runs, the second throws, so the third never runs and the exception reaches the caller's `catch`.

## Part C — Find the bug

**C1.** `products` is typed `IEnumerable<Product>`, so `Where`, `Skip`, `Take` and `Select` are LINQ to Objects. EF Core runs `SELECT` of every column of the **whole** `Products` table, and filtering and paging happen in memory on every request. There's also no `OrderBy`, so paging is non-deterministic. Fix: keep it `IQueryable<Product>`, add `OrderBy(p => p.Id)`, project to the DTO in the query, and return a materialized `IReadOnlyList<ProductDto>` via `ToListAsync()`.

**C2.** On the first day of the month (before any orders), `thisMonth` is empty and `Max` on an empty non-nullable sequence throws `InvalidOperationException`. Fix: `thisMonth.Max(o => (decimal?)o.Total)` and handle `null`, or check `Any()`. (Separately, `o.PlacedOn.Month == today.Month` also matches the same month of **previous years**: compare year and month, or use a date range, which also lets the database use an index.)

**C3.** (1) **Memory leak:** a singleton `PriceFeed` holds a reference to every `StockWatcher` that subscribed, and nothing ever unsubscribes, so watchers are never collected. Implement `IDisposable` and do `feed.PriceChanged -= OnPriceChanged` (and keep the `feed` reference to do so). (2) **`async void` handler:** if `SendAsync` throws, the publisher can't await the handler, so it can't catch the exception. In an ASP.NET Core app there's no `SynchronizationContext`, so the exception is rethrown on the thread pool, which by default crashes the process. (In a UI app it would go to the UI thread's unhandled-exception handler instead.) Wrap the body in `try/catch` and log, or move the work to a queue/`Channel<T>` processed by a background service.

**C4.** `All` returns `true` for an empty `Payments` collection, so orders with no payments pass the check. Use `order.Payments.Any() && order.Payments.All(p => p.Status == "Succeeded")`. Better still, compare the total of succeeded payments to the order total, since "all succeeded" also ignores partial payments.

**C5.** `isOverdue` is a `Func<Order, bool>`, so `_db.Orders.Where(isOverdue)` binds to `Enumerable.Where`, not `Queryable.Where`. EF Core loads the entire `Orders` table and filters in memory. Fix: declare it as `Expression<Func<Order, bool>> isOverdue = o => !o.IsPaid && o.DueDate < today;` so it's translated to SQL, and use `ToListAsync()`. (Note: with the `Func`, `ToListAsync()` isn't even available, because the result is a plain `IEnumerable<Order>`. That's a useful hint when you see `.AsQueryable()` added to "fix" a compile error.)
