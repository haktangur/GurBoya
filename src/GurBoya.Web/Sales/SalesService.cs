using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GurBoya.Web.Data;
using GurBoya.Web.Inventory;
using Microsoft.EntityFrameworkCore;

namespace GurBoya.Web.Sales;

public sealed class SalesService(AppDbContext database)
{
    private static string Hash(object value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value))));
    private async Task<long?> Replay(Guid id, string hash, CancellationToken token)
    {
        if (id == Guid.Empty) throw new InventoryException("İşlem kimliği eksik. Sayfayı yeniden açın.");
        await database.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({id.ToString()}, 0))", token);
        var previous = await database.Operations.AsNoTracking().SingleOrDefaultAsync(row => row.Id == id, token);
        if (previous is null) return null;
        if (previous.RequestHash != hash) throw new InventoryException("İşlem kimliği farklı bilgilerle kullanılmış. Sayfayı yeniden açın.");
        return previous.ResultId;
    }
    private void Remember(Guid id, string hash, long result) => database.Operations.Add(new()
    { Id = id, RequestHash = hash, ResultId = result, CreatedAt = DateTimeOffset.UtcNow });
    private static void Payment(string method)
    {
        if (method is not ("CASH" or "CARD")) throw new InventoryException("Nakit veya kart seçin.");
    }
    private async Task<Dictionary<long, Product>> Products(IEnumerable<long> ids, bool locked, CancellationToken token)
    {
        var result = new Dictionary<long, Product>();
        foreach (var id in ids.Distinct().Order())
        {
            var product = locked
                ? await database.Products.FromSqlInterpolated($"SELECT * FROM app.products WHERE \"Id\" = {id} FOR UPDATE").AsNoTracking().SingleOrDefaultAsync(token)
                : await database.Products.AsNoTracking().SingleOrDefaultAsync(row => row.Id == id, token);
            if (product is null) throw new InventoryException("Ürün bulunamadı.");
            result.Add(id, product);
        }
        return result;
    }
    public async Task<Sale> Preview(SaleInput input, CancellationToken token)
    {
        if (input.Lines.Count > 50) throw new InventoryException("En fazla 50 satış satırı eklenebilir.");
        return SalePricing.Calculate(input, await Products(input.Lines.Select(line => line.ProductId), false, token));
    }
    public async Task<long> Complete(SaleInput input, string actor, CancellationToken token)
    {
        if (input.Lines.Count > 50) throw new InventoryException("En fazla 50 satış satırı eklenebilir.");
        var hash = Hash(new { Kind = "sale", Input = input, Actor = actor });
        await using var transaction = await database.Database.BeginTransactionAsync(token);
        if (await Replay(input.OperationId, hash, token) is { } replay) return replay;
        Payment(input.PaymentMethod);
        var products = await Products(input.Lines.Select(line => line.ProductId), true, token);
        var sale = SalePricing.Calculate(input, products);
        sale.ActorId = actor; sale.OccurredAt = DateTimeOffset.UtcNow;
        database.Sales.Add(sale);
        await database.SaveChangesAsync(token);
        foreach (var line in sale.Lines.OrderBy(line => line.ProductId).ThenBy(line => line.Position))
            database.StockMovements.Add(Movement(products[line.ProductId], -line.Quantity, actor, hash, $"Satış #{sale.Id}", line.Id, null));
        Remember(input.OperationId, hash, sale.Id);
        await database.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return sale.Id;
    }
    private static StockMovement Movement(Product product, long delta, string actor, string hash, string reason, long? saleLine, long? returnLine) => new()
    {
        ProductId = product.Id,
        OperationId = Guid.NewGuid(),
        RequestHash = hash,
        Delta = delta,
        Kind = saleLine.HasValue ? "SALE" : "RETURN",
        Reason = reason,
        SaleLineId = saleLine,
        ReturnLineId = returnLine,
        PurchaseNet = Pricing.Calculate(product.PurchasePrice, product.VatRate, product.VatIncluded).Net,
        ActorId = actor,
        OccurredAt = DateTimeOffset.UtcNow
    };
    public async Task<long> Return(long saleId, ReturnInput input, bool cancel, string actor, CancellationToken token)
    {
        if (!input.Confirm) throw new InventoryException("İade/iptal onayını işaretleyin.");
        if (string.IsNullOrWhiteSpace(input.Reason) || input.Reason.Trim().Length > 300)
            throw new InventoryException("1–300 karakterlik iade/iptal açıklaması gerekli.");
        if (input.Lines.Count > 50) throw new InventoryException("En fazla 50 iade satırı olabilir.");
        var hash = Hash(new { Kind = cancel ? "cancel" : "return", SaleId = saleId, Input = input, Actor = actor });
        await using var transaction = await database.Database.BeginTransactionAsync(token);
        if (await Replay(input.OperationId, hash, token) is { } replay) return replay;
        Payment(input.RefundMethod);
        var sale = await database.Sales.FromSqlInterpolated($"SELECT * FROM app.sales WHERE \"Id\" = {saleId} FOR UPDATE").AsNoTracking().SingleOrDefaultAsync(token)
            ?? throw new InventoryException("Satış bulunamadı.");
        var lines = await database.SaleLines.AsNoTracking().Where(line => line.SaleId == sale.Id).OrderBy(line => line.Id).ToListAsync(token);
        var previous = await database.ReturnLines.AsNoTracking().Where(line => line.SalesReturn.SaleId == saleId).ToListAsync(token);
        if (input.Lines.Select(line => line.SaleLineId).Distinct().Count() != input.Lines.Count)
            throw new InventoryException("Aynı satış satırı iadede tekrar edemez.");
        var requested = cancel ? lines.Select(line => new ReturnLineInput
        {
            SaleLineId = line.Id,
            Quantity = (line.Quantity - previous.Where(p => p.SaleLineId == line.Id).Sum(p => p.Quantity)).ToString(),
            RestockQuantity = (line.Quantity - previous.Where(p => p.SaleLineId == line.Id).Sum(p => p.Quantity)).ToString()
        }).ToList() : input.Lines;
        var document = new SalesReturn
        {
            SaleId = sale.Id,
            OperationId = input.OperationId,
            ActorId = actor,
            OccurredAt = DateTimeOffset.UtcNow,
            Reason = input.Reason.Trim(),
            Kind = cancel ? "CANCEL" : "RETURN",
            RefundMethod = input.RefundMethod
        };
        foreach (var item in requested)
        {
            var source = lines.SingleOrDefault(line => line.Id == item.SaleLineId) ?? throw new InventoryException("İade satırı bu satışa ait değil.");
            var quantity = SalePricing.Quantity(item.Quantity, true);
            var restock = SalePricing.Quantity(item.RestockQuantity, true);
            if (restock > quantity) throw new InventoryException("Stoğa dönüş miktarı iade miktarını aşamaz.");
            if (quantity == 0) continue;
            if (source.IsTinted) throw new InventoryException("Renklendirilmiş boya iade veya iptal edilemez.");
            var returned = previous.Where(line => line.SaleLineId == source.Id).ToList();
            var totalQuantity = quantity + returned.Sum(line => line.Quantity);
            if (totalQuantity > source.Quantity) throw new InventoryException("İade miktarı kalan satış miktarını aşamaz.");
            var refund = Math.Min(source.Total, SalePricing.Up(source.Total * totalQuantity / source.Quantity)) - returned.Sum(line => line.Total);
            var remainingNet = source.Net - returned.Sum(line => line.Net);
            var remainingVat = source.Vat - returned.Sum(line => line.Vat);
            var idealNet = source.Total == 0 ? 0 : decimal.Round(refund * source.Net / source.Total, 2, MidpointRounding.AwayFromZero);
            var net = Math.Clamp(idealNet, Math.Max(0, refund - remainingVat), Math.Min(refund, remainingNet));
            document.Lines.Add(new() { SaleLineId = source.Id, Quantity = quantity, RestockQuantity = restock, Total = refund, Net = net, Vat = refund - net });
        }
        if (document.Lines.Count == 0) throw new InventoryException("İade edilecek miktar yok.");
        var products = await Products(document.Lines.Where(line => line.RestockQuantity > 0).Select(line => lines.Single(source => source.Id == line.SaleLineId).ProductId), true, token);
        foreach (var group in document.Lines.Where(line => line.RestockQuantity > 0).GroupBy(line => lines.Single(source => source.Id == line.SaleLineId).ProductId))
            if (products[group.Key].Quantity + group.Sum(line => line.RestockQuantity) > 1000000000)
                throw new InventoryException("İade stok üst sınırını aşar.");
        document.Net = document.Lines.Sum(line => line.Net); document.Vat = document.Lines.Sum(line => line.Vat); document.Total = document.Lines.Sum(line => line.Total);
        database.SalesReturns.Add(document);
        await database.SaveChangesAsync(token);
        foreach (var line in document.Lines.Where(line => line.RestockQuantity > 0).OrderBy(line => lines.Single(source => source.Id == line.SaleLineId).ProductId))
            database.StockMovements.Add(Movement(products[lines.Single(source => source.Id == line.SaleLineId).ProductId], line.RestockQuantity, actor, hash, $"İade #{document.Id} / Satış #{saleId}", null, line.Id));
        Remember(input.OperationId, hash, document.Id);
        await database.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return document.Id;
    }
}
