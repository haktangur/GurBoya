using System.Globalization;

namespace GurBoya.Web.Inventory;

public sealed record PriceBreakdown(decimal Net, decimal Vat, decimal Gross);

public static class Pricing
{
    public static PriceBreakdown Calculate(decimal amount, decimal rate, bool included)
    {
        var net = included ? Round(amount / (1 + rate / 100)) : amount;
        var gross = included ? amount : Round(amount * (1 + rate / 100));
        return new(net, gross - net, gross);
    }

    public static decimal Round(decimal value) => decimal.Round(value, 4, MidpointRounding.AwayFromZero);
    public static string Money(decimal value) => value.ToString("0.00##", CultureInfo.GetCultureInfo("tr-TR"));
    public static string UnitLabel(string unit) => unit switch { "BOX" => "kutu", "PIECE" => "adet", "GRAM" => "gram", _ => unit };

    public static decimal Parse(string? input, string label, int digits = 4, decimal max = 999999999)
    {
        var normalized = (input ?? "").Trim().Replace(',', '.');
        if (!decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value)
            || value < 0 || value > max || decimal.Round(value, digits) != value)
        {
            throw new InventoryException($"{label}: 0–{max} arasında, en fazla {digits} ondalık basamakla girin. Binlik ayırıcı kullanmayın.");
        }
        return value;
    }
}

public sealed class InventoryException(string message) : Exception(message);
