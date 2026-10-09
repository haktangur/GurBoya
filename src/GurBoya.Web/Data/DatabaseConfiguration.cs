using Npgsql;

namespace GurBoya.Web.Data;

public static class DatabaseConfiguration
{
    public static string ConnectionString(IConfiguration configuration)
    {
        var password = configuration["Database:Password"];
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Veritabanı parolası yapılandırılmamış.");
        }

        return new NpgsqlConnectionStringBuilder
        {
            Host = configuration["Database:Host"] ?? "db",
            Database = configuration["Database:Name"] ?? "gurboya",
            Username = configuration["Database:User"] ?? "gurboya_app",
            Password = password,
            Timeout = 3,
            CommandTimeout = 5,
            // Local SCRAM deployment has no Kerberos infrastructure.
            GssEncryptionMode = GssEncryptionMode.Disable,
            IncludeErrorDetail = false
        }.ConnectionString;
    }
}
