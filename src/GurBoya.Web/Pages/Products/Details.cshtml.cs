using System.Security.Claims;
using GurBoya.Web.Data;
using GurBoya.Web.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GurBoya.Web.Pages.Products;

public sealed class DetailsModel(AppDbContext database, InventoryService service) : PageModel
{
    public Product Product { get; private set; } = null!;
    public List<StockMovement> Movements { get; private set; } = [];
    public List<PriceChange> Prices { get; private set; } = [];
    [BindProperty] public Guid OperationId { get; set; } = Guid.NewGuid();
    [BindProperty] public string Quantity { get; set; } = "";
    [BindProperty] public string Kind { get; set; } = "RECEIPT";
    [BindProperty] public string Reason { get; set; } = "";
    [BindProperty(SupportsGet = true)] public int HistoryPage { get; set; } = 1;
    public bool HasNextHistory { get; private set; }
    public static string LocalTime(DateTimeOffset time) => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(time, "Europe/Istanbul").ToString("dd.MM.yyyy HH:mm");
    private async Task<bool> LoadAsync(long id, CancellationToken token)
    {
        var product = await database.Products.AsNoTracking().SingleOrDefaultAsync(product => product.Id == id, token);
        if (product is null) return false;
        Product = product;
        HistoryPage = Math.Clamp(HistoryPage, 1, 100000);
        var skip = (HistoryPage - 1) * 50;
        Movements = await database.StockMovements.AsNoTracking().Where(movement => movement.ProductId == id).OrderByDescending(movement => movement.Id).Skip(skip).Take(51).ToListAsync(token);
        Prices = await database.PriceChanges.AsNoTracking().Where(change => change.ProductId == id).OrderByDescending(change => change.Id).Skip(skip).Take(51).ToListAsync(token);
        HasNextHistory = Movements.Count > 50 || Prices.Count > 50;
        Movements = Movements.Take(50).ToList(); Prices = Prices.Take(50).ToList();
        Response.Headers.CacheControl = "no-store";
        return true;
    }
    public async Task<IActionResult> OnGetAsync(long id, CancellationToken token) => await LoadAsync(id, token) ? Page() : NotFound();
    public async Task<IActionResult> OnPostAsync(long id, CancellationToken token)
    {
        try
        {
            if (ModelState.IsValid)
            {
                await service.MoveStockAsync(id, OperationId, Quantity, Kind, Reason, User.FindFirstValue(ClaimTypes.NameIdentifier)!, token);
                return RedirectToPage(new { id });
            }
        }
        catch (InventoryException exception) { ModelState.AddModelError("", exception.Message); }
        Response.StatusCode = 400;
        return await LoadAsync(id, token) ? Page() : NotFound();
    }
}
