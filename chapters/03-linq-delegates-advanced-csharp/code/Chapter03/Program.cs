// Run one topic:   dotnet run -- 3
// Run all topics:  dotnet run
using Chapter03.Topics;

var demos = new Dictionary<int, (string Title, Action Run)>
{
    [1] = ("Delegates, Action, Func and Predicate", T01_Delegates.Run),
    [2] = ("Lambdas and closures", T02_LambdasAndClosures.Run),
    [3] = ("Events and publisher/subscriber", T03_Events.Run),
    [4] = ("LINQ basics: Where, Select, SelectMany, OrderBy", T04_LinqBasics.Run),
    [5] = ("LINQ grouping, joins and aggregation", T05_GroupingJoinsAggregation.Run),
    [6] = ("First / Single / OrDefault, Any / All", T06_FirstSingleAnyAll.Run),
    [7] = ("Deferred vs immediate execution", T07_DeferredExecution.Run),
    [8] = ("IEnumerable<T> vs IQueryable<T>", T08_EnumerableVsQueryable.Run),
    [9] = ("Expression trees", T09_ExpressionTrees.Run),
    [10] = ("Extension methods", T10_ExtensionMethods.Run),
    [11] = ("Tuples and deconstruction", T11_TuplesAndDeconstruction.Run),
    [12] = ("Pattern matching", T12_PatternMatching.Run),
    [13] = ("Iterators and yield return", T13_Iterators.Run),
    [14] = ("Attributes and basic reflection", T14_AttributesAndReflection.Run),
    [15] = ("Modern C# (10-12)", T15_ModernCSharp.Run),
    [16] = ("Newest C# (13-14)", T16_NewestCSharp.Run),
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
