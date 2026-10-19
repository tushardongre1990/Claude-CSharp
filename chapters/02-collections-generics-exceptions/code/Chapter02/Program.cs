// Run one topic:   dotnet run -- 3
// Run all topics:  dotnet run
using Chapter02.Topics;

var demos = new Dictionary<int, (string Title, Action Run)>
{
    [1] = ("Arrays and List<T>", T01_ArraysAndLists.Run),
    [2] = ("Dictionary and HashSet: hashing and collisions", T02_DictionaryAndHashSet.Run),
    [3] = ("Queue, Stack, LinkedList, SortedDictionary (+ PriorityQueue)", T03_QueueStackLinkedListSorted.Run),
    [4] = ("Collection interfaces", T04_CollectionInterfaces.Run),
    [5] = ("Choosing a collection: Big-O and memory", T05_ChoosingACollection.Run),
    [6] = ("Generics: classes, methods and constraints", T06_Generics.Run),
    [7] = ("Covariance and contravariance (in / out)", T07_Variance.Run),
    [8] = ("Concurrent collections", T08_ConcurrentCollections.Run),
    [9] = ("Exception hierarchy and try / catch / finally", T09_ExceptionBasics.Run),
    [10] = ("Custom exceptions and exception filters", T10_CustomExceptionsAndFilters.Run),
    [11] = ("throw vs throw ex, and stack traces", T11_ThrowVsThrowEx.Run),
    [12] = ("Expected errors vs exceptional failures (Result pattern)", T12_ResultPattern.Run),
    [13] = ("IDisposable, using and unmanaged resources", T13_Disposable.Run),
    [14] = ("Logging exceptions without swallowing or duplicating", T14_LoggingExceptions.Run),
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
