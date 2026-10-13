namespace Chapter01.Topics;

public static class T15_Equality
{
    // A normal class: no equality code written
    private class UserNoEquals
    {
        public int Id { get; init; }
        public string Email { get; init; } = "";
    }

    // A class with value equality done properly
    private class User
    {
        public int Id { get; init; }
        public string Email { get; init; } = "";

        public override bool Equals(object? obj) =>
            obj is User other && Id == other.Id && Email == other.Email;

        // RULE: if two objects are Equal, they MUST return the same hash code
        public override int GetHashCode() => HashCode.Combine(Id, Email);

        public static bool operator ==(User? left, User? right) => left is null ? right is null : left.Equals(right);
        public static bool operator !=(User? left, User? right) => !(left == right);
    }

#pragma warning disable CS0659 // the compiler warns: Equals overridden without GetHashCode. That is the bug we want to show.
    private class BrokenUser
    {
        public int Id { get; init; }
        public override bool Equals(object? obj) => obj is BrokenUser other && Id == other.Id;
    }
#pragma warning restore CS0659

    public static void Run()
    {
        var a = new UserNoEquals { Id = 1, Email = "a@x.com" };
        var b = new UserNoEquals { Id = 1, Email = "a@x.com" };
        Console.WriteLine($"No override:   a == b: {a == b}, a.Equals(b): {a.Equals(b)}");

        var u1 = new User { Id = 1, Email = "a@x.com" };
        var u2 = new User { Id = 1, Email = "a@x.com" };
        Console.WriteLine($"With override: u1 == u2: {u1 == u2}, u1.Equals(u2): {u1.Equals(u2)}, ReferenceEquals: {ReferenceEquals(u1, u2)}");
        Console.WriteLine($"Same hash code: {u1.GetHashCode() == u2.GetHashCode()}");

        var users = new HashSet<User> { u1 };
        Console.WriteLine($"HashSet<User> contains u2? {users.Contains(u2)}");

        var broken = new HashSet<BrokenUser> { new BrokenUser { Id = 1 } };
        var lookup = new BrokenUser { Id = 1 };
        Console.WriteLine($"BrokenUser Equals: {lookup.Equals(new BrokenUser { Id = 1 })}, but HashSet contains it? {broken.Contains(lookup)}  <- GetHashCode not overridden");

        // Value types: Equals compares contents
        int x = 5, y = 5;
        Console.WriteLine($"int: x == y: {x == y}, x.Equals(y): {x.Equals(y)}");
    }
}
