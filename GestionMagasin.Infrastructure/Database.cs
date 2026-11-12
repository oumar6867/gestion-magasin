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
            if (migration.Version == 5)
            {
                await transaction.CommitAsync(ct);
                await ExecuteNoTransactionAsync(connection, "PRAGMA foreign_keys = OFF;", ct);
                await using var rebuildTransaction = connection.BeginTransaction();
                try
                {
                    await ExecuteAsync(connection, rebuildTransaction, migration.Sql, ct);
                    await InsertVersionAsync(connection, rebuildTransaction, migration.Version, ct);
                    await rebuildTransaction.CommitAsync(ct);
                }
                finally { await ExecuteNoTransactionAsync(connection, "PRAGMA foreign_keys = ON;", ct); }
                return await InitializeAsync(ct);
            }
            await ExecuteAsync(connection, transaction, migration.Sql, ct);
            await InsertVersionAsync(connection, transaction, migration.Version, ct);
        }
        await transaction.CommitAsync(ct);
    }
    static async Task ExecuteAsync(SqliteConnection c, SqliteTransaction tx, string sql, CancellationToken ct = default) { await using var cmd = c.CreateCommand(); cmd.Transaction = tx; cmd.CommandText = sql; await cmd.ExecuteNonQueryAsync(ct); }
    static async Task ExecuteNoTransactionAsync(SqliteConnection c, string sql, CancellationToken ct) { await using var cmd = c.CreateCommand(); cmd.CommandText = sql; await cmd.ExecuteNonQueryAsync(ct); }
    static async Task InsertVersionAsync(SqliteConnection c, SqliteTransaction tx, int version, CancellationToken ct) { await using var insert = c.CreateCommand(); insert.Transaction = tx; insert.CommandText = "INSERT INTO VersionSchema(Version, AppliqueLeUtc) VALUES($version, $utc)"; insert.Parameters.AddWithValue("$version", version); insert.Parameters.AddWithValue("$utc", DateTime.UtcNow.ToString("O")); await insert.ExecuteNonQueryAsync(ct); }
}

public static class DatabasePaths
{
    public static string Production() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GestionMagasin", "gestion-magasin.db");
    public static string Demonstration() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GestionMagasin", "demo", "gestion-magasin-demo.db");
}
