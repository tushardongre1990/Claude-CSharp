using System.Text.Json;

namespace Chapter01.Topics;

public static class T17_ShallowVsDeepCopy
{
    private class Address
    {
        public string City { get; set; } = "";
    }

    private class Person
    {
        public string Name { get; set; } = "";
        public Address Address { get; set; } = new();
        public List<string> Skills { get; set; } = new();

        // Shallow: copies each field as-is. Reference fields still point to the SAME objects.
        public Person ShallowCopy() => (Person)MemberwiseClone();

        // Deep: creates NEW copies of everything inside
        public Person DeepCopy() => new()
        {
            Name = Name,
            Address = new Address { City = Address.City },
            Skills = new List<string>(Skills),
        };

        public override string ToString() => $"{Name}, {Address.City}, [{string.Join(", ", Skills)}]";
    }

    private record Order(string Id, List<string> Items);

    private static Person CreateAsha() => new()
    {
        Name = "Asha",
        Address = new Address { City = "Pune" },
        Skills = { "C#" },
    };

    public static void Run()
    {
        // --- Shallow copy ---
        var original = CreateAsha();
        var shallow = original.ShallowCopy();
        shallow.Name = "Ravi";              // string variable reassigned -> independent
        shallow.Address.City = "Mumbai";    // SAME Address object -> the original changes too!
        shallow.Skills.Add("SQL");          // SAME list -> the original changes too!
        Console.WriteLine($"Shallow: original = {original}");
        Console.WriteLine($"         copy     = {shallow}");

        // --- Deep copy ---
        original = CreateAsha();
        var deep = original.DeepCopy();
        deep.Address.City = "Delhi";
        deep.Skills.Add("Azure");
        Console.WriteLine($"Deep:    original = {original}");
        Console.WriteLine($"         copy     = {deep}");

        // --- Deep copy using JSON (simple but slow) ---
        var jsonCopy = JsonSerializer.Deserialize<Person>(JsonSerializer.Serialize(original))!;
        jsonCopy.Address.City = "Chennai";
        Console.WriteLine($"JSON:    original city = {original.Address.City}, copy city = {jsonCopy.Address.City}");

        // --- Records: 'with' is a SHALLOW copy ---
        var order = new Order("O1", new List<string> { "Pen" });
        var copy = order with { Id = "O2" };
        copy.Items.Add("Book");
        Console.WriteLine($"Record 'with': order.Items = [{string.Join(", ", order.Items)}]  <- the list is shared!");
    }
}
