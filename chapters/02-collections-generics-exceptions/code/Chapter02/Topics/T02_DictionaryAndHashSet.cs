namespace Chapter02.Topics;

public static class T02_DictionaryAndHashSet
{
    private record Product(string Sku, string Name, decimal Price);

    // A key whose hash code is simply its Id, and which counts how often Equals is called
    private readonly struct GoodKey(int id) : IEquatable<GoodKey>
    {
        public static int EqualsCalls;
        public int Id { get; } = id;
        public bool Equals(GoodKey other) { EqualsCalls++; return Id == other.Id; }
        public override bool Equals(object? obj) => obj is GoodKey k && Equals(k);
        public override int GetHashCode() => Id;
    }

    // The same key, but every instance returns the same hash code: every key collides
    private readonly struct BadKey(int id) : IEquatable<BadKey>
    {
        public static int EqualsCalls;
        public int Id { get; } = id;
        public bool Equals(BadKey other) { EqualsCalls++; return Id == other.Id; }
        public override bool Equals(object? obj) => obj is BadKey k && Equals(k);
        public override int GetHashCode() => 42;
    }

    // A mutable class used as a key: a bug waiting to happen
    private class CartKey
    {
        public string CustomerEmail { get; set; } = "";
        public override bool Equals(object? obj) => obj is CartKey c && c.CustomerEmail == CustomerEmail;
        public override int GetHashCode() => CustomerEmail.GetHashCode();
    }

    public static void Run()
    {
        // ---- Dictionary: look up a value by its key ----
        var catalog = new Dictionary<string, Product>
        {
            ["SKU-100"] = new("SKU-100", "Laptop", 65_000m),
            ["SKU-200"] = new("SKU-200", "Mouse", 799m),
        };

        Console.WriteLine($"catalog[\"SKU-200\"]: {catalog["SKU-200"].Name}");

        try { _ = catalog["SKU-999"]; }
        catch (KeyNotFoundException ex) { Console.WriteLine($"Indexer with missing key: {ex.GetType().Name}"); }

        // TryGetValue: one lookup, no exception
        if (catalog.TryGetValue("SKU-999", out var missing))
            Console.WriteLine(missing.Name);
        else
            Console.WriteLine("TryGetValue(\"SKU-999\"): not found, no exception");

        try { catalog.Add("SKU-100", new("SKU-100", "Duplicate", 1m)); }
        catch (ArgumentException ex) { Console.WriteLine($"Add with duplicate key: {ex.GetType().Name}"); }

        catalog["SKU-100"] = new("SKU-100", "Laptop Pro", 89_000m);   // indexer set = add or overwrite
        Console.WriteLine($"After indexer set: {catalog["SKU-100"].Name}");
        Console.WriteLine($"TryAdd duplicate returns: {catalog.TryAdd("SKU-100", new("SKU-100", "X", 1m))}");

        // ---- Key comparison: case-insensitive coupon codes ----
        var strict = new Dictionary<string, decimal> { ["DIWALI10"] = 10m };
        var relaxed = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["DIWALI10"] = 10m };
        Console.WriteLine($"Coupon 'diwali10' found? default comparer: {strict.ContainsKey("diwali10")}, OrdinalIgnoreCase: {relaxed.ContainsKey("diwali10")}");

        // ---- HashSet: unique items, fast Contains ----
        var processedPaymentIds = new HashSet<string>();
        string[] incomingWebhooks = ["pay_1", "pay_2", "pay_1", "pay_3", "pay_2"];   // the gateway retries, so duplicates arrive
        foreach (var paymentId in incomingWebhooks)
        {
            bool isNew = processedPaymentIds.Add(paymentId);  // false if it was already there
            Console.WriteLine($"  webhook {paymentId}: {(isNew ? "process" : "skip duplicate")}");
        }

        var cartSkus = new HashSet<string> { "SKU-1", "SKU-2", "SKU-3" };
        var outOfStock = new HashSet<string> { "SKU-2", "SKU-9" };
        var blocked = new HashSet<string>(cartSkus);
        blocked.IntersectWith(outOfStock);
        Console.WriteLine($"Cart items that are out of stock: {string.Join(", ", blocked)}");

        // ---- Collisions: a good hash vs a terrible hash ----
        var good = new Dictionary<GoodKey, string>();
        var bad = new Dictionary<BadKey, string>();
        for (int i = 0; i < 1_000; i++)
        {
            good[new GoodKey(i)] = $"order-{i}";
            bad[new BadKey(i)] = $"order-{i}";
        }
        GoodKey.EqualsCalls = 0;
        BadKey.EqualsCalls = 0;
        _ = good[new GoodKey(0)];
        _ = bad[new BadKey(0)];
        Console.WriteLine($"One lookup in 1,000 items -> Equals calls: good hash {GoodKey.EqualsCalls}, constant hash {BadKey.EqualsCalls}");

        // ---- Mutating a key after inserting it ----
        var key = new CartKey { CustomerEmail = "asha@shop.com" };
        var carts = new Dictionary<CartKey, int> { [key] = 3 };   // 3 items in Asha's cart
        key.CustomerEmail = "asha.k@shop.com";                    // the customer changed their email
        Console.WriteLine($"After mutating the key: ContainsKey(same object) = {carts.ContainsKey(key)}, Count = {carts.Count}");

        // ---- Enumeration order is not something to rely on ----
        var stock = new Dictionary<string, int> { ["A"] = 1, ["B"] = 2, ["C"] = 3 };
        stock.Remove("A");
        stock["D"] = 4;
        Console.WriteLine($"Order after Remove(A) then Add(D): {string.Join(", ", stock.Keys)}");
    }
}
