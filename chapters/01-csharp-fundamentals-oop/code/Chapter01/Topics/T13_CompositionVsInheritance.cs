namespace Chapter01.Topics;

public static class T13_CompositionVsInheritance
{
    // ===== The inheritance trap =====
    private class Payment
    {
        public decimal Amount { get; init; }
        public virtual string Refund() => $"Refunded {Amount} to the original payment method";
    }

    private class CardPayment : Payment { }

    private class GiftCardPayment : Payment   // "a gift card payment IS-A payment"... but it can't be refunded to source
    {
        public override string Refund() => throw new NotSupportedException("Gift card payments cannot be refunded to source");
    }

    // ===== Composition: the refund behaviour is a separate, swappable part =====
    private interface IRefundPolicy { string Refund(decimal amount); }
    private class RefundToSource : IRefundPolicy { public string Refund(decimal amount) => $"Refunded {amount} to the original payment method"; }
    private class RefundAsStoreCredit : IRefundPolicy { public string Refund(decimal amount) => $"Issued {amount} as store credit"; }

    private class ComposedPayment
    {
        private readonly IRefundPolicy _refundPolicy;     // a payment HAS-A refund policy

        public string Method { get; }
        public decimal Amount { get; }

        public ComposedPayment(string method, decimal amount, IRefundPolicy refundPolicy)
        {
            Method = method;
            Amount = amount;
            _refundPolicy = refundPolicy;
        }

        public string Refund() => $"{Method}: {_refundPolicy.Refund(Amount)}";
    }

    // ===== Real backend example: a service HAS a repository =====
    private interface IOrderRepository { void Save(string order); }

    private class InMemoryOrderRepository : IOrderRepository
    {
        public List<string> Saved { get; } = new();
        public void Save(string order) => Saved.Add(order);
    }

    private class OrderService
    {
        private readonly IOrderRepository _repository;

        public OrderService(IOrderRepository repository) => _repository = repository;

        public void PlaceOrder(string item)
        {
            _repository.Save(item);
            Console.WriteLine($"  Order placed: {item}");
        }
    }

    public static void Run()
    {
        // The refund job processes all payments of a cancelled order
        var payments = new List<Payment> { new CardPayment { Amount = 800 }, new GiftCardPayment { Amount = 200 } };
        foreach (var payment in payments)
        {
            try
            {
                Console.WriteLine(payment.Refund());
            }
            catch (NotSupportedException ex)
            {
                Console.WriteLine($"Inheritance problem, the refund job crashed: {ex.Message}");
            }
        }

        var composed = new List<ComposedPayment>
        {
            new("Card", 800, new RefundToSource()),
            new("Gift card", 200, new RefundAsStoreCredit()),
        };
        foreach (var payment in composed)
            Console.WriteLine(payment.Refund());

        var repository = new InMemoryOrderRepository();
        var service = new OrderService(repository);
        service.PlaceOrder("Laptop");
        Console.WriteLine($"Repository now has {repository.Saved.Count} order(s)");
    }
}
