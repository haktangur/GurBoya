using GurBoya.Web.Data;
using GurBoya.Web.Sales;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace GurBoya.Web.Pages.Sales;

public sealed class IndexModel(AppDbContext database) : PageModel
{
    [BindProperty(SupportsGet = true)] public int ListPage { get; set; } = 1;
    public List<Sale> Sales { get; private set; } = [];
    public Dictionary<long, decimal> Returned { get; private set; } = [];
    public bool HasNext { get; private set; }
    public async Task OnGetAsync(CancellationToken token)
    {
        Response.Headers.CacheControl = "no-store";
        ListPage = Math.Clamp(ListPage, 1, 100000);
        Sales = await database.Sales.AsNoTracking().OrderByDescending(s => s.Id).Skip((ListPage - 1) * 50).Take(51).ToListAsync(token);
        HasNext = Sales.Count > 50; Sales = Sales.Take(50).ToList();
        var ids = Sales.Select(s => s.Id).ToArray();
        Returned = await database.SalesReturns.Where(r => ids.Contains(r.SaleId)).GroupBy(r => r.SaleId).Select(g => new { Id = g.Key, Total = g.Sum(r => r.Total) }).ToDictionaryAsync(r => r.Id, r => r.Total, token);
    }
}
