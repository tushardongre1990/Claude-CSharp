namespace Chapter01.Topics;

public static class T16_EquatableComparable
{
    private readonly struct Money : IEquatable<Money>, IComparable<Money>
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        // IEquatable<Money>: strongly-typed Equals, no boxing
        public bool Equals(Money other) => Amount == other.Amount && Currency == other.Currency;
        public override bool Equals(object? obj) => obj is Money other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Amount, Currency);

        // IComparable<Money>: negative = this comes first, 0 = same, positive = this comes after
        public int CompareTo(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("Cannot compare different currencies");
            return Amount.CompareTo(other.Amount);
        }

        public static bool operator ==(Money left, Money right) => left.Equals(right);
        public static bool operator !=(Money left, Money right) => !left.Equals(right);
        public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;
        public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;

        public override string ToString() => $"{Amount} {Currency}";
    }

    private record Employee(string Name, int Age, decimal Salary);

    // IComparer<T>: a SEPARATE class that decides the order (use it when you need several sort orders)
    private class BySalaryDescending : IComparer<Employee>
    {
        public int Compare(Employee? x, Employee? y) => (y?.Salary ?? 0).CompareTo(x?.Salary ?? 0);
    }

    public static void Run()
    {
        var a = new Money(100, "INR");
        var b = new Money(100, "INR");
        var c = new Money(250, "INR");
        Console.WriteLine($"a == b: {a == b}, a < c: {a < c}, a.CompareTo(c): {a.CompareTo(c)}");

        var wallet = new List<Money> { c, a, new Money(50, "INR") };
        wallet.Sort();   // uses IComparable<Money>.CompareTo
        Console.WriteLine($"Sorted wallet: {string.Join(", ", wallet)}");

        var team = new List<Employee>
        {
            new("Asha", 30, 90000),
            new("Ravi", 25, 120000),
            new("Meera", 35, 75000),
        };

        team.Sort(new BySalaryDescending());
        Console.WriteLine($"By salary (desc): {string.Join(", ", team.Select(e => $"{e.Name}({e.Salary})"))}");

        team.Sort((x, y) => x.Age.CompareTo(y.Age));   // a quick one-off comparison
        Console.WriteLine($"By age:           {string.Join(", ", team.Select(e => $"{e.Name}({e.Age})"))}");

        try
        {
            _ = a.CompareTo(new Money(1, "USD"));
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Compare INR with USD: {ex.Message}");
        }
    }
}
