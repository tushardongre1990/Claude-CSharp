namespace Chapter01.Topics;

public static class T10_OopPillars
{
    // ===== ENCAPSULATION: hide the data, expose safe operations =====
    private class BankAccount
    {
        private decimal _balance;                 // nobody outside can touch this directly

        public decimal Balance => _balance;       // read-only view

        public void Deposit(decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Deposit must be positive");
            _balance += amount;
        }

        public bool Withdraw(decimal amount)
        {
            if (amount > _balance) return false;  // the rule is enforced in ONE place
            _balance -= amount;
            return true;
        }
    }

    // ===== ABSTRACTION + INHERITANCE: shipping methods at checkout =====
    private abstract class ShippingMethod
    {
        public string Name { get; }

        protected ShippingMethod(string name) => Name = name;

        public abstract decimal Cost(decimal weightKg);   // WHAT every shipping method must do, not HOW

        public virtual string Quote(decimal weightKg) => $"{Name}: Rs {Cost(weightKg)} for {weightKg} kg";
    }

    private class StandardShipping : ShippingMethod    // StandardShipping IS-A ShippingMethod
    {
        public StandardShipping() : base("Standard") { }

        public override decimal Cost(decimal weightKg) => 50 + 10 * weightKg;
    }

    private class ExpressShipping : ShippingMethod
    {
        public ExpressShipping() : base("Express") { }

        public override decimal Cost(decimal weightKg) => 120 + 25 * weightKg;

        public override string Quote(decimal weightKg) => base.Quote(weightKg) + " (delivers tomorrow)";
    }

    // ===== Compile-time polymorphism: overloading =====
    private static class GstCalculator
    {
        public static decimal Tax(decimal amount) => amount * 0.18m;                                // default 18%
        public static decimal Tax(decimal amount, decimal rate) => amount * rate;                    // custom rate
        public static decimal Tax(int quantity, decimal unitPrice) => quantity * unitPrice * 0.18m;  // per line item
    }

    public static void Run()
    {
        // Encapsulation
        var account = new BankAccount();
        account.Deposit(1000);
        Console.WriteLine($"Withdraw 300: {account.Withdraw(300)}, Withdraw 5000: {account.Withdraw(5000)}, Balance: {account.Balance}");
        // account._balance = 1_000_000;   // COMPILE ERROR: _balance is private

        // Runtime polymorphism: the checkout page loops over options without knowing the concrete types
        var options = new List<ShippingMethod> { new StandardShipping(), new ExpressShipping() };
        foreach (var option in options)
            Console.WriteLine(option.Quote(2m));

        // var s = new ShippingMethod("x");   // COMPILE ERROR: cannot create an instance of an abstract class

        // Compile-time polymorphism
        Console.WriteLine($"Tax(1000) = {GstCalculator.Tax(1000m)}, Tax(1000, 0.05) = {GstCalculator.Tax(1000m, 0.05m)}, Tax(3 items x 200) = {GstCalculator.Tax(3, 200m)}");
    }
}
