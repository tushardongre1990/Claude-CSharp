using System.Collections;

namespace Chapter01.Topics;

public static class T18_BoxingUnboxing
{
    private interface IStockLevel
    {
        int Quantity { get; }
        void Reserve();
    }

    // A small struct tracking how many units of a product are left
    private struct StockLevel : IStockLevel
    {
        public int Quantity { get; private set; }
        public StockLevel(int quantity) => Quantity = quantity;
        public void Reserve() => Quantity--;
    }

    public static void Run()
    {
        // --- Boxing and unboxing ---
        int orderId = 42;
        object boxed = orderId;     // BOXING: the value is copied into a new object on the heap
        int unboxed = (int)boxed;   // UNBOXING: the value is copied back out
        Console.WriteLine($"boxed = {boxed}, unboxed = {unboxed}");

        orderId = 100;
        Console.WriteLine($"After orderId = 100, boxed is still {boxed}   (the box holds its own copy)");

        // --- You must unbox to the EXACT type ---
        try
        {
            long wrong = (long)boxed;
            Console.WriteLine(wrong);
        }
        catch (InvalidCastException)
        {
            Console.WriteLine("(long)boxed threw InvalidCastException: a boxed int must be unboxed as int");
        }
        long right = (int)boxed;    // unbox to int first, then convert int -> long
        Console.WriteLine($"(long)(int)boxed = {right}");

        // --- Casting a struct to an interface boxes it: you get a separate copy ---
        var stock = new StockLevel(10);
        IStockLevel asInterface = stock;   // boxing
        asInterface.Reserve();             // reserves a unit on the BOXED COPY only
        Console.WriteLine($"stock.Quantity = {stock.Quantity}, asInterface.Quantity = {asInterface.Quantity}   <- the reservation was lost on the original!");

        // --- Old non-generic collections box every value; generic ones don't ---
        var arrayList = new ArrayList { 499, 1299, 99 };   // each int is boxed
        var list = new List<int> { 499, 1299, 99 };        // no boxing
        int sum = 0;
        foreach (object item in arrayList) sum += (int)item;   // unboxing on every read
        Console.WriteLine($"ArrayList sum = {sum}, List<int> sum = {list.Sum()}");

        // --- Boxing costs memory: each box is a small heap object ---
        object holder = 0;
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) holder = i;   // 1000 boxes
        long after = GC.GetAllocatedBytesForCurrentThread();
        GC.KeepAlive(holder);                        // keeps the last box referenced (reachable) up to this point
        Console.WriteLine($"Boxing 1000 ints allocated {after - before} bytes (observed on 64-bit .NET; varies by runtime)");
    }
}
