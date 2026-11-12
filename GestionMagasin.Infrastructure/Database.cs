using Microsoft.Data.Sqlite;
using GestionMagasin.Domain;

namespace GestionMagasin.Infrastructure;

public sealed class SqliteConnectionFactory(string databasePath)
{
    public string DatabasePath { get; } = databasePath;
    public SqliteConnection Create()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = DatabasePath, Mode = SqliteOpenMode.ReadWriteCreate, ForeignKeys = true, DefaultTimeout = 5 }.ToString());
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON; PRAGMA journal_mode = WAL; PRAGMA busy_timeout = 5000;";
        command.ExecuteNonQuery();
        return connection;
    }
}

public sealed class SqliteDatabaseInitializer(SqliteConnectionFactory factory) : IDatabaseInitializer
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await using var connection = factory.Create();
        await using var transaction = connection.BeginTransaction();
        await ExecuteAsync(connection, transaction, "CREATE TABLE IF NOT EXISTS VersionSchema (Version INTEGER PRIMARY KEY, AppliqueLeUtc TEXT NOT NULL);");
        foreach (var migration in Migrations.All.OrderBy(x => x.Version))
        {
            await using var check = connection.CreateCommand(); check.Transaction = transaction; check.CommandText = "SELECT COUNT(*) FROM VersionSchema WHERE Version = $version"; check.Parameters.AddWithValue("$version", migration.Version);
            if (Convert.ToInt32(await check.ExecuteScalarAsync(ct)) != 0) continue;
            await ExecuteAsync(connection, transaction, migration.Sql, ct);
            await using var insert = connection.CreateCommand(); insert.Transaction = transaction; insert.CommandText = "INSERT INTO VersionSchema(Version, AppliqueLeUtc) VALUES($version, $utc)"; insert.Parameters.AddWithValue("$version", migration.Version); insert.Parameters.AddWithValue("$utc", DateTime.UtcNow.ToString("O")); await insert.ExecuteNonQueryAsync(ct);
        }
        await transaction.CommitAsync(ct);
    }
    static async Task ExecuteAsync(SqliteConnection c, SqliteTransaction tx, string sql, CancellationToken ct = default) { await using var cmd = c.CreateCommand(); cmd.Transaction = tx; cmd.CommandText = sql; await cmd.ExecuteNonQueryAsync(ct); }
}

public static class DatabasePaths
{
    public static string Production() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GestionMagasin", "gestion-magasin.db");
    public static string Demonstration() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GestionMagasin", "demo", "gestion-magasin-demo.db");
}
