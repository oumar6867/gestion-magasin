using GestionMagasin.Infrastructure;
using Microsoft.Data.Sqlite;
using Xunit;

namespace GestionMagasin.Tests;

public sealed class DatabaseTests
{
    [Fact]
    public async Task Base_vierge_contient_le_schema_initial()
    {
        var path = Path.Combine(Path.GetTempPath(), "gestion-magasin-tests", Guid.NewGuid()+".db");
        try
        {
            var factory = new SqliteConnectionFactory(path); await new SqliteDatabaseInitializer(factory).InitializeAsync();
            await using var c = factory.Create(); await using var cmd = c.CreateCommand(); cmd.CommandText = "SELECT name FROM sqlite_master WHERE type IN ('table','view')"; await using var r = await cmd.ExecuteReaderAsync(); var names = new List<string>(); while (await r.ReadAsync()) names.Add(r.GetString(0));
            Assert.Contains("VersionSchema", names); Assert.Contains("Produit", names); Assert.Contains("MouvementStock", names); Assert.Contains("StockCourant", names);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public async Task Migrations_rejouables_sans_perte_de_donnees()
    {
        var path = Path.Combine(Path.GetTempPath(), "gestion-magasin-tests", Guid.NewGuid()+".db");
        try
        {
            var factory = new SqliteConnectionFactory(path); await new SqliteDatabaseInitializer(factory).InitializeAsync();
            await using (var c = factory.Create()) { await using var cmd = c.CreateCommand(); cmd.CommandText = "INSERT INTO Parametre(Cle,Valeur,ModifieLeUtc) VALUES('test','conserve',$now)"; cmd.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); await cmd.ExecuteNonQueryAsync(); }
            await new SqliteDatabaseInitializer(factory).InitializeAsync();
            await using var check = factory.Create(); await using var q = check.CreateCommand(); q.CommandText = "SELECT Valeur FROM Parametre WHERE Cle='test'; SELECT COUNT(*) FROM VersionSchema"; await using var rr = await q.ExecuteReaderAsync(); Assert.True(await rr.ReadAsync()); Assert.Equal("conserve", rr.GetString(0)); Assert.True(await rr.NextResultAsync()); Assert.True(await rr.ReadAsync()); Assert.Equal(1, rr.GetInt32(0));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public async Task Cles_etrangeres_sont_activees_sur_chaque_connexion()
    {
        var path = Path.Combine(Path.GetTempPath(), "gestion-magasin-tests", Guid.NewGuid()+".db"); try { var f=new SqliteConnectionFactory(path); await new SqliteDatabaseInitializer(f).InitializeAsync(); await using var c=f.Create(); await using var cmd=c.CreateCommand(); cmd.CommandText="PRAGMA foreign_keys"; Assert.Equal(1L, await cmd.ExecuteScalarAsync()); } finally { if(File.Exists(path)) File.Delete(path); }
    }
}
