# Chapter 1 Quiz — C# fundamentals and OOP

Answer every question **without looking at the notes**. Write your answers down, then check them against [quiz-answers.md](quiz-answers.md). Every wrong or partial answer goes into [MISTAKES.md](../../MISTAKES.md).

Questions marked ⭐ are at 5-years-of-experience depth.

## Part A — Concepts

1. What's the difference between an implicit and an explicit conversion? Give one example of each.
2. Why should money be stored as `decimal` and not `double`?
3. What does `Math.Round(2.5)` return, and why? ⭐
4. Is `var` dynamically typed? How is it different from `dynamic`?
5. What's the difference between a value type and a reference type?
6. "Value types are stored on the stack." Is this true? Explain. ⭐
7. Why are strings immutable in .NET? Give at least two reasons. ⭐
8. When should you use `StringBuilder`, and when is it unnecessary?
9. How should you compare two strings case-insensitively, and what's wrong with `a.ToLower() == b.ToLower()`?
10. Do nullable reference types prevent `NullReferenceException` at runtime?
11. What's the difference between `ref` and `out`?
12. Why can't an `async` method have `ref` or `out` parameters? ⭐
13. What's the difference between `const` and `static readonly`? Why does it matter for libraries? ⭐
14. Does `readonly List<string> Items` make the list immutable?
15. What happens if a static constructor throws an exception? ⭐
16. What's the difference between a class, a struct and a record?
17. Is `record with { ... }` a deep copy?
18. Name two dangers of enums in APIs and databases.
19. In what order do constructors and field initializers run when creating a derived class for the first time? ⭐
20. Why is calling a virtual method from a base class constructor dangerous? ⭐
21. Explain the four pillars of OOP with one example each.
22. When would you choose an interface over an abstract class? When the reverse?
23. What's the difference between overriding and method hiding?
24. What does "favour composition over inheritance" mean? Give a backend example.
25. Explain each SOLID principle in one sentence.
26. What's the difference between Dependency Inversion, Dependency Injection and Inversion of Control? ⭐
27. What's the Equals/GetHashCode contract? What breaks if you override only `Equals`?
28. Why should you implement `IEquatable<T>` on a struct? ⭐
29. What does `CompareTo` return, and why is `return a - b;` a bad implementation?
30. What is boxing? Why is it a performance concern? Name two places it happens without you noticing. ⭐

## Part B — What does this code print?

**31.**
```csharp
struct StockLevel { public int Quantity; }
var warehouseA = new StockLevel { Quantity = 1 };
var warehouseB = warehouseA;
warehouseB.Quantity = 5;
Console.WriteLine(warehouseA.Quantity);
```

**32.**
```csharp
class Cart { public int ItemCount; }
void ClearCart(Cart cart) { cart = new Cart { ItemCount = 0 }; }
var userCart = new Cart { ItemCount = 7 };
ClearCart(userCart);
Console.WriteLine(userCart.ItemCount);
```

**33.**
```csharp
class NotificationSender
{
    public virtual string Channel() => "Generic";
    public string RetryPolicy() => "No retries";
}
class SmsSender : NotificationSender
{
    public override string Channel() => "SMS";
    public new string RetryPolicy() => "3 retries";
}

NotificationSender sender = new SmsSender();
Console.WriteLine(sender.Channel() + " / " + sender.RetryPolicy());
```

**34.**
```csharp
string s = "  hello  ";
s.Trim();
Console.WriteLine($"[{s}]");
```

**35.**
```csharp
int? n = null;
Console.WriteLine(n ?? -1);
Console.WriteLine(n.HasValue);
Console.WriteLine((n + 5)?.ToString() ?? "null");
```

**36.**
```csharp
record Person(string Name, List<string> Tags);
var p1 = new Person("A", new List<string> { "x" });
var p2 = p1 with { Name = "B" };
p2.Tags.Add("y");
Console.WriteLine(p1.Tags.Count);
Console.WriteLine(p1 == new Person("A", new List<string> { "x", "y" }));
```

**37.** ⭐
```csharp
object o = 10;
long l = (long)o;
Console.WriteLine(l);
```

**38.**
```csharp
Console.WriteLine((int)7.9);
Console.WriteLine(7 / 2);
Console.WriteLine(7 / 2.0);
```

**39.** ⭐
```csharp
class RepositoryBase
{
    public RepositoryBase() { Console.WriteLine($"SELECT * FROM {TableName()}"); }
    public virtual string TableName() => "Unknown";
}
class InvoiceRepository : RepositoryBase
{
    private readonly string _table;
    public InvoiceRepository() { _table = "Invoices"; }
    public override string TableName() => _table ?? "NULL";
}

new InvoiceRepository();
```

**40.**
```csharp
enum Status { Active, Inactive }
var s = (Status)5;
Console.WriteLine(s);
Console.WriteLine(Enum.IsDefined(s));
```

## Part C — Spot the bug

**41.** This code adds users to a `HashSet<User>`, but `Contains` returns false for a user with the same Id. Why?
```csharp
class User
{
    public int Id { get; set; }
    public override bool Equals(object? obj) => obj is User u && u.Id == Id;
}
```

**42.** What's wrong with this, and how would you fix it?
```csharp
public class PriceCalculator
{
    public const decimal GstRate = 0.18m;   // in a shared NuGet library used by 12 services
}
```

**43.** ⭐ What's wrong with this in an ASP.NET Core application?
```csharp
public static class OrderCache
{
    public static List<Order> RecentOrders = new();
}
```

**44.** What's the design problem here?
```csharp
public class OrderService
{
    private readonly SqlOrderRepository _repo = new SqlOrderRepository();
    private readonly SmtpEmailSender _email = new SmtpEmailSender();
}
```

**45.** ⭐ What's wrong with this `CompareTo`?
```csharp
public int CompareTo(Score other) => this.Value - other.Value;
```
