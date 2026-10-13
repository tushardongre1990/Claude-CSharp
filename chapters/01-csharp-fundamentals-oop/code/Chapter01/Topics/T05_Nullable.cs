namespace Chapter01.Topics;

public static class T05_Nullable
{
    public static void Run()
    {
        // --- Nullable VALUE types: int? can hold a number OR null ---
        int? discount = null;
        Console.WriteLine($"discount.HasValue = {discount.HasValue}");
        Console.WriteLine($"discount.GetValueOrDefault() = {discount.GetValueOrDefault()}");
        Console.WriteLine($"discount ?? 5 = {discount ?? 5}");

        try
        {
            int value = discount.Value;   // throws, because there is no value (the compiler even warns: CS8629)
            Console.WriteLine(value);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"discount.Value threw: {ex.Message}");
        }

        discount ??= 10;   // assign only if it is currently null
        Console.WriteLine($"after discount ??= 10: {discount}");

        // Arithmetic with null gives null
        int? nothing = null;
        int? total = discount + nothing;
        Console.WriteLine($"10 + null = {total?.ToString() ?? "null"}");

        // --- Nullable REFERENCE types: the compiler warns you, nothing changes at runtime ---
        string? maybeName = GetNameOrNull(useNull: true);
        // Console.WriteLine(maybeName.Length);   // WARNING CS8602: dereference of a possibly null reference
        Console.WriteLine($"maybeName?.Length = {maybeName?.Length.ToString() ?? "null"}");
        Console.WriteLine($"maybeName?.Length ?? 0 = {maybeName?.Length ?? 0}");

        string definitelyName = GetNameOrNull(useNull: false)!;   // ! = "trust me, it's not null" (no runtime check)
        Console.WriteLine($"definitelyName = {definitelyName}");

        // Pattern-matching null checks
        if (maybeName is not null)
            Console.WriteLine("maybeName has a value");
        else
            Console.WriteLine("maybeName is null");
    }

    private static string? GetNameOrNull(bool useNull) => useNull ? null : "Ravi";
}
