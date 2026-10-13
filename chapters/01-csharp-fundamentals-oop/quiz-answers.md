# Chapter 1 Quiz — Answers

Don't read this until you've written your own answers in [quiz.md](quiz.md).

## Part A — Concepts

1. **Implicit** conversion happens automatically when no data can be lost: `int i = 5; long l = i;`. **Explicit** conversion needs a cast because data may be lost: `int x = (int)9.99;` → 9.
2. `double` is binary floating point and can't represent many decimal fractions exactly (0.1 + 0.2 = 0.30000000000000004). `decimal` is base-10 and exact for money. The trade-off is that it's slower and has a smaller range.
3. **2.** The default is **banker's rounding** (`MidpointRounding.ToEven`): .5 values round to the nearest even number, which reduces cumulative bias. Use `MidpointRounding.AwayFromZero` when business rules require it.
4. No. `var` is resolved by the **compiler** and the variable is strongly typed forever. `dynamic` skips compile-time checking and resolves members at **runtime**, so mistakes become runtime exceptions.
5. A value-type variable holds the data itself, and assignment copies the data. A reference-type variable holds a reference to an object on the heap, and assignment copies the reference, so both variables point to the same object.
6. Only partly. Value types are stored **where they are declared**: locals usually live on the stack (or in registers), but fields of classes, array elements, and captured or async locals live on the heap. The real distinction is **copy semantics**, not location.
7. Thread safety (they can be shared across threads without locks), safe use as dictionary keys (a key can't change after insertion), security (validated values can't be altered), and interning (sharing one copy is only safe if it's unchangeable).
8. Use `StringBuilder` when building a string in a loop or from many pieces. It's unnecessary for a few concatenations: `a + b + c` compiles into a single `string.Concat`, and interpolation is optimised too.
9. `string.Equals(a, b, StringComparison.OrdinalIgnoreCase)`. `ToLower()` allocates two new strings, and it's culture-sensitive (the Turkish "i" problem).
10. No. They're **compile-time warnings only**. Values can still be null at runtime (from JSON, reflection, old code or `!`), so validate inputs at public boundaries.
11. Both pass by reference. `ref` requires the variable to be initialised before the call, and the method can read and write it. `out` doesn't need initialisation, but the method **must** assign it before returning.
12. An async method can pause at `await`, return to the caller, and resume later. By then the caller's stack variable may no longer exist, so holding a reference to it isn't safe.
13. `const` is a compile-time value **copied into every caller's compiled code**. `static readonly` is read at runtime. If a library changes a public `const`, consumers keep the old value until they're recompiled. Use `const` only for values that will never change.
14. No. `readonly` only stops you reassigning the **reference**; you can still `Add`/`Remove`. Expose `IReadOnlyList<T>` or use an immutable collection to prevent changes.
15. A `TypeInitializationException` is thrown, and the type is **unusable for the rest of the process's life**: every later access throws again.
16. A **class** is a reference type with reference equality and inheritance. A **struct** is a value type: copied on assignment, field equality, no inheritance. A **record** (class) is a reference type with compiler-generated **value equality**, `ToString`, `with` and deconstruction; it's ideal for DTOs. (A `record struct` is the value-type version.)
17. No, it's **shallow**. Reference-type properties (lists, nested objects) are shared between the original and the copy.
18. Any integer can be cast to an enum (`(Status)99`), so validate input. Inserting or reordering members changes stored numeric values, so always assign explicit values. Also, enums serialize as numbers in JSON by default.
19. Derived static constructor → derived field initializers → base static constructor → base field initializers → base constructor body → derived constructor body. Static constructors run only the first time.
20. The base constructor runs **before** the derived constructor body, so the overridden method runs on a half-initialised object: fields set in the derived constructor are still null or 0.
21. **Encapsulation:** `BankAccount` with a private balance, exposing `Deposit`/`Withdraw` with rules. **Inheritance:** `ExpressShipping : ShippingMethod`. **Abstraction:** `abstract decimal Cost(weight)`, so checkout doesn't care how each courier prices. **Polymorphism:** checkout loops over `List<ShippingMethod>` calling `Quote()` and each type prices itself (runtime); `Tax(amount)`/`Tax(amount, rate)` overloads (compile time).
22. **Interface:** a capability shared by unrelated types, DI/mocking boundaries, structs, or when a class needs several. **Abstract class:** related types sharing real code or state, constructors, protected members, a template method. A common pattern is to expose an interface and provide an optional abstract base class.
23. **Overriding** (`virtual`/`override`) is runtime polymorphism: the actual object's implementation runs even through a base-type variable. **Hiding** (`new`) is resolved by the variable's compile-time type, so a base-type variable calls the base method.
24. Build behaviour by combining objects (has-a) instead of inheriting (is-a). It's more flexible and testable, and avoids fragile hierarchies. Backend example: `OrderService` receives an `IOrderRepository` and `ILogger` through its constructor instead of inheriting from a `BaseService`.
25. **S:** one class, one reason to change. **O:** extend behaviour by adding code, not editing working code. **L:** subtypes must be usable anywhere their base type is, without surprises. **I:** prefer small, focused interfaces. **D:** depend on abstractions, not concrete implementations.
26. **DIP** is a design principle: high-level code and low-level code both depend on abstractions. **DI** is a technique: dependencies are passed in, usually via the constructor. **IoC** is the broad idea that a framework or container controls object creation and flow. A DI container is one kind of IoC container.
27. If `a.Equals(b)`, then `a.GetHashCode() == b.GetHashCode()`. The reverse doesn't have to hold. Equals must be reflexive, symmetric, transitive, consistent, and false for null. If only `Equals` is overridden, equal objects get different identity-based hash codes, so `HashSet`/`Dictionary` look in the wrong bucket and lookups fail.
28. Without it, collections call `Equals(object)`, which **boxes** the struct, and the default `ValueType.Equals` may use **reflection**. `IEquatable<T>` gives a fast, typed, allocation-free comparison that `EqualityComparer<T>.Default` uses automatically.
29. A negative number if `this` comes first, 0 if they're equal in order, a positive number if `this` comes after. `a - b` can **overflow** for large values and flip the sign. Use `a.CompareTo(b)`.
30. Boxing wraps a value type in a heap-allocated object (a copy, 24 bytes for an `int` on 64-bit). It adds allocations and GC pressure in hot paths. Hidden sources: `params object[]` APIs (logging, old `string.Format`), casting structs to interfaces, `Equals(object)` on structs without `IEquatable<T>`, and non-generic collections (`ArrayList`).

## Part B — What does this code print?

31. **`1`**. Structs are copied, so `warehouseB` is an independent copy.
32. **`7`**. The reference is passed by value. Reassigning `cart` inside `ClearCart` doesn't affect the caller's variable, so the "clear cart" button would silently do nothing. The fix is to set `cart.ItemCount = 0` (or clear its item list) on the same object.
33. **`SMS / No retries`**. `Channel` is overridden, so the real object decides. `RetryPolicy` is hidden, so the variable type (`NotificationSender`) decides. In production, SMS messages would silently get no retries.
34. **`[  hello  ]`**. Strings are immutable. `Trim()` returns a new string, which was thrown away. The fix is `s = s.Trim();`.
35. **`-1`**, **`False`**, **`null`**. `null + 5` is `null` for nullable arithmetic.
36. **`2`** (the list is shared because `with` is shallow), then **`False`** (record equality compares the `List` **reference**, and the new list is a different object, even with the same items).
37. It **throws `InvalidCastException`**. A boxed `int` must be unboxed as `int`. The fix is `long l = (int)o;`.
38. **`7`** (a cast truncates), **`3`** (integer division), **`3.5`**.
39. **`SELECT * FROM NULL`**. The base constructor calls the virtual `TableName()` *before* the `InvoiceRepository` constructor body has set `_table`, so the override sees `null`. In production, this builds broken SQL.
40. **`5`** then **`False`**. Any int can be cast to an enum; `ToString` prints the number when there's no matching name.

## Part C — Spot the bug

41. `Equals` is overridden but `GetHashCode` isn't, so equal users get different hash codes and the `HashSet` searches the wrong bucket. Also, `Id` has a public setter: if it changes after the user is added, the hash breaks anyway. Fix: override `GetHashCode() => Id.GetHashCode()` and make `Id` `init`-only.
42. A `public const` is baked into all 12 consuming services at compile time. If the GST rate changes and only the library is updated, services that weren't recompiled keep using 0.18. Also, a tax rate is business configuration, not a true constant. Fix: `public static readonly`, or better, load it from configuration (the options pattern, Chapter 6).
43. Static mutable state is **shared across all concurrent requests**. `List<T>` isn't thread-safe, so concurrent `Add` calls can corrupt it or throw. It also grows forever (a memory leak), and it isn't shared across multiple server instances. Fix: a proper cache (`IMemoryCache`/distributed cache, Chapter 18) or a thread-safe collection with a size limit, registered as a singleton service.
44. It violates **Dependency Inversion**: `OrderService` creates concrete classes itself. It's tightly coupled to SQL and SMTP, can't be unit-tested without a real database and mail server, and can't be swapped. Fix: depend on `IOrderRepository` and `IEmailSender`, injected through the constructor.
45. Subtraction can **overflow** (e.g. `int.MinValue - 1` wraps to a large positive number), which gives the wrong sign and corrupts sorting. Fix: `=> Value.CompareTo(other.Value);`.
