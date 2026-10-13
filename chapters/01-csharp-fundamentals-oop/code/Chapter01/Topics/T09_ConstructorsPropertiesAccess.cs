namespace Chapter01.Topics;

public static class T09_ConstructorsPropertiesAccess
{
    private static string Say(string message)
    {
        Console.WriteLine($"  {message}");
        return message;
    }

    // --- Order in which constructors and initializers run ---
    // A base repository that every repository in the app inherits from
    private class RepositoryBase
    {
        private readonly string _connectionString = Say("3. RepositoryBase field initializer (connection string)");

        static RepositoryBase() => Say("2. RepositoryBase static constructor (only the first time)");

        public RepositoryBase() => Say("4. RepositoryBase constructor (opens connection settings)");
    }

    private class OrderRepository : RepositoryBase
    {
        private readonly string _tableName = Say("1. OrderRepository field initializer (table name)");

        static OrderRepository() => Say("0. OrderRepository static constructor (only the first time)");

        public OrderRepository() : base() => Say("5. OrderRepository constructor");
    }

    // --- Constructor chaining with this(...) ---
    private class Customer
    {
        public string Name { get; }
        public string Country { get; }

        public Customer(string name) : this(name, "India") { }   // calls the other constructor first

        public Customer(string name, string country)
        {
            Name = name;
            Country = country;
        }
    }

    // --- Property styles ---
    private class Product
    {
        public int Id { get; init; }                  // can only be set while the object is being created
        public required string Name { get; set; }     // MUST be set when creating the object (C# 11)

        public decimal Price                          // 'field' = the compiler-made backing field (C# 14)
        {
            get => field;
            set => field = value >= 0 ? value : throw new ArgumentException("Price cannot be negative");
        }

        public int Stock { get; private set; }        // anyone can read, only this class can change it
        public bool InStock => Stock > 0;             // computed, read-only property

        public void Restock(int quantity) => Stock += quantity;
    }

    // --- Access modifiers on a payment gateway client ---
    public class PaymentGatewayClient
    {
        public string ProviderName = "CardPay";             // anyone
        private string _apiKey = "sk_live_123";             // only this class (secrets!)
        protected int TimeoutSeconds = 30;                  // this class + child classes (e.g. a RetryingGatewayClient)
        internal string BaseUrl = "https://api.cardpay";    // anything in the same project (assembly)
        protected internal int MaxRetries = 3;              // same project OR child classes anywhere
        private protected string _signingSecret = "whsec";  // child classes, but only in the same project

        public string Describe() =>
            $"{ProviderName} at {BaseUrl}, timeout {TimeoutSeconds}s, retries {MaxRetries}, key ends with {_apiKey[^3..]}, secret length {_signingSecret.Length}";
    }

    public static void Run()
    {
        Console.WriteLine("Creating the first OrderRepository:");
        _ = new OrderRepository();
        Console.WriteLine("Creating the second OrderRepository:");
        _ = new OrderRepository();

        var customer = new Customer("Asha");
        Console.WriteLine($"Customer: {customer.Name}, {customer.Country}");

        var p = new Product { Id = 1, Name = "Keyboard", Price = 1500m };
        // p.Id = 2;                            // COMPILE ERROR: init-only property
        // var bad = new Product { Id = 2 };    // COMPILE ERROR: required member 'Name' must be set
        // p.Stock = 100;                       // COMPILE ERROR: the setter is private
        p.Restock(5);
        Console.WriteLine($"Product: Id={p.Id}, Name={p.Name}, Price={p.Price}, Stock={p.Stock}, InStock={p.InStock}");

        try
        {
            p.Price = -1;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Setting a negative price: {ex.Message}");
        }

        var gateway = new PaymentGatewayClient();
        // gateway._apiKey                       // COMPILE ERROR: private
        // gateway.TimeoutSeconds                // COMPILE ERROR: protected
        Console.WriteLine($"From outside: ProviderName = {gateway.ProviderName}, BaseUrl (internal, same project) = {gateway.BaseUrl}");
        Console.WriteLine($"From inside:  {gateway.Describe()}");
    }
}
