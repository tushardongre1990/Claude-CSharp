namespace Chapter02.Topics;

public abstract record Payment(string Id, decimal Amount);
public record CardPayment(string Id, decimal Amount, string Last4) : Payment(Id, Amount);
public record UpiPayment(string Id, decimal Amount, string Vpa) : Payment(Id, Amount);

// out T: T only comes OUT of the interface (return values) -> covariant
public interface IPaymentSource<out T> where T : Payment
{
    T Next();
}

// in T: T only goes IN to the interface (parameters) -> contravariant
public interface IPaymentAuditor<in T> where T : Payment
{
    string Audit(T payment);
}

public class CardPaymentSource : IPaymentSource<CardPayment>
{
    public CardPayment Next() => new("pay_77", 1_999m, "4242");
}

// One auditor that understands ANY payment
public class GenericPaymentAuditor : IPaymentAuditor<Payment>
{
    public string Audit(Payment payment) => $"audited {payment.GetType().Name} {payment.Id} for {payment.Amount}";
}

public static class T07_Variance
{
    private static decimal TotalOf(IEnumerable<Payment> payments) => payments.Sum(p => p.Amount);

    public static void Run()
    {
        // ---- Covariance (out): a sequence of CardPayment IS usable as a sequence of Payment ----
        List<CardPayment> cardPayments = [new("pay_1", 500m, "1111"), new("pay_2", 250m, "2222")];
        IEnumerable<Payment> asPayments = cardPayments;          // OK: IEnumerable<out T>
        Console.WriteLine($"Total of card payments via IEnumerable<Payment>: {TotalOf(asPayments)}");

        // List<Payment> list = cardPayments;   // compile error: List<T> is invariant.
        // If it compiled, list.Add(new UpiPayment(...)) would put a UPI payment into a List<CardPayment>.

        IPaymentSource<Payment> source = new CardPaymentSource();   // OK: IPaymentSource<out T>
        Console.WriteLine($"Covariant source returned: {source.Next().GetType().Name}");

        // ---- Contravariance (in): something that handles ANY Payment can handle a CardPayment ----
        IPaymentAuditor<CardPayment> cardAuditor = new GenericPaymentAuditor();   // OK: IPaymentAuditor<in T>
        Console.WriteLine($"Contravariant auditor: {cardAuditor.Audit(cardPayments[0])}");

        Action<Payment> logAny = p => Console.WriteLine($"  log: {p.Id}");
        Action<CardPayment> logCard = logAny;                     // OK: Action<in T>
        logCard(cardPayments[1]);

        IComparer<Payment> byAmount = Comparer<Payment>.Create((a, b) => a.Amount.CompareTo(b.Amount));
        cardPayments.Sort(byAmount);                              // a List<CardPayment> sorted with an IComparer<Payment>
        Console.WriteLine($"Sorted card payments with IComparer<Payment>: {string.Join(", ", cardPayments.Select(p => p.Id))}");

        // ---- Variance only works for reference types ----
        IEnumerable<int> quantities = [1, 2, 3];
        Console.WriteLine($"IEnumerable<int> is IEnumerable<object>? {quantities is IEnumerable<object>}");
        IEnumerable<string> skus = ["SKU-1"];
        Console.WriteLine($"IEnumerable<string> is IEnumerable<object>? {skus is IEnumerable<object>}");

        // ---- The old trap: arrays are covariant, and it is checked only at runtime ----
        Payment[] batch = new CardPayment[2];                     // compiles!
        batch[0] = new CardPayment("pay_3", 10m, "3333");         // fine
        try
        {
            batch[1] = new UpiPayment("pay_4", 20m, "asha@upi");  // compiles, fails at runtime
        }
        catch (ArrayTypeMismatchException ex)
        {
            Console.WriteLine($"Array covariance: {ex.GetType().Name}");
        }
    }
}
