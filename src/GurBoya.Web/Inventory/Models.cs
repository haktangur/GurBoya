using System.ComponentModel.DataAnnotations;

namespace GurBoya.Web.Inventory;

public sealed class Product
{
    public long Id { get; set; }
    public Guid RequestId { get; set; }
    [MaxLength(64)] public string RequestHash { get; set; } = "";
    [MaxLength(160)] public string Name { get; set; } = "";
    [MaxLength(100)] public string Brand { get; set; } = "";
    [MaxLength(100)] public string Color { get; set; } = "";
    [MaxLength(100)] public string Category { get; set; } = "";
    [MaxLength(80)] public string? Barcode { get; set; }
    public bool IsPaint { get; set; }
    public decimal? PackageLiters { get; set; }
    [MaxLength(8)] public string Unit { get; set; } = "BOX";
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal VatRate { get; set; } = 15;
    public bool VatIncluded { get; set; }
    public long Quantity { get; set; }
    public bool IsActive { get; set; } = true;
    public long Version { get; set; } = 1;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class StockMovement
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid OperationId { get; set; }
    [MaxLength(64)] public string RequestHash { get; set; } = "";
    public long Delta { get; set; }
    [MaxLength(16)] public string Kind { get; set; } = "RECEIPT";
    [MaxLength(300)] public string Reason { get; set; } = "";
    public decimal PurchaseNet { get; set; }
    [MaxLength(450)] public string ActorId { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; }
}

public sealed class PriceChange
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal VatRate { get; set; }
    public bool VatIncluded { get; set; }
    [MaxLength(300)] public string Reason { get; set; } = "";
    [MaxLength(450)] public string ActorId { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; }
}

public sealed class OperationRecord
{
    public Guid Id { get; set; }
    [MaxLength(64)] public string RequestHash { get; set; } = "";
    public long ResultId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
