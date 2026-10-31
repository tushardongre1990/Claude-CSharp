using System.Runtime.CompilerServices;

namespace Chapter03.Topics;

public static class T03_Events
{
    // The data sent with the event. Deriving from EventArgs is the .NET convention (optional since .NET 4.5).
    public sealed class OrderPlacedEventArgs(int orderId, decimal total) : EventArgs
    {
        public int OrderId { get; } = orderId;
        public decimal Total { get; } = total;
    }

    // PUBLISHER: raises the event. It doesn't know who is listening.
    public sealed class OrderService
    {
        public event EventHandler<OrderPlacedEventArgs>? OrderPlaced;

        public void PlaceOrder(int orderId, decimal total)
        {
            Console.WriteLine($"  OrderService: order {orderId} saved");
            OnOrderPlaced(new OrderPlacedEventArgs(orderId, total));
        }

        // Convention: an On<EventName> method raises the event. In a non-sealed class it is
        // 'protected virtual' so derived classes can hook in; this class is sealed, so it is private.
        private void OnOrderPlaced(OrderPlacedEventArgs e) => OrderPlaced?.Invoke(this, e);

        public int SubscriberCount => OrderPlaced?.GetInvocationList().Length ?? 0;
    }

    // SUBSCRIBERS: react to the event
    public sealed class EmailNotifier
    {
        public void Handle(object? sender, OrderPlacedEventArgs e) =>
            Console.WriteLine($"  EmailNotifier: confirmation for order {e.OrderId}");
    }

    public sealed class InventoryReserver
    {
        public void Handle(object? sender, OrderPlacedEventArgs e) =>
            Console.WriteLine($"  InventoryReserver: stock reserved for order {e.OrderId}");
    }

    // A short-lived object (e.g. a per-request dashboard widget) that subscribes to a long-lived publisher
    public sealed class LiveSalesWidget
    {
        public decimal RunningTotal { get; private set; }
        public void Handle(object? sender, OrderPlacedEventArgs e) => RunningTotal += e.Total;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateWidget(OrderService service, bool unsubscribe)
    {
        var widget = new LiveSalesWidget();
        service.OrderPlaced += widget.Handle;
        if (unsubscribe) service.OrderPlaced -= widget.Handle;
        return new WeakReference(widget);   // lets us ask "is it still alive?" without keeping it alive
    }

    public static void Run()
    {
        var service = new OrderService();
        var email = new EmailNotifier();
        var inventory = new InventoryReserver();

        service.OrderPlaced += email.Handle;
        service.OrderPlaced += inventory.Handle;
        service.OrderPlaced += (_, e) => { if (e.Total > 50_000m) Console.WriteLine($"  FraudCheck: review order {e.OrderId}"); };

        Console.WriteLine($"Subscribers: {service.SubscriberCount}");
        service.PlaceOrder(1042, 75_000m);

        service.OrderPlaced -= email.Handle;
        Console.WriteLine($"After email unsubscribed: {service.SubscriberCount} subscribers");
        service.PlaceOrder(1043, 900m);

        // From OUTSIDE the class you can only += and -=. These lines do not compile:
        // service.OrderPlaced(service, args);   // CS0070: can only appear on the left of += or -=
        // service.OrderPlaced = null;           // CS0070: would wipe every other subscriber

        // ---- Memory leak: the publisher keeps the subscriber alive ----
        var longLivedService = new OrderService();      // imagine a singleton
        WeakReference leaked = CreateWidget(longLivedService, unsubscribe: false);
        WeakReference released = CreateWidget(longLivedService, unsubscribe: true);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Console.WriteLine($"Widget that never unsubscribed still alive after GC: {leaked.IsAlive}");
        Console.WriteLine($"Widget that unsubscribed still alive after GC:       {released.IsAlive}");
    }
}
