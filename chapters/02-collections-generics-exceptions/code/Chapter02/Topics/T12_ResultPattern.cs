using System.Diagnostics;

namespace Chapter02.Topics;

// ---- A small Result type ----
public record Error(string Code, string Message);

public readonly record struct Result<T>
{
    public T? Value { get; }
    public Error? Error { get; }
    public bool IsSuccess => Error is null;

    private Result(T? value, Error? error) { Value = value; Error = error; }

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(Error error) => new(default, error);
}

public record Coupon(string Code, decimal Percent, decimal MinOrder, DateOnly ExpiresOn);

public static class T12_ResultPattern
{
    private static readonly Dictionary<string, Coupon> Coupons = new(StringComparer.OrdinalIgnoreCase)
    {
        ["DIWALI10"] = new("DIWALI10", 10m, 500m, new DateOnly(2026, 11, 15)),
        ["SUMMER20"] = new("SUMMER20", 20m, 1_000m, new DateOnly(2026, 6, 30)),
    };

    private static bool _databaseDown;

    // Expected outcomes (unknown code, expired, minimum not met) are RETURNED as errors.
    // A truly unexpected failure (database unreachable) is THROWN.
    private static Result<decimal> ApplyCoupon(string code, decimal orderTotal, DateOnly today)
    {
        if (_databaseDown) throw new TimeoutException("Coupon database did not respond.");

        if (!Coupons.TryGetValue(code, out var coupon))
            return Result<decimal>.Failure(new("coupon.not_found", $"Coupon '{code}' does not exist."));
        if (today > coupon.ExpiresOn)
            return Result<decimal>.Failure(new("coupon.expired", $"Coupon '{coupon.Code}' expired on {coupon.ExpiresOn:yyyy-MM-dd}."));
        if (orderTotal < coupon.MinOrder)
            return Result<decimal>.Failure(new("coupon.min_order", $"Add {coupon.MinOrder - orderTotal} more to use '{coupon.Code}'."));

        return Result<decimal>.Success(orderTotal - orderTotal * coupon.Percent / 100);
    }

    // What an API endpoint would do with the result: map error codes to HTTP status codes
    private static string ToHttpResponse(Result<decimal> result) => result switch
    {
        { IsSuccess: true } => $"200 OK, new total {result.Value}",
        { Error.Code: "coupon.not_found" } => $"404 Not Found: {result.Error.Message}",
        _ => $"422 Unprocessable: {result.Error!.Message}",
    };

    public static void Run()
    {
        var today = new DateOnly(2026, 10, 20);
        Console.WriteLine($"DIWALI10 on 800:  {ToHttpResponse(ApplyCoupon("diwali10", 800m, today))}");
        Console.WriteLine($"DIWALI10 on 300:  {ToHttpResponse(ApplyCoupon("DIWALI10", 300m, today))}");
        Console.WriteLine($"SUMMER20 on 2000: {ToHttpResponse(ApplyCoupon("SUMMER20", 2_000m, today))}");
        Console.WriteLine($"FREE100:          {ToHttpResponse(ApplyCoupon("FREE100", 800m, today))}");

        _databaseDown = true;
        try { ApplyCoupon("DIWALI10", 800m, today); }
        catch (TimeoutException ex) { Console.WriteLine($"Database down:    exception -> global handler -> 500/503 ({ex.Message})"); }
        _databaseDown = false;

        // The built-in version of the same idea: TryParse instead of Parse
        string[] quantitiesFromCsv = ["3", "two", "-1", "5"];
        foreach (var raw in quantitiesFromCsv)
            Console.WriteLine($"  qty '{raw}': {(int.TryParse(raw, out var q) && q > 0 ? $"ok {q}" : "rejected")}");

        // ---- Cost: an exception is far more expensive than a returned value ----
        const int iterations = 100_000;
        try { ApplyCouponThrowing("NOPE"); } catch (KeyNotFoundException) { }   // warm-up (JIT) so it is not timed
        _ = ApplyCoupon("NOPE", 800m, today);

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            try { ApplyCouponThrowing("NOPE"); } catch (KeyNotFoundException) { }
        }
        var throwTime = sw.Elapsed;

        sw.Restart();
        for (int i = 0; i < iterations; i++)
            _ = ApplyCoupon("NOPE", 800m, today);
        var resultTime = sw.Elapsed;

        Console.WriteLine($"{iterations:N0} invalid coupons: throw/catch {throwTime.TotalMilliseconds:N0} ms, Result {resultTime.TotalMilliseconds:N0} ms");
    }

    private static void ApplyCouponThrowing(string code)
    {
        if (!Coupons.ContainsKey(code)) throw new KeyNotFoundException($"Coupon '{code}' does not exist.");
    }
}
