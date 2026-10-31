using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Chapter03.Topics;

// ---- A custom attribute: marks properties that must never appear in logs ----
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class SensitiveAttribute : Attribute
{
    public string Reason { get; }
    public SensitiveAttribute(string reason) => Reason = reason;
}

// Marks payment handler classes with the payment method they process
[AttributeUsage(AttributeTargets.Class)]
public sealed class PaymentMethodAttribute(string code) : Attribute
{
    public string Code { get; } = code;
}

public class CustomerRecord
{
    public int Id { get; set; }

    [Required, EmailAddress]                     // built-in validation attributes (used by ASP.NET Core)
    public string Email { get; set; } = "";

    [Sensitive("PII")]
    public string Phone { get; set; } = "";

    [Sensitive("PCI-DSS")]
    public string CardNumber { get; set; } = "";

    public string City { get; set; } = "";
}

public interface IPaymentHandler
{
    string Pay(decimal amount);
}

[PaymentMethod("card")]
public sealed class CardPaymentHandler : IPaymentHandler
{
    public string Pay(decimal amount) => $"Charged {amount} to card";
}

[PaymentMethod("upi")]
public sealed class UpiPaymentHandler : IPaymentHandler
{
    public string Pay(decimal amount) => $"Collected {amount} via UPI";
}

public sealed class LegacyChequeHandler : IPaymentHandler     // no attribute: ignored by the scanner
{
    public string Pay(decimal amount) => "Cheque";
}

public static class T14_AttributesAndReflection
{
    // Builds a log-safe string for ANY object: sensitive properties are masked
    private static string ToSafeLogString(object obj)
    {
        var parts = new List<string>();
        foreach (PropertyInfo property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var sensitive = property.GetCustomAttribute<SensitiveAttribute>();
            object? value = property.GetValue(obj);
            parts.Add(sensitive is null ? $"{property.Name}={value}" : $"{property.Name}=***({sensitive.Reason})");
        }
        return $"{obj.GetType().Name} {{ {string.Join(", ", parts)} }}";
    }

    [Obsolete("Use ToSafeLogString instead. Will be removed in v3.", error: false)]
    private static string OldLogFormat(object obj) => obj.ToString()!;

    public static void Run()
    {
        var customer = new CustomerRecord { Id = 7, Email = "asha@shop.in", Phone = "9876543210", CardNumber = "4111111111111111", City = "Pune" };

        // ---- 1. Reading attributes with reflection ----
        Console.WriteLine(ToSafeLogString(customer));

        // ---- 2. The same attributes are used by framework code: DataAnnotations validation ----
        var invalid = new CustomerRecord { Id = 8, Email = "not-an-email" };
        var errors = new List<ValidationResult>();
        bool valid = Validator.TryValidateObject(invalid, new ValidationContext(invalid), errors, validateAllProperties: true);
        Console.WriteLine($"Valid: {valid}; errors: {string.Join(" | ", errors.Select(e => e.ErrorMessage))}");

        // ---- 3. Inspecting a type ----
        Type type = typeof(CustomerRecord);
        Console.WriteLine($"Type {type.Name}: {type.GetProperties().Length} properties, namespace {type.Namespace}");
        var sensitiveNames = type.GetProperties().Where(p => p.IsDefined(typeof(SensitiveAttribute))).Select(p => p.Name);
        Console.WriteLine($"Sensitive properties: {string.Join(", ", sensitiveNames)}");

        // ---- 4. Assembly scanning: find every IPaymentHandler with a [PaymentMethod] and create it ----
        var handlers = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IPaymentHandler).IsAssignableFrom(t))
            .Select(t => (Type: t, Attr: t.GetCustomAttribute<PaymentMethodAttribute>()))
            .Where(x => x.Attr is not null)
            .ToDictionary(x => x.Attr!.Code, x => (IPaymentHandler)Activator.CreateInstance(x.Type)!);
        Console.WriteLine($"Handlers found: {string.Join(", ", handlers.Keys.Order())}");
        Console.WriteLine($"upi -> {handlers["upi"].Pay(499m)}");

        // ---- 5. Reflection can read private members, which is why it is powerful and fragile ----
        MethodInfo method = typeof(T14_AttributesAndReflection).GetMethod(nameof(OldLogFormat), BindingFlags.NonPublic | BindingFlags.Static)!;
        var obsolete = method.GetCustomAttribute<ObsoleteAttribute>()!;
        Console.WriteLine($"Private method {method.Name} is [Obsolete]: \"{obsolete.Message}\"");

        // ---- 6. nameof is compile-time, not reflection: free and refactor-safe ----
        Console.WriteLine($"nameof(CustomerRecord.CardNumber) = {nameof(CustomerRecord.CardNumber)}");
    }
}
