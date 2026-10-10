using System.Globalization;
using GurBoya.Web.Data;
using GurBoya.Web.Inventory;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AllowAnonymousToPage("/Status");
    options.Conventions.AllowAnonymousToPage("/Index");
}).AddMvcOptions(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    var messages = options.ModelBindingMessageProvider;
    messages.SetAttemptedValueIsInvalidAccessor((value, field) => "Geçersiz değer. Alanı kontrol edin.");
    messages.SetValueMustNotBeNullAccessor(value => "Bu alanı doldurun.");
    messages.SetValueIsInvalidAccessor(value => "Geçersiz değer.");
    messages.SetValueMustBeANumberAccessor(field => "Geçerli bir sayı girin.");
    messages.SetUnknownValueIsInvalidAccessor(field => "Geçersiz değer.");
    messages.SetMissingBindRequiredValueAccessor(field => "Bu alanı doldurun.");
    messages.SetMissingKeyOrValueAccessor(() => "Gerekli bilgi eksik.");
    messages.SetNonPropertyAttemptedValueIsInvalidAccessor(value => "Geçersiz değer.");
    messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Geçersiz değer.");
    messages.SetNonPropertyValueMustBeANumberAccessor(() => "Geçerli bir sayı girin.");
});
builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.Password.RequiredLength = 10;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
}).AddEntityFrameworkStores<AppDbContext>().AddSignInManager().AddDefaultTokenProviders();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.Cookie.Name = "GurBoya.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});
builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.FromMinutes(1));
var protection = builder.Services.AddDataProtection().SetApplicationName("GurBoya");
if (builder.Configuration["DataProtection:Path"] is { } keyPath)
    protection.PersistKeysToFileSystem(new DirectoryInfo(keyPath));
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<GurBoya.Web.Sales.SalesService>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(
    DatabaseConfiguration.ConnectionString(builder.Configuration),
    postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "infrastructure")));
builder.Services.AddScoped<DatabaseStatusService>();

var app = builder.Build();

// Explicit maintenance command; normal web startup never changes the schema.
if (args.Contains("--migrate", StringComparer.Ordinal))
{
    try
    {
        using var scope = app.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await database.Database.MigrateAsync();
        Console.WriteLine("Veritabanı güncellemesi tamamlandı.");
    }
    catch
    {
        Console.Error.WriteLine("Veritabanı güncellenemedi. Bağlantı ve migration hesabının yetkilerini kontrol edin.");
        Environment.ExitCode = 1;
    }
    return;
}

if (args.Contains("--create-admin", StringComparer.Ordinal))
{
    try
    {
        using var scope = app.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        if (await users.FindByNameAsync("yonetici") is not null)
            Console.WriteLine("Yönetici hesabı zaten var; şifre değiştirilmedi.");
        else
        {
            var password = builder.Configuration["Admin:Password"] ?? "";
            var result = await users.CreateAsync(new IdentityUser("yonetici") { LockoutEnabled = true }, password);
            if (!result.Succeeded) throw new InvalidOperationException();
            Console.WriteLine("Yönetici hesabı oluşturuldu.");
        }
    }
    catch
    {
        Console.Error.WriteLine("Yönetici oluşturulamadı. Migration ve şifre ayarını kontrol edin; en az 10 karakter, harf ve rakam gerekir.");
        Environment.ExitCode = 1;
    }
    return;
}

var culture = CultureInfo.GetCultureInfo("tr-TR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new(culture),
    SupportedCultures = [culture],
    SupportedUICultures = [culture]
});
app.UseExceptionHandler("/Error");
app.UseStatusCodePagesWithReExecute("/Status", "?code={0}");
app.Use(async (context, next) =>
{
    context.Response.Headers.ContentSecurityPolicy = "default-src 'self'; style-src 'self'; img-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapGet("/health/live", () => Results.Json(new { status = "alive", message = "Uygulama çalışıyor." }));
app.MapGet("/health/ready", async (DatabaseStatusService service, CancellationToken token) =>
{
    var status = await service.CheckAsync(token);
    return Results.Json(new { status = status.Code, message = status.Message }, statusCode: status.Ready ? 200 : 503);
});
app.Run();
