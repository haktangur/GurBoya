using GurBoya.Web.Data;
using GurBoya.Web.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GurBoya.Web.Pages.Products;

public sealed class IndexModel(AppDbContext database) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public bool ShowInactive { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public List<Product> Products { get; private set; } = [];
    public bool HasNext { get; private set; }
    public async Task OnGetAsync(CancellationToken token)
    {
        var query = database.Products.AsNoTracking().Where(product => ShowInactive || product.IsActive);
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = "%" + Search.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
            query = query.Where(product => EF.Functions.ILike(product.Name, search) || EF.Functions.ILike(product.Brand, search)
                || EF.Functions.ILike(product.Color, search) || EF.Functions.ILike(product.Category, search) || EF.Functions.ILike(product.Barcode!, search));
        }
        PageNumber = Math.Clamp(PageNumber, 1, 100000);
        Products = await query.OrderBy(product => product.Name).ThenBy(product => product.Id).Skip((PageNumber - 1) * 50).Take(51).ToListAsync(token);
        HasNext = Products.Count > 50;
        Products = Products.Take(50).ToList();
        Response.Headers.CacheControl = "no-store";
    }
}
