using GurBoya.Web.Data;
using GurBoya.Web.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GurBoya.Web.Pages.Reports;

public sealed class StockModel(AppDbContext database) : PageModel
{
    [BindProperty(SupportsGet = true)] public bool OnlyEmpty { get; set; }
    [BindProperty(SupportsGet = true)] public bool ShowInactive { get; set; }
    [BindProperty(SupportsGet = true)] public int ListPage { get; set; } = 1;
    public List<Product> Products { get; private set; } = [];
    public bool HasNext { get; private set; }
    public async Task OnGetAsync(CancellationToken token)
    {
        Response.Headers.CacheControl = "no-store";
        ListPage = Math.Clamp(ListPage, 1, 100000);
        Products = await database.Products.AsNoTracking().Where(p => (ShowInactive || p.IsActive) && (!OnlyEmpty || p.Quantity == 0))
            .OrderBy(p => p.Name).ThenBy(p => p.Id).Skip((ListPage - 1) * 50).Take(51).ToListAsync(token);
        HasNext = Products.Count > 50; Products = Products.Take(50).ToList();
    }
}
