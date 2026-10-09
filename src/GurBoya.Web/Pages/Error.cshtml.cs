using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GurBoya.Web.Pages;

public sealed class ErrorModel : PageModel
{
    public string Message { get; private set; } = "Beklenmeyen bir sorun oluştu. İşlem sonucunu kontrol edip yeniden deneyin.";
    public void OnGet() => SetMessage();
    public void OnPost() => SetMessage();
    private void SetMessage()
    {
        var error = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (error is NpgsqlException or DbUpdateException or TimeoutException)
        {
            Response.StatusCode = 503;
            Message = "Veritabanı işlemi tamamlanamadı. Kayıtları kontrol edin; aynı formu yeniden göndererek güvenle tekrar deneyebilirsiniz.";
        }
        Response.Headers.CacheControl = "no-store";
    }
}
