using System.Diagnostics;
using System.Text;

namespace Chapter01.Topics;

public static class T04_Strings
{
    public static void Run()
    {
        // --- Strings are immutable: methods return a NEW string, the original never changes ---
        string original = "hello";
        string upper = original.ToUpper();
        Console.WriteLine($"original = {original}, upper = {upper}");

        string a = "hi";
        string b = a;       // b points to the same string as a
        a += " there";      // creates a NEW string; a now points to it, b still points to the old one
        Console.WriteLine($"a = \"{a}\", b = \"{b}\"");

        // --- == compares the TEXT for strings (not the address) ---
        string x = "dotnet";
        string y = new string("dotnet".ToCharArray());   // a different object with the same text
        Console.WriteLine($"x == y: {x == y}, same object: {ReferenceEquals(x, y)}");

        // --- String interning: identical literals in code share one object ---
        string lit1 = "interned";
        string lit2 = "interned";
        Console.WriteLine($"Two identical literals are the same object: {ReferenceEquals(lit1, lit2)}");

        // --- Comparing safely ---
        Console.WriteLine($"\"ABC\" == \"abc\": {"ABC" == "abc"}");
        Console.WriteLine($"string.Equals(\"ABC\", \"abc\", OrdinalIgnoreCase): {string.Equals("ABC", "abc", StringComparison.OrdinalIgnoreCase)}");

        // --- Null / empty / whitespace checks ---
        string spaces = "   ";
        Console.WriteLine($"IsNullOrEmpty(\"   \") = {string.IsNullOrEmpty(spaces)}, IsNullOrWhiteSpace(\"   \") = {string.IsNullOrWhiteSpace(spaces)}");

        // --- Concatenation in a loop vs StringBuilder ---
        const int n = 20_000;
        var sw = Stopwatch.StartNew();
        string slow = "";
        for (int i = 0; i < n; i++) slow += "x";     // creates 20,000 temporary strings
        sw.Stop();
        long concatMs = sw.ElapsedMilliseconds;

        sw.Restart();
        var sb = new StringBuilder();
        for (int i = 0; i < n; i++) sb.Append('x');   // edits one growing buffer
        string fast = sb.ToString();
        sw.Stop();
        Console.WriteLine($"+= in loop: {concatMs} ms, StringBuilder: {sw.ElapsedMilliseconds} ms (lengths {slow.Length}, {fast.Length})");

        // --- Literal styles ---
        string path = @"C:\temp\new";                 // verbatim string: backslashes are not escape characters
        string json = """
            { "name": "Asha", "age": 30 }
            """;                                       // raw string literal (C# 11): quotes need no escaping
        Console.WriteLine($"verbatim: {path}");
        Console.WriteLine($"raw: {json}");
    }
}
