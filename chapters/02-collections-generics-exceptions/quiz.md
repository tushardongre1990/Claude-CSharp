# Chapter 2 Quiz — Collections, generics and exceptions

Answer every question **without looking at the notes**. Write your answers down, then check them against [quiz-answers.md](quiz-answers.md). Every wrong or partial answer goes into [MISTAKES.md](../../MISTAKES.md).

Questions marked ⭐ are at 5-years-of-experience depth.

## Part A — Concepts

1. What's the difference between an array and a `List<T>`? What happens inside a `List<T>` when you add an item and it's full?
2. What's the difference between `Count` and `Capacity`? When would you pass a capacity to the constructor?
3. Why does removing an item from a list inside `foreach` throw? Give two correct ways to do it.
4. Explain step by step how `Dictionary<TKey,TValue>` finds a value by key.
5. What is a hash collision? How does .NET handle it, and what happens to performance when there are many?
6. What happens if `GetHashCode()` returns the same value for every key? Does the dictionary still work? ⭐
7. What happens if you change a property of an object after using it as a dictionary key?
8. `ContainsKey` + indexer vs `TryGetValue`: which is better and why? What's the difference between `Add`, the indexer setter and `TryAdd`?
9. Can you rely on the enumeration order of a `Dictionary`? Why or why not? ⭐
10. Why should you never store `string.GetHashCode()` in a database? ⭐
11. FIFO vs LIFO: give a backend example of each.
12. When is `LinkedList<T>` actually a good choice? Why is it often slower than `List<T>` in practice? ⭐
13. `SortedDictionary` vs `SortedList`: what's the difference?
14. Does `PriorityQueue<TElement,TPriority>` return items with equal priority in insertion order? How do you make it fair? ⭐
15. How would you build an LRU cache with built-in collections? ⭐
16. Explain the difference between `IEnumerable<T>`, `ICollection<T>`, `IList<T>` and `IReadOnlyList<T>`.
17. Does `IList<T>` inherit from `IReadOnlyList<T>`? ⭐
18. What should a repository method return, and why? What should a public method accept as a parameter?
19. What is deferred execution? Why can enumerating an `IEnumerable<T>` twice be a problem?
20. Is `IReadOnlyList<T>` immutable? How can a caller still change it, and how do you prevent that? ⭐
21. Your nightly job does `foreach (var line in lines) if (products.Any(p => p.Id == line.ProductId)) ...` with 50,000 of each. What's the complexity, and how do you fix it?
22. What does "amortized O(1)" mean?
23. What problems do generics solve? Use `ArrayList` vs `List<T>` to explain.
24. List five generic constraints and what each lets you do.
25. Are `List<int>` and `List<string>` the same type at runtime? How does the JIT handle generic code for value types vs reference types? ⭐
26. How do you compare two values of an unconstrained generic type `T` for equality?
27. Explain covariance and contravariance with one example each.
28. Why can't you assign a `List<CardPayment>` to a `List<Payment>` variable? What can you use instead?
29. What is array covariance, and why is it considered a design mistake? ⭐
30. Why doesn't `IEnumerable<int>` convert to `IEnumerable<object>`? ⭐
31. Why is `Dictionary` unsafe to use from multiple threads? What could happen? ⭐
32. Can the value factory passed to `ConcurrentDictionary.GetOrAdd` run more than once? What's the fix? ⭐
33. Is `ConcurrentDictionary` enough to make "check stock, then reserve" safe? Explain.
34. `BlockingCollection<T>` vs `Channel<T>`: which would you use in ASP.NET Core and why? ⭐
35. Does `finally` always run? Name cases where it doesn't. ⭐
36. What happens if you put `catch (ArgumentException)` before `catch (ArgumentNullException)`?
37. What happens to the original exception if `finally` throws?
38. `ArgumentException` vs `InvalidOperationException`: when do you use each?
39. When should you create a custom exception? What should it contain?
40. What is an exception filter? How is it different from catching and rethrowing? In what order do a filter and an inner `finally` block run? ⭐
41. Difference between `throw;`, `throw ex;` and `throw new X("...", ex);`?
42. What is `ExceptionDispatchInfo` for? ⭐
43. When should you throw an exception, and when should you return a Result? Give examples of each.
44. Why are exceptions a poor fit for validating user input?
45. If .NET has a garbage collector, why do we need `IDisposable`?
46. What does a `using` declaration compile to? In what order are several `using` objects disposed?
47. When do you need a finalizer? What is `SafeHandle` for? ⭐
48. Should you dispose an `HttpClient` after every request? Should you dispose a `DbContext` that was injected into your service? ⭐
49. What's wrong with logging and rethrowing an exception in every layer?
50. Write the correct `ILogger` call to log a failed checkout for an order, and explain why it's better than `logger.LogError(ex.Message)` and `logger.LogError($"... {orderId}")`.

## Part B — What does this code print?

**51.**
```csharp
var orderIds = new List<int>();
for (int i = 0; i < 5; i++) orderIds.Add(i);
Console.WriteLine($"{orderIds.Count} {orderIds.Capacity}");
```

**52.**
```csharp
var stock = new Dictionary<string, int>();
stock["SKU-1"] = 1;
stock["SKU-1"] = 2;
Console.WriteLine($"{stock.Count} {stock["SKU-1"]}");
try { stock.Add("SKU-1", 3); } catch (Exception ex) { Console.WriteLine(ex.GetType().Name); }
```

**53.**
```csharp
var processed = new HashSet<string> { "pay_1" };
Console.WriteLine($"{processed.Add("pay_1")} {processed.Add("PAY_1")} {processed.Count}");
```

**54.**
```csharp
int calls = 0;
IEnumerable<int> LoadOrderIds() { calls++; yield return 1; yield return 2; }

var ids = LoadOrderIds();
Console.WriteLine(calls);
var count = ids.Count();
var sum = ids.Sum();
Console.WriteLine(calls);
```

**55.**
```csharp
static string Ship()
{
    try { Console.WriteLine("try"); return "shipped"; }
    finally { Console.WriteLine("finally"); }
}
Console.WriteLine(Ship());
```

**56.**
```csharp
try { throw new TimeoutException("gateway"); }
catch (Exception ex) when (ex is ArgumentException) { Console.WriteLine("A"); }
catch (TimeoutException) { Console.WriteLine("B"); }
catch (Exception) { Console.WriteLine("C"); }
```

**57.** ⭐
```csharp
object[] invoiceNumbers = new string[2];
try { invoiceNumbers[0] = 42; }
catch (Exception ex) { Console.WriteLine(ex.GetType().Name); }
```

**58.**
```csharp
class Res(string name) : IDisposable
{
    public void Dispose() => Console.WriteLine($"dispose {name}");
}

using (var conn = new Res("connection"))
using (var cmd = new Res("command"))
{
    Console.WriteLine("query");
}
```

**59.** ⭐
```csharp
class Counter<T> { public static int Count; }

Counter<int>.Count++;
Counter<string>.Count++;
Counter<int>.Count++;
Console.WriteLine($"{Counter<int>.Count},{Counter<string>.Count}");
```

**60.**
```csharp
try
{
    try { throw new IOException("disk full"); }
    finally { throw new InvalidOperationException("cleanup failed"); }
}
catch (Exception ex) { Console.WriteLine(ex.Message); }
```

**61.** ⭐
```csharp
class SessionKey
{
    public int UserId { get; set; }
    public override bool Equals(object? o) => o is SessionKey k && k.UserId == UserId;
    public override int GetHashCode() => UserId.GetHashCode();
}

var key = new SessionKey { UserId = 7 };
var sessions = new Dictionary<SessionKey, string> { [key] = "cart-123" };
key.UserId = 8;
Console.WriteLine($"{sessions.ContainsKey(key)} {sessions.Count}");
```

**62.**
```csharp
var queue = new Queue<string>(); queue.Enqueue("A"); queue.Enqueue("B");
var stack = new Stack<string>(); stack.Push("A"); stack.Push("B");
Console.WriteLine($"{queue.Dequeue()} {stack.Pop()}");
```

## Part C — Spot the bug

For each snippet: what's wrong, what happens in production, and how do you fix it?

**63.**
```csharp
foreach (var order in orders)
    if (order.Status == OrderStatus.Cancelled)
        orders.Remove(order);
```

**64.** ⭐
```csharp
private static readonly ConcurrentDictionary<string, int> Stock = new();

public bool Reserve(string sku)
{
    if (Stock[sku] > 0)
    {
        Stock[sku] = Stock[sku] - 1;
        return true;
    }
    return false;
}
```

**65.**
```csharp
catch (Exception ex)
{
    _logger.LogWarning("Payment failed, retrying later");
    throw ex;
}
```

**66.**
```csharp
public bool CapturePayment(int orderId, decimal amount)
{
    try { _gateway.Charge(orderId, amount); }
    catch { }
    return true;
}
```

**67.** ⭐
```csharp
public async Task<decimal> GetShippingRateAsync(string pincode)
{
    using var client = new HttpClient();
    var json = await client.GetStringAsync($"https://rates.example.com/{pincode}");
    return decimal.Parse(json);
}
```

**68.**
```csharp
public class Order
{
    public List<OrderLine> Lines { get; set; } = new();
    public decimal Total => Lines.Sum(l => l.Quantity * l.UnitPrice);
}
```

**69.**
```csharp
catch (Exception ex)
{
    _logger.LogError($"Checkout failed for order {orderId}: {ex.Message}");
    throw;
}
```

**70.** ⭐
```csharp
public IEnumerable<Customer> GetActiveCustomers() =>
    _db.Customers.Where(c => c.IsActive);   // EF Core query

// caller
var customers = _repo.GetActiveCustomers();
if (customers.Any())
{
    _logger.LogInformation("Sending to {Count} customers", customers.Count());
    foreach (var c in customers) await _email.SendAsync(c.Email);
}
```

**71.** ⭐
```csharp
private readonly Dictionary<string, decimal> _rateCache = new();   // inside a singleton service

public decimal GetRate(string currency)
{
    if (!_rateCache.TryGetValue(currency, out var rate))
    {
        rate = _rateApi.Fetch(currency);
        _rateCache[currency] = rate;
    }
    return rate;
}
```

**72.**
```csharp
public class InsufficientStockException : Exception
{
    public InsufficientStockException(string sku)
        : base("Not enough stock") { }
}

// ...
catch (DbException ex)
{
    throw new InsufficientStockException(sku);
}
```
