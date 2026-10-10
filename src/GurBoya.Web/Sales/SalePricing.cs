using System.Globalization;
using GurBoya.Web.Inventory;

namespace GurBoya.Web.Sales;

public static class SalePricing
{
    public static decimal Down(decimal value) => decimal.Floor(value * 100) / 100;
    public static decimal Up(decimal value) => decimal.Ceiling(value * 100) / 100;
    public static long Quantity(string? value, bool allowZero = false)
    {
        if (!long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var amount)
            || amount < (allowZero ? 0 : 1) || amount > 1000000000)
            throw new InventoryException("Miktarı tam sayı girin (en fazla 1.000.000.000).");
        return amount;
    }
    private static decimal Discount(decimal basis, string kind, string value, out decimal entered)
    {
        if (kind is not ("TL" or "PERCENT")) throw new InventoryException("İndirim türü TL veya yüzde olmalı.");
        entered = Pricing.Parse(value, "İndirim", 2, kind == "PERCENT" ? 100 : 999999999);
        var amount = kind == "PERCENT" ? basis * entered / 100 : entered;
        if (amount > basis) throw new InventoryException("İndirim tutarı satış tutarını aşamaz.");
        return amount;
    }
    public static Sale Calculate(SaleInput input, IReadOnlyDictionary<long, Product> products)
    {
        if (input.Lines.Count is < 1 or > 50) throw new InventoryException("Satışa 1–50 ürün satırı ekleyin.");
        var sale = new Sale { OperationId = input.OperationId, PaymentMethod = input.PaymentMethod, DiscountKind = input.DiscountKind };
        var bases = new List<decimal>();
        foreach (var item in input.Lines)
        {
            if (!products.TryGetValue(item.ProductId, out var product)) throw new InventoryException("Ürün bulunamadı.");
            if (!product.IsActive) throw new InventoryException($"{product.Name}: ürün pasif.");
            if (item.Version != product.Version) throw new InventoryException($"{product.Name}: fiyat veya stok değişti. Ürünü sepetten çıkarıp yeniden ekleyin.");
            var quantity = Quantity(item.Quantity);
            var fee = Pricing.Parse(item.TintFee, "KDV dahil renklendirme ücreti", 2);
            if ((!product.IsPaint && item.IsTinted) || (!item.IsTinted && fee != 0))
                throw new InventoryException("Renklendirme ücreti yalnız renklendirilen boya satırına eklenebilir.");
            if ((item.Color ?? "").Length > 100) throw new InventoryException("Renk kodu en fazla 100 karakter olmalı.");
            var gross = (product.VatIncluded ? product.SalePrice : product.SalePrice * (1 + product.VatRate / 100)) * quantity + fee;
            if (gross > 999999999) throw new InventoryException("Bir satış satırı en fazla 999.999.999 TL olabilir.");
            var discount = Discount(gross, item.DiscountKind, item.DiscountValue, out var value);
            sale.Lines.Add(new()
            {
                Position = sale.Lines.Count + 1,
                ProductId = product.Id,
                ProductName = product.Name,
                Brand = product.Brand,
                Color = item.IsTinted ? (item.Color ?? "").Trim() : product.Color,
                Unit = product.Unit,
                PackageLiters = product.PackageLiters,
                Quantity = quantity,
                UnitPrice = product.SalePrice,
                VatIncluded = product.VatIncluded,
                VatRate = product.VatRate,
                IsTinted = item.IsTinted,
                TintFee = fee,
                DiscountKind = item.DiscountKind,
                DiscountValue = value,
                LineDiscount = discount
            });
            bases.Add(gross - discount);
        }
        foreach (var group in sale.Lines.GroupBy(line => line.ProductId))
            if (group.Sum(line => line.Quantity) > products[group.Key].Quantity)
                throw new InventoryException($"{products[group.Key].Name}: stokta yok veya miktar yetersiz.");
        var basis = bases.Sum();
        var receiptDiscount = Discount(basis, input.DiscountKind, input.DiscountValue, out var receiptValue);
        sale.DiscountValue = receiptValue;
        // Distribute the receipt discount at high precision, assigning the residual to the last nonzero line.
        var last = bases.FindLastIndex(value => value > 0);
        decimal assigned = 0;
        var exact = new decimal[bases.Count];
        for (var i = 0; i < bases.Count; i++)
        {
            var share = i == last ? receiptDiscount - assigned : basis == 0 ? 0 : receiptDiscount * bases[i] / basis;
            assigned += share;
            sale.Lines[i].ReceiptDiscount = share;
            exact[i] = bases[i] - share;
            sale.Lines[i].Total = Down(exact[i]);
        }
        // Round only the receipt down; allocate its cents by largest remainder, deterministically.
        var cents = (int)((Down(basis - receiptDiscount) - sale.Lines.Sum(line => line.Total)) * 100);
        foreach (var index in Enumerable.Range(0, exact.Length).OrderByDescending(i => exact[i] - sale.Lines[i].Total).ThenBy(i => i).Take(cents))
            sale.Lines[index].Total += 0.01m;
        for (var i = 0; i < sale.Lines.Count; i++)
        {
            var line = sale.Lines[i];
            line.Rounding = exact[i] - line.Total;
            line.Net = decimal.Round(line.Total / (1 + line.VatRate / 100), 2, MidpointRounding.AwayFromZero);
            line.Vat = line.Total - line.Net;
        }
        sale.Net = sale.Lines.Sum(line => line.Net); sale.Vat = sale.Lines.Sum(line => line.Vat); sale.Total = sale.Lines.Sum(line => line.Total);
        if (sale.Total > 999999999) throw new InventoryException("Fiş toplamı en fazla 999.999.999 TL olabilir.");
        return sale;
    }
}
