using System.Security.Claims;
using GurBoya.Web.Data;
using GurBoya.Web.Inventory;
using GurBoya.Web.Sales;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GurBoya.Web.Pages.Sales;

public sealed class CreateModel(AppDbContext database, SalesService service) : PageModel
{
    [BindProperty] public SaleInput Input { get; set; } = new();
    [BindProperty] public string Search { get; set; } = "";
    [BindProperty] public long ProductId { get; set; }
    public List<Product> Products { get; private set; } = [];
    public Dictionary<long, Product> CartProducts { get; private set; } = [];
    public Sale? Preview { get; private set; }
    public async Task OnGetAsync(CancellationToken token) => await Load(token);
    private async Task Load(CancellationToken token)
    {
        Response.Headers.CacheControl = "no-store";
        var query = database.Products.AsNoTracking().Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();
            query = query.Where(p => p.Name.Contains(search) || p.Brand.Contains(search) || p.Color.Contains(search) || (p.Barcode != null && p.Barcode.Contains(search)));
        }
        Products = await query.OrderBy(p => p.Name).ThenBy(p => p.Id).Take(50).ToListAsync(token);
        var ids = Input.Lines.Select(line => line.ProductId).Distinct().ToArray();
        CartProducts = await database.Products.AsNoTracking().Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id, token);
    }
    public async Task<IActionResult> OnPostAsync(string? action, CancellationToken token)
    {
        try
        {
            if (!ModelState.IsValid) throw new InventoryException("Alanları kontrol edin.");
            if (Input.Lines.Count > 50) throw new InventoryException("En fazla 50 satış satırı eklenebilir.");
            if (action == "complete")
            {
                var id = await service.Complete(Input, User.FindFirstValue(ClaimTypes.NameIdentifier)!, token);
                return RedirectToPage("Details", new { id });
            }
            if (action == "add")
            {
                if (Input.Lines.Count == 50) throw new InventoryException("En fazla 50 satış satırı eklenebilir.");
                var product = await database.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == ProductId && p.IsActive, token)
                    ?? throw new InventoryException("Eklenecek ürünü seçin.");
                Input.Lines.Add(new() { ProductId = product.Id, Version = product.Version, Color = product.Color });
            }
            else if (action is not null && action.StartsWith("remove:", StringComparison.Ordinal) && int.TryParse(action.AsSpan(7), out var remove) && remove >= 0 && remove < Input.Lines.Count)
                Input.Lines.RemoveAt(remove);
            else if (action is not null && action.StartsWith("tint:", StringComparison.Ordinal) && int.TryParse(action.AsSpan(5), out var tint) && tint >= 0 && tint < Input.Lines.Count)
            {
                if (!await database.Products.AnyAsync(p => p.Id == Input.Lines[tint].ProductId && p.IsPaint, token))
                    throw new InventoryException("Yalnız boya renklendirilebilir.");
                Input.Lines[tint].IsTinted = true;
            }
            ModelState.Clear();
            if (Input.Lines.Count > 0) Preview = await service.Preview(Input, token);
        }
        catch (InventoryException exception) { ModelState.AddModelError("", exception.Message); Response.StatusCode = 400; }
        await Load(token);
        return Page();
    }
}
