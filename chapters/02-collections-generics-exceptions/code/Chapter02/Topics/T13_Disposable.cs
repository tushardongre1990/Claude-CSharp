using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace Chapter02.Topics;

// ---- The common case: a class that OWNS another IDisposable (here a FileStream) ----
// No finalizer needed: FileStream already cleans up its own OS handle.
public sealed class CsvReportWriter : IDisposable
{
    private readonly StreamWriter _writer;
    private readonly string _name;
    private bool _disposed;

    public CsvReportWriter(string path, string name)
    {
        _name = name;
        _writer = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None));
        _writer.WriteLine("OrderId,Total");
    }

    public void WriteRow(int orderId, decimal total)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);   // .NET 7+
        if (total < 0) throw new ArgumentOutOfRangeException(nameof(total), "Total cannot be negative.");
        _writer.WriteLine($"{orderId},{total}");
    }

    public void Dispose()
    {
        if (_disposed) return;          // Dispose must be safe to call twice
        _writer.Dispose();              // flushes buffered text and closes the file handle
        _disposed = true;
        Console.WriteLine($"  [{_name}] disposed");
    }
}

// ---- The rare case: a class that DIRECTLY owns an unmanaged handle ----
// Best practice: wrap the raw handle in a SafeHandle. SafeHandle has the finalizer, so this class does not need one.
public sealed class NativeLabelPrinter : IDisposable
{
    private readonly SafeHandle _handle;

    public NativeLabelPrinter()
    {
        // In real code this would come from a native "open printer" call via P/Invoke.
        // Here we allocate unmanaged memory as a stand-in for a native resource.
        IntPtr raw = Marshal.AllocHGlobal(64);
        _handle = new UnmanagedMemoryHandle(raw);
    }

    public bool IsOpen => !_handle.IsClosed;
    public void Dispose() => _handle.Dispose();

    private sealed class UnmanagedMemoryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public UnmanagedMemoryHandle(IntPtr existing) : base(ownsHandle: true) => SetHandle(existing);
        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(handle);   // runs from Dispose OR, if someone forgot, from the finalizer
            return true;
        }
    }
}

public static class T13_Disposable
{
    public static void Run()
    {
        string folder = Path.Combine(Path.GetTempPath(), "chapter02-reports");
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, "daily-sales.csv");

        // ---- Bug: forgetting to dispose keeps the file locked ----
        var leaked = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);   // never disposed (yet)
        try
        {
            using var uploader = new FileStream(path, FileMode.Open, FileAccess.Read);           // e.g. the upload job
        }
        catch (IOException)
        {
            Console.WriteLine("Upload job: IOException, the file is still locked by an undisposed FileStream");
        }
        leaked.Dispose();

        // ---- using declaration: disposed at the end of the enclosing scope ----
        {
            using var report = new CsvReportWriter(path, "report");
            report.WriteRow(1001, 2_500m);
            report.WriteRow(1002, 799m);
            Console.WriteLine("  rows written, leaving scope...");
        }
        Console.WriteLine($"File content after dispose: {File.ReadAllText(path).Replace(Environment.NewLine, " | ")}");

        // ---- using also disposes when an exception is thrown ----
        try
        {
            using var report = new CsvReportWriter(path, "report-with-error");
            report.WriteRow(1003, 100m);
            report.WriteRow(1004, -1m);   // throws
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine("  caught the exception AFTER dispose ran");
        }

        // ---- Several usings: disposed in reverse order ----
        {
            using var first = new CsvReportWriter(Path.Combine(folder, "a.csv"), "first");
            using var second = new CsvReportWriter(Path.Combine(folder, "b.csv"), "second");
            Console.WriteLine("  leaving scope with two writers...");
        }

        // ---- Using an object after Dispose ----
        var closed = new CsvReportWriter(path, "closed");
        closed.Dispose();
        closed.Dispose();                 // second call is harmless
        try { closed.WriteRow(1, 1m); }
        catch (ObjectDisposedException) { Console.WriteLine("WriteRow after Dispose: ObjectDisposedException"); }

        // ---- SafeHandle-based wrapper ----
        var printer = new NativeLabelPrinter();
        Console.WriteLine($"Native printer open: {printer.IsOpen}");
        printer.Dispose();
        Console.WriteLine($"Native printer open after Dispose: {printer.IsOpen}");

        Directory.Delete(folder, recursive: true);
    }
}
