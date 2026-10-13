namespace Chapter01.Topics;

public static class T07_StaticConstReadonly
{
    private class AppSettings
    {
        public const double TaxRate = 0.18;                  // compile-time constant: baked into the code that uses it
        public static readonly DateTime StartedAt;           // set once at runtime, shared by all objects
        public readonly string InstanceId;                   // set once per object (in the constructor)
        public readonly List<string> Tags = new();           // the REFERENCE is read-only; the list itself is not

        public static int InstancesCreated;                  // one copy shared by all objects

        static AppSettings()                                 // static constructor: runs ONCE, just before first use
        {
            StartedAt = DateTime.UtcNow;
            Console.WriteLine("  [static constructor ran]");
        }

        public AppSettings(string id)
        {
            InstanceId = id;      // readonly fields can be set in the constructor
            InstancesCreated++;
        }

        public void TryChange()
        {
            // InstanceId = "new";    // COMPILE ERROR: readonly field can only be set in a constructor
            Tags.Add("allowed");      // fine: we change the list's contents, not the reference
            // Tags = new();          // COMPILE ERROR: cannot replace a readonly reference
        }
    }

    private static class MathHelper   // static class: no instances, only static members
    {
        public static int Square(int x) => x * x;
    }

    public static void Run()
    {
        Console.WriteLine($"TaxRate (const) = {AppSettings.TaxRate}   <- reading a const does not touch the class");
        Console.WriteLine("Creating first object...");
        var s1 = new AppSettings("A");
        var s2 = new AppSettings("B");
        s1.TryChange();

        Console.WriteLine($"InstancesCreated (static, shared) = {AppSettings.InstancesCreated}");
        Console.WriteLine($"s1.InstanceId = {s1.InstanceId}, s2.InstanceId = {s2.InstanceId}");
        Console.WriteLine($"s1.Tags = [{string.Join(", ", s1.Tags)}], s2.Tags = [{string.Join(", ", s2.Tags)}]");
        Console.WriteLine($"MathHelper.Square(7) = {MathHelper.Square(7)}");
        // var helper = new MathHelper();   // COMPILE ERROR: cannot create an instance of a static class
    }
}
