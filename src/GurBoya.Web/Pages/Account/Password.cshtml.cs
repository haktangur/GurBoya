using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GurBoya.Web.Pages.Account;

public sealed class PasswordModel(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn) : PageModel
{
    [BindProperty] public string CurrentPassword { get; set; } = "";
    [BindProperty] public string NewPassword { get; set; } = "";
    [BindProperty] public string ConfirmPassword { get; set; } = "";
    public string? Message { get; private set; }
    public async Task<IActionResult> OnPostAsync()
    {
        if (NewPassword != ConfirmPassword || NewPassword.Length < 10)
        {
            Message = "Yeni şifreler aynı olmalı; en az 10 karakter, harf ve rakam kullanın.";
            Response.StatusCode = 400;
            return Page();
        }
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        var result = await users.ChangePasswordAsync(user, CurrentPassword, NewPassword);
        if (!result.Succeeded)
        {
            Message = "Şifre değiştirilemedi. Mevcut şifreyi ve yeni şifrenin kurallarını kontrol edin.";
            Response.StatusCode = 400;
            return Page();
        }
        await signIn.RefreshSignInAsync(user);
        Message = "Şifreniz değiştirildi.";
        return Page();
    }
}
