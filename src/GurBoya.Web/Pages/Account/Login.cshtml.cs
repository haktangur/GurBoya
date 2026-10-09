using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;

namespace GurBoya.Web.Pages.Account;

public sealed class LoginModel(SignInManager<IdentityUser> signIn) : PageModel
{
    [BindProperty] public string Password { get; set; } = "";
    public string? Error { get; private set; }
    public async Task<IActionResult> OnPostAsync(string? returnUrl)
    {
        Response.Headers.CacheControl = "no-store";
        try
        {
            if (!string.IsNullOrEmpty(Password))
            {
                var result = await signIn.PasswordSignInAsync("yonetici", Password, false, true);
                if (result.Succeeded) return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/Products");
            }
            Error = "Şifre hatalı veya hesap geçici olarak kilitli. Beş hatalı denemeden sonra 5 dakika bekleyin.";
            Response.StatusCode = 400;
        }
        catch (Exception exception) when (exception is NpgsqlException or TimeoutException)
        {
            Error = "Veritabanına ulaşılamıyor. Biraz sonra yeniden deneyin.";
            Response.StatusCode = 503;
        }
        ModelState.Remove(nameof(Password));
        Password = "";
        return Page();
    }
}
