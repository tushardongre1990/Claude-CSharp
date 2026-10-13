namespace Chapter01.Topics;

public static class T01_TypesAndConversions
{
    public static void Run()
    {
        // --- 1. Common built-in types ---
        int age = 30;                       // whole number, 4 bytes
        long population = 8_000_000_000L;   // big whole number, 8 bytes (underscores are only for readability)
        double temperature = 36.6;          // decimal-point number: fast but approximate
        decimal salary = 50000.75m;         // exact decimal number: use for money (note the 'm')
        bool isActive = true;               // true or false
        char grade = 'A';                   // a single character, in single quotes
        string city = "Pune";               // text, in double quotes

        Console.WriteLine($"int={age}, long={population}, double={temperature}, decimal={salary}");
        Console.WriteLine($"bool={isActive}, char={grade}, string={city}");
        Console.WriteLine($"Sizes in bytes: int={sizeof(int)}, long={sizeof(long)}, double={sizeof(double)}, decimal={sizeof(decimal)}");

        // --- 2. Implicit conversion: small type -> bigger type. No data can be lost, so no cast needed ---
        int small = 100;
        long bigger = small;
        double asDouble = small;
        Console.WriteLine($"Implicit: int {small} -> long {bigger} -> double {asDouble}");

        // --- 3. Explicit conversion (cast): you force it, and data CAN be lost ---
        double price = 9.99;
        int whole = (int)price;             // the decimal part is CUT OFF, not rounded
        Console.WriteLine($"(int)9.99 = {whole}");

        long tooBig = 3_000_000_000L;
        int squeezed = (int)tooBig;         // does not fit in an int -> wraps around silently
        Console.WriteLine($"(int)3,000,000,000 = {squeezed}");

        // --- 4. checked: throw an error instead of silently wrapping ---
        try
        {
            int safe = checked((int)tooBig);
            Console.WriteLine(safe);
        }
        catch (OverflowException)
        {
            Console.WriteLine("checked((int)3,000,000,000) threw OverflowException");
        }

        // --- 5. Converting text to numbers ---
        int parsed = int.Parse("42");                              // throws if the text is not a number
        bool success = int.TryParse("abc", out int tryResult);      // never throws, returns true/false
        Console.WriteLine($"int.Parse(\"42\") = {parsed}");
        Console.WriteLine($"int.TryParse(\"abc\") -> success={success}, value={tryResult}");

        string? missing = null;
        Console.WriteLine($"Convert.ToInt32(null) = {Convert.ToInt32(missing)}");   // null becomes 0
        try
        {
            int.Parse(missing!);
        }
        catch (ArgumentNullException)
        {
            Console.WriteLine("int.Parse(null) threw ArgumentNullException");
        }

        // --- 6. Rounding surprises ---
        Console.WriteLine($"Convert.ToInt32(2.5) = {Convert.ToInt32(2.5)}, Convert.ToInt32(3.5) = {Convert.ToInt32(3.5)}");
        Console.WriteLine($"Math.Round(2.5) = {Math.Round(2.5)}, Math.Round(2.5, AwayFromZero) = {Math.Round(2.5, MidpointRounding.AwayFromZero)}");

        // --- 7. double is approximate, decimal is exact ---
        Console.WriteLine($"double:  0.1 + 0.2 = {0.1 + 0.2}");
        Console.WriteLine($"decimal: 0.1 + 0.2 = {0.1m + 0.2m}");
        Console.WriteLine($"0.1 + 0.2 == 0.3 with double? {0.1 + 0.2 == 0.3}");
    }
}
