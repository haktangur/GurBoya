using System.Globalization;
using GurBoya.Web.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
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
app.MapRazorPages();
app.MapGet("/health/live", () => Results.Json(new { status = "alive", message = "Uygulama çalışıyor." }));
app.MapGet("/health/ready", async (DatabaseStatusService service, CancellationToken token) =>
{
    var status = await service.CheckAsync(token);
    return Results.Json(new { status = status.Code, message = status.Message }, statusCode: status.Ready ? 200 : 503);
});
app.Run();
