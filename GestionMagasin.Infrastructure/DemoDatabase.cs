using Microsoft.Data.Sqlite;
using GestionMagasin.Domain;

namespace GestionMagasin.Infrastructure;

public static class DemoDatabase
{
    public static async Task CreateAsync(CancellationToken ct = default)
    {
        var factory = new SqliteConnectionFactory(DatabasePaths.Demonstration());
        await new SqliteDatabaseInitializer(factory).InitializeAsync(ct);
        await using var c = factory.Create();
        await using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT OR IGNORE INTO Parametre(Cle,Valeur,ModifieLeUtc) VALUES('mode','DEMONSTRATION',$now); INSERT OR IGNORE INTO Categorie(Id,Nom,Active,CreeLeUtc,ModifieLeUtc) VALUES($id,'Général',1,$now,$now);";
        cmd.Parameters.AddWithValue("$id", Guid.NewGuid().ToString()); cmd.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
