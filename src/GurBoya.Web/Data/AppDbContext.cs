using GurBoya.Web.Inventory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GurBoya.Web.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityUserContext<IdentityUser>(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<PriceChange> PriceChanges => Set<PriceChange>();
    public DbSet<OperationRecord> Operations => Set<OperationRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("app");
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products", table =>
            {
                table.HasCheckConstraint("ck_product_quantity", "\"Quantity\" BETWEEN 0 AND 1000000000");
                table.HasCheckConstraint("ck_product_unit", "\"Unit\" IN ('BOX','PIECE','GRAM')");
                table.HasCheckConstraint("ck_product_paint", "NOT \"IsPaint\" OR (\"Unit\" = 'BOX' AND \"PackageLiters\" IS NOT NULL AND \"PackageLiters\" > 0 AND length(trim(\"Brand\")) > 0 AND length(trim(\"Color\")) > 0)");
                table.HasCheckConstraint("ck_product_amounts", "\"PurchasePrice\" BETWEEN 0 AND 999999999 AND \"SalePrice\" BETWEEN 0 AND 999999999 AND \"VatRate\" BETWEEN 0 AND 100 AND (\"PackageLiters\" IS NULL OR \"PackageLiters\" BETWEEN 0.001 AND 999999)");
                table.HasCheckConstraint("ck_product_name", "length(trim(\"Name\")) > 0");
            });
            entity.HasIndex(product => product.RequestId).IsUnique();
            entity.HasIndex(product => product.Barcode).IsUnique();
            entity.Property(product => product.Version).IsConcurrencyToken();
            entity.Property(product => product.PackageLiters).HasPrecision(12, 3);
        });
        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.ToTable("stock_movements", table =>
            {
                table.HasCheckConstraint("ck_movement_delta", "(\"Delta\" <> 0 OR \"Kind\" = 'COUNT') AND abs(\"Delta\"::numeric) <= 1000000000");
                table.HasCheckConstraint("ck_movement_kind", "(\"Kind\" = 'RECEIPT' AND \"Delta\" > 0) OR \"Kind\" IN ('ADJUSTMENT','COUNT')");
                table.HasCheckConstraint("ck_movement_reason", "length(trim(\"Reason\")) > 0");
            });
            entity.HasIndex(movement => movement.OperationId).IsUnique();
            entity.HasIndex(movement => new { movement.ProductId, movement.OccurredAt });
            entity.HasOne(movement => movement.Product).WithMany().HasForeignKey(movement => movement.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<IdentityUser>().WithMany().HasForeignKey(movement => movement.ActorId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<PriceChange>(entity =>
        {
            entity.ToTable("price_changes");
            entity.HasIndex(change => new { change.ProductId, change.OccurredAt });
            entity.HasOne(change => change.Product).WithMany().HasForeignKey(change => change.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<IdentityUser>().WithMany().HasForeignKey(change => change.ActorId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<OperationRecord>().ToTable("operations");
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties().Where(property => property.ClrType == typeof(decimal)))
            {
                property.SetPrecision(18);
                property.SetScale(4);
            }
        }
    }
}
