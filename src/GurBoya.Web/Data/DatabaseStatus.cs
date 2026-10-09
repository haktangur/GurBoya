using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GurBoya.Web.Data;

public sealed record DatabaseStatus(bool Ready, string Code, string Message);

public sealed class DatabaseStatusService(AppDbContext database, ILogger<DatabaseStatusService> logger)
{
    public async Task<DatabaseStatus> CheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            var pending = await database.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pending.Any())
            {
                return new(false, "migration_required", "Veritabanı güncellemesi gerekiyor. Kurulum adımını tamamlayın.");
            }

            return new(true, "ready", "Veritabanı bağlantısı hazır.");
        }
        catch (Exception exception) when (exception is NpgsqlException or TimeoutException)
        {
            // Deliberately omit exception text, SQL and credentials from diagnostics.
            logger.LogWarning("Veritabanı bağlantısı kurulamadı.");
            return new(false, "database_unavailable", "Veritabanına ulaşılamıyor. Bağlantıyı kontrol edip yeniden deneyin.");
        }
    }
}
