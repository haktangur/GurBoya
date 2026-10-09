using GurBoya.Web.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GurBoya.Web.Pages;

public sealed class IndexModel(DatabaseStatusService service) : PageModel
{
    public DatabaseStatus Status { get; private set; } = new(false, "unknown", "Bağlantı kontrol ediliyor.");

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Status = await service.CheckAsync(cancellationToken);
        Response.StatusCode = Status.Ready ? 200 : 503;
        Response.Headers.CacheControl = "no-store";
    }
}
