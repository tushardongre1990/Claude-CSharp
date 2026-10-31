# Chapter 3 Quiz — LINQ, delegates and advanced C#

Answer every question **without looking at the notes**. Write your answers down, then check them against [quiz-answers.md](quiz-answers.md). Every wrong or partial answer goes into [MISTAKES.md](../../MISTAKES.md).

Questions marked ⭐ are at 5-years-of-experience depth.

## Part A — Concepts

### Delegates, lambdas, events

1. What is a delegate? What does a delegate object store inside it?
2. What's the difference between `Action`, `Func` and `Predicate<T>`? In `Func<Order, int, decimal>`, which type is the return type?
3. A multicast `Func<decimal, decimal>` has three methods. What does calling it return? What happens if the second method throws?
4. Why does `onShipped -= id => SendEmail(id);` not remove the handler that was added with the same lambda?
5. What is a closure? Is the variable's value or the variable itself captured?
6. Explain the loop-variable capture trap. Why does `foreach` not have the problem but `for` does?
7. What is a `static` lambda, and why would you use one? ⭐
8. What's the difference between an event and a public delegate field?
9. Are event handlers called synchronously or asynchronously? What happens to the publisher if a handler throws?
10. How can events cause memory leaks? Give a backend example and the fix. ⭐
11. Would you use C# events to notify another microservice that an order was placed? Why or why not? ⭐

### LINQ operators

12. What's the difference between `Select` and `SelectMany`? Give a real example.
13. What's wrong with `orders.OrderBy(o => o.City).OrderBy(o => o.Total)`?
14. Is `OrderBy` stable? Is `List<T>.Sort` stable?
15. How do you implement paging with LINQ? What goes wrong if you forget `OrderBy`?
16. What's the difference between `Join` and `GroupJoin`? How do you write a left join (before and after .NET 10)?
17. What's the time complexity of LINQ-to-Objects `Join`, and why? ⭐
18. What do `Sum`, `Average` and `Max` return for an empty `List<decimal>`? How do you make `Max` safe?
19. `GroupBy` vs `ToLookup`: what's the difference?
20. What happens with `new[] { int.MaxValue, 1 }.Sum()`? ⭐
21. What's the difference between `First`, `FirstOrDefault`, `Single` and `SingleOrDefault`? Does `SingleOrDefault` ever throw?
22. What does `retryDelays.FirstOrDefault(ms => ms > 5000)` return on a `List<int>` with no match? Why is that dangerous and how do you fix it?
23. What does `payments.All(p => p.Succeeded)` return for an empty list? Describe a bug this can cause.
24. Why prefer `Any()` over `Count() > 0`? What SQL does each produce in EF Core? ⭐

### Deferred execution, IQueryable, expression trees

25. What is deferred execution? Name three deferred and three immediate operators.
26. What's the difference between streaming and buffering operators? Give an example of each.
27. A method calls `.Count()`, `.Sum()` and `foreach` on the same `IEnumerable<Order>` that comes from an EF Core query. What happens?
28. A repository method does `using var db = ...; return db.Orders.Where(o => o.IsPaid);` What happens when the caller loops over it, and how do you fix it?
29. What's the difference between `IEnumerable<T>` and `IQueryable<T>`? Which `Where` method does each use?
30. How does the compiler decide whether a `Where` runs in SQL or in memory? Give three ways a query silently ends up running in memory. ⭐
31. What does EF Core do when it can't translate part of a `Where` to SQL? How was it different before EF Core 3.0? ⭐
32. Should a repository return `IQueryable<T>`? Give both sides. ⭐
33. What is an expression tree? What's the difference between `Func<T, bool>` and `Expression<Func<T, bool>>`?
34. Explain how EF Core turns a LINQ query into SQL, step by step. ⭐
35. Name two C# constructs that can't appear in an expression tree lambda.
36. Why can a hand-built expression filter using `Expression.Constant` hurt SQL Server performance? What's the better approach? ⭐

### Extension methods, tuples, pattern matching, iterators

37. What are the rules for declaring an extension method? What does the compiler turn `invoice.IsOverdue(today)` into?
38. A class has an instance method with the same signature as an extension method. Which one is called?
39. Can you call an extension method on a `null` reference? What happens?
40. Are extension methods polymorphic? ⭐
41. `System.Tuple` vs `System.ValueTuple`: what's the difference?
42. Are tuple element names available at runtime? What does `JsonSerializer.Serialize((Count: 3, Total: 99m))` produce? ⭐
43. How do you make your own class support `var (city, pin) = address;`?
44. In a switch expression, what happens if no arm matches? How do you protect against it?
45. What's wrong if `CardPayment c => ...` appears before `CardPayment { IsInternational: true } c => ...`?
46. Why does a switch over every named value of an enum still produce a "not exhaustive" warning? ⭐
47. When would you choose pattern matching over virtual methods (polymorphism)? ⭐
48. What does `yield return` do? When does the code inside an iterator method start running?
49. Why can argument validation inside an iterator be a problem? How do you fix it?
50. Does a `finally` block inside an iterator run if the caller `break`s out of `foreach`? Why?

### Attributes, reflection, modern C#

51. What is an attribute? Does `[Sensitive]` on a property do anything by itself?
52. What are three downsides of reflection, and how do source generators help? ⭐
53. What are C# 12 primary constructors? What's the gotcha with their parameters?
54. What's the difference between `required` and `init`? Is `required` enforced at runtime?
55. What are top-level statements, and how do they affect `WebApplicationFactory<Program>` tests?
56. What does `[.. baseRoles, "Admin"]` do? Which C# version added it?
57. What is the C# 14 `field` keyword? Give a use case.
58. What does `System.Threading.Lock` give you over `lock (new object())`? What happens if you cast it to `object` first? ⭐
59. What can C# 14 extension blocks do that classic extension methods can't?
60. What does `cart.Owner?.LastOrderAt = DateTime.UtcNow;` do? Can you write `cart.Owner?.OrderCount++`?

## Part B — What does this print?

Predict the output, then run the code to check.

**B1.**
```csharp
var jobs = new List<Action>();
for (int i = 1; i <= 3; i++)
    jobs.Add(() => Console.Write(i));
foreach (var job in jobs) job();
```

**B2.**
```csharp
Func<int, int> pipeline = x => x + 1;
pipeline += x => x * 10;
Console.WriteLine(pipeline(5));
```

**B3.**
```csharp
var orders = new List<decimal> { 100m, 250m };
var big = orders.Where(t => t > 150m);
orders.Add(900m);
Console.WriteLine(big.Count());
```

**B4.**
```csharp
var ids = new List<int> { 3, 7, 9 };
Console.WriteLine(ids.FirstOrDefault(x => x > 100));
Console.WriteLine(new List<string>().All(s => s.Length > 100));
```

**B5.**
```csharp
IEnumerable<int> Numbers()
{
    Console.Write("A");
    yield return 1;
    Console.Write("B");
    yield return 2;
    Console.Write("C");
}
var seq = Numbers();
Console.Write("X");
Console.Write(seq.First());
```

**B6.**
```csharp
var users = new[] { "asha@shop.in", "ravi@shop.in", "asha@shop.in" };
try { Console.WriteLine(users.SingleOrDefault(u => u.StartsWith("asha"))); }
catch (InvalidOperationException) { Console.WriteLine("threw"); }
```

**B7.**
```csharp
var a = (Sku: "KB-01", Qty: 2);
var b = (Code: "KB-01", Count: 2);
Console.WriteLine(a == b);
Console.WriteLine(a);
```

**B8.**
```csharp
static string Fee(int amount) => amount switch
{
    > 1000 and < 5000 => "mid",
    > 5000 => "high",
    _ => "low",
};
Console.WriteLine($"{Fee(5000)} {Fee(1000)} {Fee(4999)}");
```

**B9.**
```csharp
int minQty = 5;
var stock = new List<int> { 2, 6, 9 };
var q = stock.Where(s => s >= minQty);
minQty = 1;
Console.WriteLine(string.Join(",", q));
```

**B10.** ⭐
```csharp
Action<string>? notify = null;
notify += s => Console.Write($"[1:{s}]");
notify += s => throw new Exception("down");
notify += s => Console.Write($"[3:{s}]");
try { notify("order"); } catch { Console.Write("[caught]"); }
```

## Part C — Find the bug

**C1.** This endpoint is slow in production. Why?
```csharp
public IEnumerable<ProductDto> Search(string category, int page)
{
    IEnumerable<Product> products = _db.Products;
    return products.Where(p => p.Category == category)
                   .Skip((page - 1) * 20).Take(20)
                   .Select(p => new ProductDto(p.Id, p.Name));
}
```

**C2.** Monthly dashboard crashes on the first day of each month:
```csharp
var thisMonth = orders.Where(o => o.PlacedOn.Month == today.Month).ToList();
var biggest = thisMonth.Max(o => o.Total);
```

**C3.** Two bugs here:
```csharp
public class StockWatcher
{
    public StockWatcher(PriceFeed feed) => feed.PriceChanged += OnPriceChanged;   // PriceFeed is a singleton
    private async void OnPriceChanged(object? s, PriceChangedEventArgs e) => await _hub.SendAsync(e);
}
```

**C4.** Orders with no payments get marked as paid:
```csharp
if (order.Payments.All(p => p.Status == "Succeeded"))
    order.MarkPaid();
```

**C5.** ⭐ A reusable filter makes this query load the whole table:
```csharp
Func<Order, bool> isOverdue = o => !o.IsPaid && o.DueDate < today;
var overdue = _db.Orders.Where(isOverdue).ToList();
```
