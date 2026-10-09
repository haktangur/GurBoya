using System.Security.Claims;
using GurBoya.Web.Data;
using GurBoya.Web.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GurBoya.Web.Pages.Products;

public sealed class EditModel(AppDbContext database, InventoryService service) : PageModel
{
    [BindProperty(SupportsGet = true)] public long? Id { get; set; }
    [BindProperty] public ProductInput Input { get; set; } = new();
    public PriceBreakdown? Purchase { get; private set; }
    public PriceBreakdown? Sale { get; private set; }
    private void Preview()
    {
        var product = new Product();
        Input.Apply(product);
        Purchase = Pricing.Calculate(product.PurchasePrice, product.VatRate, product.VatIncluded);
        Sale = Pricing.Calculate(product.SalePrice, product.VatRate, product.VatIncluded);
    }
    public async Task<IActionResult> OnGetAsync(CancellationToken token)
    {
        if (Id.HasValue)
        {
            var product = await database.Products.AsNoTracking().SingleOrDefaultAsync(product => product.Id == Id, token);
            if (product is null) return NotFound();
            Input = ProductInput.From(product);
            Preview();
        }
        return Page();
    }
    public IActionResult OnPostPreview()
    {
        try { if (ModelState.IsValid) Preview(); }
        catch (InventoryException exception) { ModelState.AddModelError("", exception.Message); }
        if (!ModelState.IsValid) Response.StatusCode = 400;
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(CancellationToken token)
    {
        try
        {
            if (ModelState.IsValid)
            {
                Preview();
                var id = await service.SaveProductAsync(Id, Input, User.FindFirstValue(ClaimTypes.NameIdentifier)!, token);
                return RedirectToPage("Details", new { id });
            }
        }
        catch (InventoryException exception) { ModelState.AddModelError("", exception.Message); }
        catch (DbUpdateConcurrencyException) { ModelState.AddModelError("", "Ürün değişti. Sayfayı yenileyip tekrar deneyin."); }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: "23505" })
        { ModelState.AddModelError("", "Barkod veya işlem kimliği zaten kullanılıyor. Ürün listesini kontrol edin."); }
        Response.StatusCode = 400;
        return Page();
    }
}
