namespace Chapter01.Topics;

public static class T06_MethodParameters
{
    public static void Run()
    {
        // --- Optional and named parameters ---
        SendOtp("+91-9000000001");                                // uses the default channel and expiry
        SendOtp("+91-9000000002", channel: "WhatsApp");           // named argument
        SendOtp(expiryMinutes: 10, phone: "+91-9000000003");      // named arguments can be in any order

        // --- ref: pass the variable itself. It must be assigned before the call ---
        int loyaltyPoints = 120;
        AddLoyaltyPoints(ref loyaltyPoints, orderAmount: 2500m);
        Console.WriteLine($"After AddLoyaltyPoints(ref loyaltyPoints): {loyaltyPoints}");

        // --- out: the method MUST assign it. The caller does not need to initialise it ---
        if (TryGetCouponDiscount("FESTIVE20", out int percent))
            Console.WriteLine($"Coupon FESTIVE20 gives {percent}% off");
        if (!TryGetCouponDiscount("EXPIRED10", out _))           // _ is a "discard": I don't need this value
            Console.WriteLine("Coupon EXPIRED10 is not valid");

        // --- in: pass by reference but READ-ONLY (useful for large structs) ---
        var deliveryZone = new BoundingBox(18.40, 73.70, 18.65, 74.00);
        Console.WriteLine($"Is (18.52, 73.85) inside the Pune delivery zone? {IsInside(in deliveryZone, 18.52, 73.85)}");

        // --- params: pass any number of arguments ---
        Console.WriteLine($"OrderTotal(499, 1299, 99) = {OrderTotal(499m, 1299m, 99m)}");
        Console.WriteLine($"OrderTotal() = {OrderTotal()}");

        // --- ref with a reference type: lets the method REPLACE the caller's object ---
        var cartItems = new List<string> { "Mouse" };
        ReplaceWithSavedCart(cartItems);
        Console.WriteLine($"After ReplaceWithSavedCart(cartItems):          count = {cartItems.Count}");
        ReplaceWithSavedCartByRef(ref cartItems);
        Console.WriteLine($"After ReplaceWithSavedCartByRef(ref cartItems): count = {cartItems.Count}");
    }

    private static void SendOtp(string phone, string channel = "SMS", int expiryMinutes = 5)
        => Console.WriteLine($"OTP sent to {phone} via {channel}, valid for {expiryMinutes} min");

    // 1 point for every 100 rupees spent
    private static void AddLoyaltyPoints(ref int points, decimal orderAmount) => points += (int)(orderAmount / 100);

    private static bool TryGetCouponDiscount(string couponCode, out int percent)
    {
        if (couponCode == "FESTIVE20")
        {
            percent = 20;
            return true;
        }
        percent = 0;         // out parameters must be assigned on every path
        return false;
    }

    // A 32-byte struct: copying it on every call would be wasteful
    private readonly struct BoundingBox
    {
        public readonly double MinLatitude;
        public readonly double MinLongitude;
        public readonly double MaxLatitude;
        public readonly double MaxLongitude;

        public BoundingBox(double minLat, double minLng, double maxLat, double maxLng)
        {
            MinLatitude = minLat;
            MinLongitude = minLng;
            MaxLatitude = maxLat;
            MaxLongitude = maxLng;
        }
    }

    private static bool IsInside(in BoundingBox zone, double lat, double lng)
    {
        // zone = new BoundingBox(0, 0, 0, 0);   // COMPILE ERROR: cannot assign to an 'in' parameter
        return lat >= zone.MinLatitude && lat <= zone.MaxLatitude
            && lng >= zone.MinLongitude && lng <= zone.MaxLongitude;
    }

    private static decimal OrderTotal(params decimal[] lineAmounts) => lineAmounts.Sum();

    private static void ReplaceWithSavedCart(List<string> items) => items = new List<string> { "Laptop", "Bag", "Charger" };
    private static void ReplaceWithSavedCartByRef(ref List<string> items) => items = new List<string> { "Laptop", "Bag", "Charger" };
}
