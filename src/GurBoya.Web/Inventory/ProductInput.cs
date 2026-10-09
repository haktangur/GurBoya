using System.ComponentModel.DataAnnotations;

namespace GurBoya.Web.Inventory;

public sealed class ProductInput
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public long Version { get; set; }
    [Required(ErrorMessage = "Ürün adı gerekli."), StringLength(160, ErrorMessage = "Ürün adı en fazla 160 karakter olabilir.")]
    public string Name { get; set; } = "";
    [StringLength(100, ErrorMessage = "Marka en fazla 100 karakter olabilir.")] public string? Brand { get; set; }
    [StringLength(100, ErrorMessage = "Renk en fazla 100 karakter olabilir.")] public string? Color { get; set; }
    [StringLength(100, ErrorMessage = "Kategori en fazla 100 karakter olabilir.")] public string? Category { get; set; }
    [StringLength(80, ErrorMessage = "Barkod en fazla 80 karakter olabilir.")] public string? Barcode { get; set; }
    public bool IsPaint { get; set; } = true;
    public string? PackageLiters { get; set; }
    public string Unit { get; set; } = "BOX";
    public string PurchasePrice { get; set; } = "0";
    public string SalePrice { get; set; } = "0";
    public string VatRate { get; set; } = "15";
    public bool VatIncluded { get; set; }
    public bool IsActive { get; set; } = true;
    [StringLength(300, ErrorMessage = "Açıklama en fazla 300 karakter olabilir.")] public string? Reason { get; set; }

    public void Apply(Product product)
    {
        if (RequestId == Guid.Empty) throw new InventoryException("İşlem kimliği eksik; sayfayı yeniden açın.");
        if (string.IsNullOrWhiteSpace(Name)) throw new InventoryException("Ürün adı gerekli.");
        if (Unit is not ("BOX" or "PIECE" or "GRAM")) throw new InventoryException("Kutu, adet veya gram seçin.");
        var liters = string.IsNullOrWhiteSpace(PackageLiters) ? (decimal?)null : Pricing.Parse(PackageLiters, "Ambalaj litresi", 3, 999999);
        if (IsPaint && (string.IsNullOrWhiteSpace(Brand) || string.IsNullOrWhiteSpace(Color) || liters is null or <= 0 || Unit != "BOX"))
            throw new InventoryException("Boya için marka, renk, pozitif ambalaj litresi ve kutu birimi gerekli.");
        product.Name = Name.Trim();
        product.Brand = Brand?.Trim() ?? "";
        product.Color = Color?.Trim() ?? "";
        product.Category = Category?.Trim() ?? "";
        product.Barcode = string.IsNullOrWhiteSpace(Barcode) ? null : Barcode.Trim();
        product.IsPaint = IsPaint;
        product.PackageLiters = liters;
        product.Unit = Unit;
        product.PurchasePrice = Pricing.Parse(PurchasePrice, "Alış fiyatı");
        product.SalePrice = Pricing.Parse(SalePrice, "Satış fiyatı");
        product.VatRate = Pricing.Parse(VatRate, "KDV oranı", 2, 100);
        product.VatIncluded = VatIncluded;
        product.IsActive = IsActive;
    }

    public static ProductInput From(Product product) => new()
    {
        Version = product.Version,
        Name = product.Name,
        Brand = product.Brand,
        Color = product.Color,
        Category = product.Category,
        Barcode = product.Barcode,
        IsPaint = product.IsPaint,
        PackageLiters = product.PackageLiters?.ToString("0.###"),
        Unit = product.Unit,
        PurchasePrice = Pricing.Money(product.PurchasePrice),
        SalePrice = Pricing.Money(product.SalePrice),
        VatRate = product.VatRate.ToString("0.##"),
        VatIncluded = product.VatIncluded,
        IsActive = product.IsActive
    };
}
