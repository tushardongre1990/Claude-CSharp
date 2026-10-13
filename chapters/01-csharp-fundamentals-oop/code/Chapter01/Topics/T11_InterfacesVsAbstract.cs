namespace Chapter01.Topics;

public static class T11_InterfacesVsAbstract
{
    // ===== Interface: a CONTRACT ("can do"), no state =====
    private interface IPaymentMethod
    {
        string Name { get; }
        bool Pay(decimal amount);

        // Default interface method (C# 8): a body inside the interface
        string Receipt(decimal amount) => $"Paid {amount} using {Name}";
    }

    private interface IRefundable
    {
        void Refund(decimal amount);
    }

    // A class can implement MANY interfaces
    private class CardPayment : IPaymentMethod, IRefundable
    {
        public string Name => "Card";
        public bool Pay(decimal amount) => amount <= 10_000;
        public void Refund(decimal amount) => Console.WriteLine($"  Refunded {amount} to card");
    }

    private class UpiPayment : IPaymentMethod
    {
        public string Name => "UPI";
        public bool Pay(decimal amount) => amount <= 100_000;
        public string Receipt(decimal amount) => $"UPI receipt for {amount}";   // replaces the default
    }

    // ===== Abstract class: shared STATE and CODE + methods children must fill in. Only ONE base class allowed =====
    private abstract class Notification
    {
        private static int _sent;

        protected string Recipient { get; }

        protected Notification(string recipient) => Recipient = recipient;

        public void Send()                        // same steps for every notification
        {
            Console.WriteLine($"  [{GetType().Name}] {Format()}");
            _sent++;
        }

        protected abstract string Format();       // each child decides this part

        public static int TotalSent => _sent;
    }

    private class EmailNotification : Notification
    {
        public EmailNotification(string email) : base(email) { }
        protected override string Format() => $"To: {Recipient} | Subject: Your order shipped";
    }

    private class SmsNotification : Notification
    {
        public SmsNotification(string phone) : base(phone) { }
        protected override string Format() => $"SMS to {Recipient}: Order shipped";
    }

    // ===== Explicit interface implementation: two interfaces with the same method name =====
    private interface IPrinter { string Output(); }
    private interface IScanner { string Output(); }

    private class MultiFunctionDevice : IPrinter, IScanner
    {
        string IPrinter.Output() => "Printed a document";
        string IScanner.Output() => "Scanned an image";
    }

    public static void Run()
    {
        var methods = new List<IPaymentMethod> { new CardPayment(), new UpiPayment() };
        foreach (var method in methods)
            Console.WriteLine($"{method.Name}: Pay(20000) = {method.Pay(20000)}; {method.Receipt(20000)}");

        var card = new CardPayment();
        // card.Receipt(10);   // COMPILE ERROR: default interface methods are only visible through the interface
        Console.WriteLine(((IPaymentMethod)card).Receipt(10));
        if (card is IRefundable refundable)
            refundable.Refund(500);

        new EmailNotification("asha@example.com").Send();
        new SmsNotification("+91-9000000000").Send();
        Console.WriteLine($"Total notifications sent: {Notification.TotalSent}");

        var device = new MultiFunctionDevice();
        Console.WriteLine($"As IPrinter: {((IPrinter)device).Output()}");
        Console.WriteLine($"As IScanner: {((IScanner)device).Output()}");
        // device.Output();   // COMPILE ERROR: explicit implementations are only reachable through the interface
    }
}
