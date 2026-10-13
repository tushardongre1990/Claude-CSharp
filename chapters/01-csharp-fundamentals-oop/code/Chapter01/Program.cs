// Run one topic:   dotnet run -- 3
// Run all topics:  dotnet run
using Chapter01.Topics;

var demos = new Dictionary<int, (string Title, Action Run)>
{
    [0] = ("C# syntax you need: if, switch, loops, is/as", T00_SyntaxBasics.Run),
    [1] = ("Types, variables, casting and type conversions", T01_TypesAndConversions.Run),
    [2] = ("var vs dynamic vs object", T02_VarDynamicObject.Run),
    [3] = ("Value types vs reference types", T03_ValueVsReference.Run),
    [4] = ("Strings, immutability and StringBuilder", T04_Strings.Run),
    [5] = ("Nullable value types and nullable reference types", T05_Nullable.Run),
    [6] = ("Method parameters: optional, named, ref, out, in, params", T06_MethodParameters.Run),
    [7] = ("static, const and readonly", T07_StaticConstReadonly.Run),
    [8] = ("Classes, structs, records and enums", T08_ClassStructRecordEnum.Run),
    [9] = ("Constructors, properties and access modifiers", T09_ConstructorsPropertiesAccess.Run),
    [10] = ("Encapsulation, inheritance, abstraction and polymorphism", T10_OopPillars.Run),
    [11] = ("Interfaces vs abstract classes", T11_InterfacesVsAbstract.Run),
    [12] = ("Overloading vs overriding vs method hiding", T12_OverrideVsHiding.Run),
    [13] = ("Composition vs inheritance", T13_CompositionVsInheritance.Run),
    [14] = ("SOLID principles", T14_Solid.Run),
    [15] = ("Equality vs reference identity", T15_Equality.Run),
    [16] = ("IEquatable<T> and IComparable<T>", T16_EquatableComparable.Run),
    [17] = ("Shallow copy vs deep copy", T17_ShallowVsDeepCopy.Run),
    [18] = ("Boxing and unboxing", T18_BoxingUnboxing.Run),
};

if (args.Length > 0)
{
    if (int.TryParse(args[0], out var topic) && demos.TryGetValue(topic, out var demo))
        Print(topic, demo);
    else
        Console.WriteLine($"Unknown topic '{args[0]}'. Valid topics: {string.Join(", ", demos.Keys)}");
    return;
}

foreach (var (number, d) in demos)
    Print(number, d);

static void Print(int number, (string Title, Action Run) demo)
{
    Console.WriteLine($"===== Topic {number}: {demo.Title} =====");
    demo.Run();
    Console.WriteLine();
}
