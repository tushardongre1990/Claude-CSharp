namespace Chapter01.Topics;

public static class T03_ValueVsReference
{
    // A struct is a VALUE type: the variable holds the data itself.
    private struct DeliveryLocation
    {
        public double Latitude;
        public double Longitude;
    }

    // A class is a REFERENCE type: the variable holds an address that points to the data.
    private class ShoppingCart
    {
        public decimal Total;
    }

    public static void Run()
    {
        // --- Copying a value type copies the DATA ---
        var warehouse = new DeliveryLocation { Latitude = 18.52, Longitude = 73.85 };
        var customer = warehouse;          // full, independent copy
        customer.Latitude = 19.07;         // move the customer's pin
        Console.WriteLine($"struct: warehouse.Latitude = {warehouse.Latitude}, customer.Latitude = {customer.Latitude}   (independent copies)");

        // --- Copying a reference type copies the ADDRESS ---
        var cart = new ShoppingCart { Total = 1000 };
        var cartInSession = cart;          // both variables point to the SAME cart
        cartInSession.Total += 500;        // the user adds an item through the session
        Console.WriteLine($"class:  cart.Total = {cart.Total}, cartInSession.Total = {cartInSession.Total}  (same object)");
        Console.WriteLine($"cart and cartInSession are the same object? {ReferenceEquals(cart, cartInSession)}");

        // --- Passing to a method works the same way ---
        var pin = new DeliveryLocation { Latitude = 18.52, Longitude = 73.85 };
        SnapToNearestRoad(pin);
        Console.WriteLine($"After SnapToNearestRoad: pin.Latitude = {pin.Latitude}   (the method changed a copy)");

        var cart2 = new ShoppingCart { Total = 1000 };
        ApplyFlatDiscount(cart2);
        Console.WriteLine($"After ApplyFlatDiscount: cart2.Total = {cart2.Total}  (the method changed the real cart)");

        // --- Replacing the reference inside a method does NOT affect the caller ---
        var cart3 = new ShoppingCart { Total = 1000 };
        ResetCart(cart3);
        Console.WriteLine($"After ResetCart: cart3.Total = {cart3.Total}   (the method only replaced its own copy of the address)");

        // --- Default values ---
        DeliveryLocation noLocation = default;
        ShoppingCart? noCart = default;
        Console.WriteLine($"default struct Latitude = {noLocation.Latitude}, default class is null? {noCart is null}");
    }

    private static void SnapToNearestRoad(DeliveryLocation location) => location.Latitude = 18.53;
    private static void ApplyFlatDiscount(ShoppingCart cart) => cart.Total -= 200;
    private static void ResetCart(ShoppingCart cart) => cart = new ShoppingCart { Total = 0 };
}
