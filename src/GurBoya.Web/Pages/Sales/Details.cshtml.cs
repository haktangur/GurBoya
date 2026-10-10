using System.Security.Claims;
using GurBoya.Web.Data;
using GurBoya.Web.Inventory;
using GurBoya.Web.Sales;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace GurBoya.Web.Pages.Sales;

public sealed class DetailsModel(AppDbContext database, SalesService service) : PageModel
{
    public Sale Sale { get; private set; } = null!;
    public List<SalesReturn> Returns { get; private set; } = [];
    [BindProperty] public ReturnInput Input { get; set; } = new();
    public long Remaining(SaleLine line) => line.Quantity - Returns.SelectMany(r => r.Lines).Where(r => r.SaleLineId == line.Id).Sum(r => r.Quantity);
    private async Task<bool> Load(long id, CancellationToken token)
    {
        Response.Headers.CacheControl = "no-store";
        var sale = await database.Sales.AsNoTracking().Include(s => s.Lines).SingleOrDefaultAsync(s => s.Id == id, token);
        if (sale is null) return false;
        Sale = sale; Sale.Lines = sale.Lines.OrderBy(l => l.Position).ToList();
        Returns = await database.SalesReturns.AsNoTracking().Include(r => r.Lines).Where(r => r.SaleId == id).OrderByDescending(r => r.Id).ToListAsync(token);
        return true;
    }
    public async Task<IActionResult> OnGetAsync(long id, CancellationToken token)
    {
        if (!await Load(id, token)) return NotFound();
        Input.RefundMethod = Sale.PaymentMethod;
        Input.Lines = Sale.Lines.Select(l => new ReturnLineInput { SaleLineId = l.Id }).ToList();
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(long id, string action, CancellationToken token)
    {
        try
        {
            if (ModelState.IsValid)
            {
                if (action is not ("return" or "cancel")) throw new InventoryException("Geçersiz işlem.");
                await service.Return(id, Input, action == "cancel", User.FindFirstValue(ClaimTypes.NameIdentifier)!, token);
                return RedirectToPage(new { id });
            }
        }
        catch (InventoryException exception) { ModelState.AddModelError("", exception.Message); }
        Response.StatusCode = 400;
        return await Load(id, token) ? Page() : NotFound();
    }
}
