using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GurBoya.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace GurBoya.Web.Inventory;

public sealed class InventoryService(AppDbContext database)
{
    private static string Hash(object value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value))));

    private async Task<long?> ReplayAsync(Guid id, string hash, CancellationToken token)
    {
        if (id == Guid.Empty) throw new InventoryException("İşlem kimliği eksik; sayfayı yeniden açın.");
        await database.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({id.ToString()}, 0))", token);
        var operation = await database.Operations.AsNoTracking().SingleOrDefaultAsync(operation => operation.Id == id, token);
        if (operation is null) return null;
        if (operation.RequestHash != hash) throw new InventoryException("Bu işlem kimliği başka bilgilerle kullanılmış. Sayfayı yeniden açın.");
        return operation.ResultId;
    }

    private void Remember(Guid id, string hash, long result) => database.Operations.Add(new()
    {
        Id = id,
        RequestHash = hash,
        ResultId = result,
        CreatedAt = DateTimeOffset.UtcNow
    });

    public async Task<long> SaveProductAsync(long? id, ProductInput input, string actor, CancellationToken token)
    {
        var hash = Hash(new { Kind = "product", Id = id, Input = input, Actor = actor });
        await using var transaction = await database.Database.BeginTransactionAsync(token);
        var replay = await ReplayAsync(input.RequestId, hash, token);
        if (replay.HasValue) return replay.Value;
        Product product;
        if (id.HasValue)
        {
            product = await database.Products.FromSqlInterpolated($"SELECT * FROM app.products WHERE \"Id\" = {id.Value} FOR UPDATE").SingleOrDefaultAsync(token)
                ?? throw new InventoryException("Ürün bulunamadı.");
            if (product.Version != input.Version) throw new InventoryException("Ürün başka bir işlemle değişti. Sayfayı yenileyip tekrar deneyin.");
            if (string.IsNullOrWhiteSpace(input.Reason)) throw new InventoryException("Değişiklik açıklaması gerekli.");
            product.Version++;
        }
        else
        {
            product = new() { RequestId = input.RequestId, RequestHash = hash, CreatedAt = DateTimeOffset.UtcNow };
            database.Products.Add(product);
        }
        var oldPrice = new { product.PurchasePrice, product.SalePrice, product.VatRate, product.VatIncluded };
        var oldUnit = new { product.IsPaint, product.Unit, product.PackageLiters };
        input.Apply(product);
        if (id.HasValue && !oldUnit.Equals(new { product.IsPaint, product.Unit, product.PackageLiters })
            && await database.StockMovements.AnyAsync(movement => movement.ProductId == id, token))
            throw new InventoryException("Stok geçmişi olan ürünün türü, birimi veya ambalajı değiştirilemez. Yeni ürün kartı açın.");
        if (product.Barcode is not null && await database.Products.AnyAsync(other => other.Barcode == product.Barcode && other.Id != product.Id, token))
            throw new InventoryException("Bu barkod başka bir üründe kullanılıyor.");
        if (!id.HasValue || !oldPrice.Equals(new { product.PurchasePrice, product.SalePrice, product.VatRate, product.VatIncluded }))
        {
            database.PriceChanges.Add(new()
            {
                Product = product,
                PurchasePrice = product.PurchasePrice,
                SalePrice = product.SalePrice,
                VatRate = product.VatRate,
                VatIncluded = product.VatIncluded,
                Reason = input.Reason?.Trim() ?? "İlk fiyat",
                ActorId = actor,
                OccurredAt = DateTimeOffset.UtcNow
            });
        }
        await database.SaveChangesAsync(token);
        Remember(input.RequestId, hash, product.Id);
        await database.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return product.Id;
    }

    public async Task MoveStockAsync(long id, Guid operationId, string? quantity, string kind, string? reason, string actor, CancellationToken token)
    {
        if (!long.TryParse(quantity, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var amount) || amount < -1000000000 || amount > 1000000000 || (amount == 0 && kind != "COUNT"))
            throw new InventoryException("Miktarı tam sayı girin; sıfır yalnızca sayımda kullanılabilir. Kutu, adet ve gram kesirli olamaz.");
        if (kind is not ("RECEIPT" or "ADJUSTMENT" or "COUNT") || (kind is "RECEIPT" or "COUNT" && amount < 0))
            throw new InventoryException("Stok girişi pozitif olmalı. Azaltmak için gerekçeli düzeltme seçin.");
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 300) throw new InventoryException("1–300 karakterlik işlem açıklaması gerekli.");
        var hash = Hash(new { Kind = kind, Id = id, Amount = amount, Reason = reason.Trim(), Actor = actor });
        await using var transaction = await database.Database.BeginTransactionAsync(token);
        if (await ReplayAsync(operationId, hash, token) is not null) return;
        var product = await database.Products.FromSqlInterpolated($"SELECT * FROM app.products WHERE \"Id\" = {id} FOR UPDATE").SingleOrDefaultAsync(token)
            ?? throw new InventoryException("Ürün bulunamadı.");
        if (!product.IsActive) throw new InventoryException("Bu ürün pasif. Önce ürün kartını etkinleştirin.");
        var delta = kind == "COUNT" ? amount - product.Quantity : amount;
        if (product.Quantity + delta > 1000000000) throw new InventoryException("Stok miktarı en fazla 1.000.000.000 olabilir.");
        if (product.Quantity + delta < 0) throw new InventoryException("Stokta yok veya miktar yetersiz. Stok ekledikten sonra yeniden deneyin.");
        database.StockMovements.Add(new()
        {
            ProductId = id,
            OperationId = operationId,
            RequestHash = hash,
            Delta = delta,
            Kind = kind,
            Reason = reason.Trim(),
            PurchaseNet = Pricing.Calculate(product.PurchasePrice, product.VatRate, product.VatIncluded).Net,
            ActorId = actor,
            OccurredAt = DateTimeOffset.UtcNow
        });
        Remember(operationId, hash, id);
        // Database trigger locks the product and adjusts its balance in this transaction.
        await database.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
    }
}
