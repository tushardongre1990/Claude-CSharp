namespace Chapter01.Topics;

public static class T14_Solid
{
    // ===== S: Single Responsibility — one class, one reason to change =====
    private record Invoice(int Id, decimal Amount);

    private class InvoiceCalculator
    {
        public decimal Tax(Invoice invoice) => invoice.Amount * 0.18m;
    }

    private class InvoicePrinter
    {
        public string Print(Invoice invoice, decimal tax) => $"Invoice {invoice.Id}: amount {invoice.Amount}, tax {tax}";
    }

    // ===== O: Open/Closed — add new behaviour by ADDING classes, not EDITING old ones =====
    private interface IDiscount { decimal Apply(decimal amount); }
    private class NoDiscount : IDiscount { public decimal Apply(decimal amount) => amount; }
    private class FestivalDiscount : IDiscount { public decimal Apply(decimal amount) => amount * 0.9m; }
    private class LoyaltyDiscount : IDiscount { public decimal Apply(decimal amount) => amount - 100; }   // added later; nothing else changed

    // ===== L: Liskov Substitution — any implementation must work anywhere the abstraction is expected =====
    private interface IFileStorage { string Save(string fileName); }
    private class LocalDiskStorage : IFileStorage { public string Save(string fileName) => $"Saved {fileName} to D:\\uploads"; }
    private class BlobStorage : IFileStorage { public string Save(string fileName) => $"Saved {fileName} to blob container 'uploads'"; }

    // This code works with ANY storage and never needs to check which one it got
    private static string UploadInvoice(IFileStorage storage) => storage.Save("invoice-1001.pdf");

    // ===== I: Interface Segregation — small, focused interfaces =====
    private interface IReadRepository<T> { T? Get(int index); }
    private interface IWriteRepository<T> { void Add(T item); }

    private class ProductCatalog : IReadRepository<string>, IWriteRepository<string>
    {
        private readonly List<string> _items = new() { "Pen", "Book" };
        public string? Get(int index) => index >= 0 && index < _items.Count ? _items[index] : null;   // check BOTH bounds
        public void Add(string item) => _items.Add(item);
    }

    // ===== D: Dependency Inversion — depend on abstractions, not concrete classes =====
    private interface IMessageSender { string Send(string to, string message); }
    private class EmailSender : IMessageSender { public string Send(string to, string message) => $"Email to {to}: {message}"; }
    private class SmsSender : IMessageSender { public string Send(string to, string message) => $"SMS to {to}: {message}"; }

    private class SignupService
    {
        private readonly IMessageSender _sender;
        public SignupService(IMessageSender sender) => _sender = sender;   // given from outside, not created inside
        public string Register(string user) => _sender.Send(user, "Welcome!");
    }

    public static void Run()
    {
        var invoice = new Invoice(1, 1000m);
        var tax = new InvoiceCalculator().Tax(invoice);
        Console.WriteLine($"S: {new InvoicePrinter().Print(invoice, tax)}");

        IDiscount[] discounts = { new NoDiscount(), new FestivalDiscount(), new LoyaltyDiscount() };
        foreach (var discount in discounts)
            Console.WriteLine($"O: {discount.GetType().Name} on 1000 -> {discount.Apply(1000m)}");

        IFileStorage[] storages = { new LocalDiskStorage(), new BlobStorage() };
        foreach (var storage in storages)
            Console.WriteLine($"L: {UploadInvoice(storage)}");

        IReadRepository<string> reader = new ProductCatalog();   // this code only needs to READ
        Console.WriteLine($"I: reader.Get(1) = {reader.Get(1)}");

        Console.WriteLine($"D: {new SignupService(new EmailSender()).Register("asha@example.com")}");
        Console.WriteLine($"D: {new SignupService(new SmsSender()).Register("+91-9000000000")}");
    }
}
