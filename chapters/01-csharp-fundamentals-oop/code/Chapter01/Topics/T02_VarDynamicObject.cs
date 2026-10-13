using Microsoft.CSharp.RuntimeBinder;

namespace Chapter01.Topics;

public static class T02_VarDynamicObject
{
    public static void Run()
    {
        // --- var: the COMPILER works out the type from the right-hand side. Still strongly typed. ---
        var count = 10;          // count is an int, forever
        var message = "hello";   // message is a string, forever
        // count = "ten";        // COMPILE ERROR: cannot convert string to int
        Console.WriteLine($"var count -> {count.GetType().Name}, var message -> {message.GetType().Name}");

        // --- object: the base type of everything. Can hold anything, but you must cast to use it. ---
        object box = 10;
        // int sum = box + 5;    // COMPILE ERROR: operator '+' cannot be applied to 'object' and 'int'
        int sum = (int)box + 5;  // cast back to int first
        Console.WriteLine($"object box holds {box.GetType().Name}; (int)box + 5 = {sum}");
        box = "now I am a string";   // allowed: object can hold anything
        Console.WriteLine($"object box now holds {box.GetType().Name}");

        // --- dynamic: type checks are skipped at compile time and done at RUNTIME ---
        dynamic anything = 10;
        anything = anything + 5;     // works: at runtime it sees int + int
        Console.WriteLine($"dynamic: {anything} ({anything.GetType().Name})");
        anything = "text";
        Console.WriteLine($"dynamic now: {anything} (Length = {anything.Length})");

        try
        {
            anything.ThisMethodDoesNotExist();   // compiles fine! It only fails when it runs
        }
        catch (RuntimeBinderException ex)
        {
            Console.WriteLine($"Runtime error: {ex.Message}");
        }
    }
}
