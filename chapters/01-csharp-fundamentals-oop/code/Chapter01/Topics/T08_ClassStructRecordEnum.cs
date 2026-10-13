namespace Chapter01.Topics;

public static class T08_ClassStructRecordEnum
{
    private class PersonClass
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    private struct Coordinate
    {
        public double Lat { get; init; }
        public double Lng { get; init; }
    }

    // Positional record: the compiler generates the constructor, properties,
    // Equals, GetHashCode, ToString, == / != and Deconstruct for you.
    private record PersonRecord(string Name, int Age);

    // A value-type record (C# 10)
    private record struct Temperature(double Celsius);

    private enum OrderStatus { Pending, Shipped, Delivered, Cancelled = 10 }

    [Flags]
    private enum Permissions { None = 0, Read = 1, Write = 2, Delete = 4, All = Read | Write | Delete }

    public static void Run()
    {
        // --- Class: Equals means "same object" by default ---
        var c1 = new PersonClass { Name = "Asha", Age = 30 };
        var c2 = new PersonClass { Name = "Asha", Age = 30 };
        Console.WriteLine($"class equal?  {c1.Equals(c2)}   ToString: {c1}");

        // --- Record: Equals means "same data" ---
        var r1 = new PersonRecord("Asha", 30);
        var r2 = new PersonRecord("Asha", 30);
        Console.WriteLine($"record equal? {r1 == r2}    ToString: {r1}");

        // 'with' creates a modified COPY. Positional records are immutable.
        var older = r1 with { Age = 31 };
        Console.WriteLine($"r1 = {r1}, older = {older}");

        var (name, age) = r1;   // deconstruction
        Console.WriteLine($"Deconstructed: {name}, {age}");

        // --- Struct: value type, Equals compares the fields ---
        var p1 = new Coordinate { Lat = 18.52, Lng = 73.85 };
        var p2 = new Coordinate { Lat = 18.52, Lng = 73.85 };
        Console.WriteLine($"struct equal? {p1.Equals(p2)}");
        var t = new Temperature(25.5);
        Console.WriteLine($"record struct: {t}");

        // --- Enums ---
        var status = OrderStatus.Shipped;
        Console.WriteLine($"status = {status}, as int = {(int)status}, Cancelled as int = {(int)OrderStatus.Cancelled}");
        var parsed = Enum.Parse<OrderStatus>("Delivered");
        Console.WriteLine($"Enum.Parse(\"Delivered\") = {parsed}");
        Console.WriteLine($"Enum.TryParse(\"Lost\") = {Enum.TryParse<OrderStatus>("Lost", out _)}");

        var weird = (OrderStatus)99;   // GOTCHA: any int can be cast to an enum, even undefined ones
        Console.WriteLine($"(OrderStatus)99 = {weird}, Enum.IsDefined? {Enum.IsDefined(weird)}");

        // Flags enum: combine values with |
        var perms = Permissions.Read | Permissions.Write;
        Console.WriteLine($"perms = {perms}, can write? {perms.HasFlag(Permissions.Write)}, can delete? {perms.HasFlag(Permissions.Delete)}");
        Console.WriteLine($"Permissions.All = {Permissions.All}, as int = {(int)Permissions.All}");
    }
}
