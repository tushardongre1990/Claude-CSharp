# Chapter 1 Exercises

Do these **in code**. Create a new console app for them (`dotnet new console -o Chapter01Exercises` inside this chapter's `code/` folder), or paste them into a scratch project. Try each one before opening its hint or solution.

---

## Part 1 — Coding exercises

### Exercise 1: Safe money type
Create a `readonly struct Money` with `Amount` (decimal) and `Currency` (string).
- Adding two `Money` values with different currencies must throw.
- Implement `IEquatable<Money>`, `IComparable<Money>`, `Equals(object)`, `GetHashCode`, and the operators `+`, `==`, `!=`, `<`, `>`.
- `ToString()` returns e.g. `"1500.00 INR"` (always 2 decimal places).
- Rounding to 2 decimals must use `MidpointRounding.AwayFromZero`.

Test: `new Money(10.005m, "INR")` should print `10.01 INR`. Put `Money` values in a `HashSet<Money>` and check that `Contains` works.

<details><summary>Hint</summary>

Round in the constructor: `Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);`. Format with `Amount.ToString("F2")`.
</details>

### Exercise 2: Fix the broken equality
This class is used as a dictionary key and lookups fail. Fix it **properly**: equality based on `Email`, ignoring case.

```csharp
public class Customer
{
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public override bool Equals(object? obj) => obj is Customer c && c.Email == Email;
}
```

<details><summary>Hint</summary>

Three things: make `Email` immutable (`init`), compare with `StringComparison.OrdinalIgnoreCase`, and override `GetHashCode` using `StringComparer.OrdinalIgnoreCase.GetHashCode(Email)` so the hash agrees with the case-insensitive Equals.
</details>

### Exercise 3: Notification system (OOP + SOLID)
Build a notification system where:
- There are `Email`, `Sms` and `Push` notifications. Each formats its message differently.
- Every notification is logged before sending (shared behaviour).
- `NotificationService` sends a notification to a user **without knowing which concrete type** it's using.
- Adding a new `WhatsApp` notification must **not** require editing `NotificationService`.

Decide: interface, abstract class, or both? Write one or two sentences explaining your choice.

<details><summary>Hint</summary>

Expose an `INotificationChannel` interface (for DI and testing), plus an abstract `NotificationChannelBase : INotificationChannel` that implements the shared "log, then send" template and leaves `Format()` abstract. `NotificationService` takes `IEnumerable<INotificationChannel>` or a single channel through its constructor.
</details>

### Exercise 4: Override vs hide
Without running it, write down what this prints. Then run it and explain any surprises.

```csharp
class PaymentGateway
{
    public virtual string Provider() => "Generic gateway";
    public string FeePercent() => "2%";
}
class CardGateway : PaymentGateway
{
    public override string Provider() => "Card gateway";
    public new string FeePercent() => "1.8%";
}
class InternationalCardGateway : CardGateway
{
    public new string Provider() => "International card gateway";
}

PaymentGateway x = new InternationalCardGateway();
CardGateway y = new InternationalCardGateway();
InternationalCardGateway z = new InternationalCardGateway();
Console.WriteLine($"{x.Provider()} | {y.Provider()} | {z.Provider()}");
Console.WriteLine($"{x.FeePercent()} | {y.FeePercent()} | {z.FeePercent()}");
```

Then explain which of these results would be a real bug in a billing system, and how to fix it.

<details><summary>Hint</summary>

`InternationalCardGateway.Provider` *hides* (it doesn't override), so any variable typed `PaymentGateway` or `CardGateway` uses the virtual chain, which stops at `CardGateway`'s override. Only an `InternationalCardGateway`-typed variable sees the hidden version. `FeePercent` isn't virtual at all, so the variable type decides.
</details>

### Exercise 5: Deep copy an order
Create `Order` (Id, Customer, `List<OrderLine>`), `Customer` (Name, Address) and `OrderLine` (Product, Quantity), all classes with settable properties.
- Write `ShallowCopy()` and `DeepCopy()`.
- Prove with output that changing a line's quantity in the shallow copy affects the original, but doing the same in the deep copy doesn't.
- Bonus: rewrite all three as immutable records with `IReadOnlyList<OrderLine>`, and explain why deep copying is no longer needed.

### Exercise 6: Find the boxing
Rewrite this so it does **zero boxing**, and measure the before/after allocations with `GC.GetAllocatedBytesForCurrentThread()`:

```csharp
var list = new System.Collections.ArrayList();
for (int i = 0; i < 100_000; i++) list.Add(i);
long total = 0;
foreach (object o in list) total += (int)o;
```

---

## Part 2 — Scenario questions (5-YOE level)

Answer these out loud, as if in an interview. Aim for 1–2 minutes each, and include **trade-offs**.

1. **A teammate stored prices as `double`.** Finance reports that totals are off by a few paise across thousands of orders. Explain the cause, how you'd fix the code, and how you'd handle the data already stored in the database.

2. **A shared NuGet library exposes `public const int MaxPageSize = 50;`.** The team changes it to 100 and publishes a new version. Some services still cap at 50. Why? How would you design this instead?

3. **You've been asked to add a fourth payment provider.** The current code has a 200-line `switch (providerName)` in `PaymentService`. Walk through how you'd refactor it, which SOLID principles apply, and what you would *not* abstract.

4. **After enabling nullable reference types** in a large existing project, there are 1,200 warnings. How do you roll this out safely without stopping feature work? What risks does it introduce for EF Core?

5. **A performance profile shows millions of small allocations** in a hot API endpoint that processes a list of `struct OrderItem` values through an `IOrderItem` interface and puts them in a `HashSet`. What's likely happening, and how do you fix it?

6. **A code review shows `CashOnDeliveryPayment : Payment` with `CapturePaymentOnline()` throwing `NotSupportedException`**, plus a `BaseService` that 40 services inherit just for logging and a database helper. What are the problems, and what would you propose?

7. **An API accepts an `OrderStatus` enum in the request body.** A client sends `99`, and it gets saved. Later, someone inserts a new enum value in the middle of the list. Explain both bugs and how you'd prevent them.
