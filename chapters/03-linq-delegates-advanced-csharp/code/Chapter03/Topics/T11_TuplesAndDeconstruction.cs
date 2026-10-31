namespace Chapter03.Topics;

public static class T11_TuplesAndDeconstruction
{
    private record CartItem(string Sku, int Quantity, decimal UnitPrice);

    // A positional record gets a Deconstruct method for free
    private record ShippingQuote(string Courier, decimal Cost, int Days);

    // A normal class can support deconstruction by adding a Deconstruct method
    private sealed class Address(string line1, string city, string pinCode)
    {
        public string Line1 { get; } = line1;
        public string City { get; } = city;
        public string PinCode { get; } = pinCode;

        public void Deconstruct(out string city, out string pinCode)
        {
            city = City;
            pinCode = PinCode;
        }
    }

    // Return several values without creating a class just for that
    private static (decimal Subtotal, decimal Tax, decimal Total) CalculateCart(IEnumerable<CartItem> items, decimal taxRate)
    {
        decimal subtotal = items.Sum(i => i.Quantity * i.UnitPrice);
        decimal tax = Math.Round(subtotal * taxRate, 2);
        return (subtotal, tax, subtotal + tax);
    }

    // The Try pattern with a tuple instead of an out parameter
    private static (bool Found, decimal Rate) TryGetTaxRate(string state) => state switch
    {
        "MH" => (true, 0.18m),
        "KA" => (true, 0.18m),
        _ => (false, 0m),
    };

    public static void Run()
    {
        var cart = new List<CartItem> { new("KB-01", 1, 2_500m), new("MS-02", 2, 800m) };

        // ---- 1. Named tuple elements ----
        var totals = CalculateCart(cart, 0.18m);
        Console.WriteLine($"Subtotal {totals.Subtotal}, tax {totals.Tax}, total {totals.Total}");
        Console.WriteLine($"Item1 still works: {totals.Item1}. ToString(): {totals}");

        // ---- 2. Deconstruction into separate variables ----
        var (subtotal, tax, total) = CalculateCart(cart, 0.05m);
        Console.WriteLine($"Deconstructed: {subtotal} + {tax} = {total}");

        (_, _, decimal onlyTotal) = CalculateCart([], 0.18m);   // discard what you don't need; empty cart is fine
        Console.WriteLine($"Empty cart total: {onlyTotal}");

        var (found, rate) = TryGetTaxRate("GOA");
        Console.WriteLine($"Tax rate for GOA found: {found}, rate {rate}");

        // ---- 3. Deconstructing records, classes and dictionary entries ----
        var (courier, cost, days) = new ShippingQuote("BlueDart", 120m, 2);
        Console.WriteLine($"Record: {courier} costs {cost} and takes {days} days");

        var (city, pin) = new Address("12 MG Road", "Pune", "411001");
        Console.WriteLine($"Class with Deconstruct: {city} {pin}");

        var stock = new Dictionary<string, int> { ["KB-01"] = 14, ["MS-02"] = 0 };
        foreach (var (sku, qty) in stock)                       // KeyValuePair has Deconstruct (.NET Core 2.0+)
            Console.WriteLine($"  {sku}: {(qty == 0 ? "out of stock" : qty)}");

        // ---- 4. Swap without a temp variable ----
        decimal oldPrice = 999m, newPrice = 799m;
        (oldPrice, newPrice) = (newPrice, oldPrice);
        Console.WriteLine($"Swapped: old {oldPrice}, new {newPrice}");

        // ---- 5. Equality and names ----
        var a = (Sku: "KB-01", Qty: 2);
        var b = (Code: "KB-01", Count: 2);                      // different names, same values
        Console.WriteLine($"(KB-01, 2) == (KB-01, 2) with different names: {a == b}");
        // Names exist only at compile time. At runtime both are ValueTuple<string, int>.
        Console.WriteLine($"Runtime type: {a.GetType().Name}<{string.Join(", ", a.GetType().GetGenericArguments().Select(t => t.Name))}>");

        // ---- 6. ValueTuple is a mutable struct: copies are independent ----
        var original = (Sku: "KB-01", Qty: 2);
        var copy = original;
        copy.Qty = 99;
        Console.WriteLine($"Original Qty {original.Qty}, copy Qty {copy.Qty}");

        // ---- 7. Tuples as composite dictionary keys ----
        var priceByRegionAndSku = new Dictionary<(string Region, string Sku), decimal>
        {
            [("IN", "KB-01")] = 2_500m,
            [("US", "KB-01")] = 30m,
        };
        Console.WriteLine($"Price for (US, KB-01): {priceByRegionAndSku[("US", "KB-01")]}");

        // Tuples in LINQ: a lightweight projection
        var lines = cart.Select(i => (i.Sku, LineTotal: i.Quantity * i.UnitPrice));
        Console.WriteLine($"LINQ tuples: {string.Join(", ", lines.Select(l => $"{l.Sku}={l.LineTotal}"))}");
    }
}
