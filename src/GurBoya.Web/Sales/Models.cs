using System.ComponentModel.DataAnnotations;
using GurBoya.Web.Inventory;

namespace GurBoya.Web.Sales;

public sealed class Sale
{
    public long Id { get; set; }
    public Guid OperationId { get; set; }
    [MaxLength(450)] public string ActorId { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; }
    [MaxLength(8)] public string PaymentMethod { get; set; } = "";
    [MaxLength(8)] public string DiscountKind { get; set; } = "TL";
    public decimal DiscountValue { get; set; }
    public decimal Net { get; set; }
    public decimal Vat { get; set; }
    public decimal Total { get; set; }
    public List<SaleLine> Lines { get; set; } = [];
}

public sealed class SaleLine
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public int Position { get; set; }
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
    [MaxLength(160)] public string ProductName { get; set; } = "";
    [MaxLength(100)] public string Brand { get; set; } = "";
    [MaxLength(100)] public string Color { get; set; } = "";
    [MaxLength(8)] public string Unit { get; set; } = "";
    public decimal? PackageLiters { get; set; }
    public long Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public bool VatIncluded { get; set; }
    public decimal VatRate { get; set; }
    public bool IsTinted { get; set; }
    public decimal TintFee { get; set; }
    [MaxLength(8)] public string DiscountKind { get; set; } = "TL";
    public decimal DiscountValue { get; set; }
    public decimal LineDiscount { get; set; }
    public decimal ReceiptDiscount { get; set; }
    public decimal Rounding { get; set; }
    public decimal Net { get; set; }
    public decimal Vat { get; set; }
    public decimal Total { get; set; }
}

public sealed class SalesReturn
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public Guid OperationId { get; set; }
    [MaxLength(450)] public string ActorId { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; }
    [MaxLength(300)] public string Reason { get; set; } = "";
    [MaxLength(8)] public string Kind { get; set; } = "RETURN";
    [MaxLength(8)] public string RefundMethod { get; set; } = "";
    public decimal Net { get; set; }
    public decimal Vat { get; set; }
    public decimal Total { get; set; }
    public List<ReturnLine> Lines { get; set; } = [];
}

public sealed class ReturnLine
{
    public long Id { get; set; }
    public long SalesReturnId { get; set; }
    public SalesReturn SalesReturn { get; set; } = null!;
    public long SaleLineId { get; set; }
    public SaleLine SaleLine { get; set; } = null!;
    public long Quantity { get; set; }
    public long RestockQuantity { get; set; }
    public decimal Net { get; set; }
    public decimal Vat { get; set; }
    public decimal Total { get; set; }
}

public sealed class SaleInput
{
    public Guid OperationId { get; set; } = Guid.NewGuid();
    public List<SaleLineInput> Lines { get; set; } = [];
    public string DiscountKind { get; set; } = "TL";
    public string DiscountValue { get; set; } = "0";
    public string PaymentMethod { get; set; } = "";
}
public sealed class SaleLineInput
{
    public long ProductId { get; set; }
    public long Version { get; set; }
    public string Quantity { get; set; } = "1";
    public bool IsTinted { get; set; }
    [MaxLength(100)] public string Color { get; set; } = "";
    public string TintFee { get; set; } = "0";
    public string DiscountKind { get; set; } = "TL";
    public string DiscountValue { get; set; } = "0";
}
public sealed class ReturnInput
{
    public Guid OperationId { get; set; } = Guid.NewGuid();
    [MaxLength(300)] public string Reason { get; set; } = "";
    public string RefundMethod { get; set; } = "";
    public bool Confirm { get; set; }
    public List<ReturnLineInput> Lines { get; set; } = [];
}
public sealed class ReturnLineInput
{
    public long SaleLineId { get; set; }
    public string Quantity { get; set; } = "0";
    public string RestockQuantity { get; set; } = "0";
}
