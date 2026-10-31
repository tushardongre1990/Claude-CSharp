namespace Chapter03.Topics;

public static class T06_FirstSingleAnyAll
{
    private record User(int Id, string Email, string Role, bool IsActive);
    private record Payment(string TransactionId, decimal Amount, string Status);

    private static readonly List<User> Users =
    [
        new(1, "asha@shop.in",  "Admin",    true),
        new(2, "ravi@shop.in",  "Customer", true),
        new(3, "meera@shop.in", "Customer", false),
        new(4, "ravi@shop.in",  "Customer", true),   // duplicate email: bad data
    ];

    private static string Try(Func<object?> action)
    {
        try { return action()?.ToString() ?? "null"; }
        catch (InvalidOperationException ex) { return $"{ex.GetType().Name}: {ex.Message}"; }
    }

    public static void Run()
    {
        // ---- First / FirstOrDefault ----
        Console.WriteLine($"First customer:                {Try(() => Users.First(u => u.Role == "Customer").Id)}");
        Console.WriteLine($"First 'Support' user:          {Try(() => Users.First(u => u.Role == "Support"))}");
        Console.WriteLine($"FirstOrDefault 'Support' user: {Try(() => Users.FirstOrDefault(u => u.Role == "Support"))}");

        // ---- Single / SingleOrDefault: "there must be at most one" ----
        Console.WriteLine($"Single admin:                  {Try(() => Users.Single(u => u.Role == "Admin").Email)}");
        Console.WriteLine($"Single by email (duplicate):   {Try(() => Users.Single(u => u.Email == "ravi@shop.in"))}");
        Console.WriteLine($"SingleOrDefault (duplicate):   {Try(() => Users.SingleOrDefault(u => u.Email == "ravi@shop.in"))}");
        Console.WriteLine($"SingleOrDefault (none):        {Try(() => Users.SingleOrDefault(u => u.Email == "x@shop.in"))}");

        // ---- The default(T) trap with value types ----
        var retryDelaysMs = new List<int> { 200, 400, 800 };
        int firstSlowDelay = retryDelaysMs.FirstOrDefault(ms => ms > 5_000);
        Console.WriteLine($"FirstOrDefault on List<int> with no match: {firstSlowDelay} (0 looks like a real value!)");

        // Fix 1 (.NET 6): pass the default you want
        int withDefault = retryDelaysMs.FirstOrDefault(ms => ms > 5_000, defaultValue: -1);
        Console.WriteLine($"FirstOrDefault(..., defaultValue: -1): {withDefault}");
        // Fix 2: work with a nullable type
        int? asNullable = retryDelaysMs.Select(ms => (int?)ms).FirstOrDefault(ms => ms > 5_000);
        Console.WriteLine($"Nullable version: {(asNullable is null ? "null" : asNullable)}");

        // Last / ElementAt work the same way (and ElementAtOrDefault)
        Console.WriteLine($"Last: {retryDelaysMs.Last()}, ElementAtOrDefault(10): {retryDelaysMs.ElementAtOrDefault(10)}");

        // ---- Any / All ----
        Console.WriteLine($"Any inactive user? {Users.Any(u => !u.IsActive)}");
        Console.WriteLine($"All users have an email? {Users.All(u => u.Email.Contains('@'))}");

        // All() on an EMPTY sequence is TRUE ("no item breaks the rule")
        var payments = new List<Payment>();
        Console.WriteLine($"Empty payments: All succeeded? {payments.All(p => p.Status == "Succeeded")}, Any? {payments.Any()}");
        bool fullyPaid = payments.Any() && payments.All(p => p.Status == "Succeeded");
        Console.WriteLine($"Safer 'fully paid' check on empty list: {fullyPaid}");

        // Any() vs Count() > 0 on a lazy sequence: Any stops at the first match, Count reads everything
        int checkedCount = 0;
        IEnumerable<User> lazyUsers = Users.Where(u => { checkedCount++; return u.IsActive; });
        _ = lazyUsers.Any();
        int afterAny = checkedCount;
        checkedCount = 0;
        _ = lazyUsers.Count() > 0;
        Console.WriteLine($"Items checked: Any() = {afterAny}, Count() > 0 = {checkedCount}");

        // Contains for a simple membership test
        var blockedDomains = new HashSet<string> { "spam.com", "fake.org" };
        Console.WriteLine($"Is 'fake.org' blocked? {blockedDomains.Contains("fake.org")}");
    }
}
