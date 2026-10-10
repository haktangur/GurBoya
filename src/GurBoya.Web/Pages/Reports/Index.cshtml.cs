using System.Data;
using GurBoya.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GurBoya.Web.Pages.Reports;

public sealed class IndexModel(AppDbContext database) : PageModel
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
    [BindProperty(SupportsGet = true)] public DateOnly? From { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? To { get; set; }
    [BindProperty(SupportsGet = true)] public int ListPage { get; set; } = 1;
    public List<PaymentRow> Payments { get; private set; } = [];
    public List<ProductRow> Products { get; private set; } = [];
    public bool HasNext { get; private set; }
    public bool Loaded { get; private set; }
    public sealed record PaymentRow(string Method, decimal Sales, decimal Returns);
    public sealed record ProductRow(long Id, string Name, string Unit, long Sold, long Returned);
    public async Task<IActionResult> OnGetAsync(CancellationToken token)
    {
        Response.Headers.CacheControl = "no-store";
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Zone).DateTime);
        From ??= today; To ??= today;
        if (From > To || From < new DateOnly(2000, 1, 1) || To > new DateOnly(9998, 12, 31))
            ModelState.AddModelError("", "Geçerli bir başlangıç ve bitiş tarihi seçin; başlangıç bitişten sonra olamaz.");
        if (!ModelState.IsValid) { Response.StatusCode = 400; return Page(); }
        var start = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(From.Value.ToDateTime(TimeOnly.MinValue), Zone));
        var end = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(To.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), Zone));
        // All sections see the same committed snapshot, including concurrent returns.
        await using var transaction = await database.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, token);
        var sales = database.Sales.Where(s => s.OccurredAt >= start && s.OccurredAt < end);
        var returns = database.SalesReturns.Where(r => r.OccurredAt >= start && r.OccurredAt < end);
        var sold = await sales.GroupBy(s => s.PaymentMethod).Select(g => new { Method = g.Key, Total = g.Sum(s => s.Total) }).ToDictionaryAsync(x => x.Method, x => x.Total, token);
        var refunded = await returns.GroupBy(r => r.RefundMethod).Select(g => new { Method = g.Key, Total = g.Sum(r => r.Total) }).ToDictionaryAsync(x => x.Method, x => x.Total, token);
        Payments = new[] { "CASH", "CARD" }.Select(m => new PaymentRow(m, sold.GetValueOrDefault(m), refunded.GetValueOrDefault(m))).ToList();
        var lines = database.SaleLines.Where(l => l.Sale.OccurredAt >= start && l.Sale.OccurredAt < end);
        var returnLines = database.ReturnLines.Where(l => l.SalesReturn.OccurredAt >= start && l.SalesReturn.OccurredAt < end);
        var ids = lines.Select(l => l.ProductId).Union(returnLines.Select(l => l.SaleLine.ProductId));
        ListPage = Math.Clamp(ListPage, 1, 100000);
        var cards = await database.Products.AsNoTracking().Where(p => ids.Contains(p.Id)).OrderBy(p => p.Id).Skip((ListPage - 1) * 50).Take(51).ToListAsync(token);
        HasNext = cards.Count > 50; cards = cards.Take(50).ToList();
        var pageIds = cards.Select(p => p.Id).ToArray();
        var quantities = await lines.Where(l => pageIds.Contains(l.ProductId)).GroupBy(l => l.ProductId).Select(g => new { Id = g.Key, Quantity = g.Sum(l => l.Quantity) }).ToDictionaryAsync(x => x.Id, x => x.Quantity, token);
        var returned = await returnLines.Where(l => pageIds.Contains(l.SaleLine.ProductId)).GroupBy(l => l.SaleLine.ProductId).Select(g => new { Id = g.Key, Quantity = g.Sum(l => l.Quantity) }).ToDictionaryAsync(x => x.Id, x => x.Quantity, token);
        Products = cards.Select(p => new ProductRow(p.Id, p.Name, p.Unit, quantities.GetValueOrDefault(p.Id), returned.GetValueOrDefault(p.Id))).ToList();
        await transaction.CommitAsync(token);
        Loaded = true;
        return Page();
    }
    public static string UnitName(string unit) => unit switch { "BOX" => "kutu", "GRAM" => "gram", _ => "adet" };
}
