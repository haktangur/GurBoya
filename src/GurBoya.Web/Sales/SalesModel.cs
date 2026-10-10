using GurBoya.Web.Inventory;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GurBoya.Web.Sales;

public static class SalesModel
{
    public static void Configure(ModelBuilder model)
    {
        const string amounts = "\"Net\" >= 0 AND \"Vat\" >= 0 AND \"Total\" BETWEEN 0 AND 999999999 AND \"Net\" + \"Vat\" = \"Total\"";
        model.Entity<Sale>(e =>
        {
            e.ToTable("sales", t => { t.HasCheckConstraint("ck_sale_amounts", amounts); t.HasCheckConstraint("ck_sale_payment", "\"PaymentMethod\" IN ('CASH','CARD')"); t.HasCheckConstraint("ck_sale_discount", "\"DiscountKind\" IN ('TL','PERCENT') AND \"DiscountValue\" >= 0 AND (\"DiscountKind\" <> 'PERCENT' OR \"DiscountValue\" <= 100)"); });
            e.HasIndex(x => x.OperationId).IsUnique(); e.HasIndex(x => x.OccurredAt);
            e.HasOne<IdentityUser>().WithMany().HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<SaleLine>(e =>
        {
            e.ToTable("sale_lines", t =>
            {
                t.HasCheckConstraint("ck_sale_line_amounts", amounts);
                t.HasCheckConstraint("ck_sale_line_values", "\"Quantity\" BETWEEN 1 AND 1000000000 AND \"VatRate\" BETWEEN 0 AND 100 AND \"UnitPrice\" BETWEEN 0 AND 999999999 AND \"TintFee\" BETWEEN 0 AND 999999999 AND (\"IsTinted\" OR \"TintFee\" = 0) AND \"Position\" BETWEEN 1 AND 50");
                t.HasCheckConstraint("ck_sale_line_discount", "\"DiscountKind\" IN ('TL','PERCENT') AND \"DiscountValue\" >= 0 AND (\"DiscountKind\" <> 'PERCENT' OR \"DiscountValue\" <= 100) AND \"LineDiscount\" >= 0 AND \"ReceiptDiscount\" >= 0 AND abs(\"Rounding\") < 0.011");
            });
            e.HasIndex(x => new { x.SaleId, x.Position }).IsUnique();
            e.HasOne(x => x.Sale).WithMany(x => x.Lines).HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.PackageLiters).HasPrecision(12, 3);
            e.Property(x => x.LineDiscount).HasPrecision(28, 12); e.Property(x => x.ReceiptDiscount).HasPrecision(28, 12); e.Property(x => x.Rounding).HasPrecision(28, 12);
        });
        model.Entity<SalesReturn>(e =>
        {
            e.ToTable("sales_returns", t => { t.HasCheckConstraint("ck_return_amounts", amounts); t.HasCheckConstraint("ck_return_kind", "\"Kind\" IN ('RETURN','CANCEL') AND \"RefundMethod\" IN ('CASH','CARD') AND length(trim(\"Reason\")) > 0"); });
            e.HasIndex(x => x.OperationId).IsUnique(); e.HasIndex(x => x.OccurredAt);
            e.HasOne(x => x.Sale).WithMany().HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<IdentityUser>().WithMany().HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<ReturnLine>(e =>
        {
            e.ToTable("return_lines", t => { t.HasCheckConstraint("ck_return_line_amounts", amounts); t.HasCheckConstraint("ck_return_quantity", "\"Quantity\" BETWEEN 1 AND 1000000000 AND \"RestockQuantity\" BETWEEN 0 AND \"Quantity\""); });
            e.HasIndex(x => new { x.SalesReturnId, x.SaleLineId }).IsUnique();
            e.HasOne(x => x.SalesReturn).WithMany(x => x.Lines).HasForeignKey(x => x.SalesReturnId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SaleLine).WithMany().HasForeignKey(x => x.SaleLineId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<StockMovement>(e =>
        {
            e.HasIndex(x => x.SaleLineId).IsUnique(); e.HasIndex(x => x.ReturnLineId).IsUnique();
            e.HasOne<SaleLine>().WithMany().HasForeignKey(x => x.SaleLineId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ReturnLine>().WithMany().HasForeignKey(x => x.ReturnLineId).OnDelete(DeleteBehavior.Restrict);
            e.ToTable("stock_movements", t => t.HasCheckConstraint("ck_movement_source", "(\"Kind\" = 'SALE' AND \"SaleLineId\" IS NOT NULL AND \"ReturnLineId\" IS NULL) OR (\"Kind\" = 'RETURN' AND \"ReturnLineId\" IS NOT NULL AND \"SaleLineId\" IS NULL) OR (\"Kind\" NOT IN ('SALE','RETURN') AND \"SaleLineId\" IS NULL AND \"ReturnLineId\" IS NULL)"));
        });
        foreach (var type in new[] { typeof(Sale), typeof(SaleLine), typeof(SalesReturn), typeof(ReturnLine) })
            foreach (var name in new[] { "Net", "Vat", "Total" }) model.Entity(type).Property(name).HasPrecision(18, 2);
    }
}
