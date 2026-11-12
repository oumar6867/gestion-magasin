using GestionMagasin.Application;
using GestionMagasin.Domain;
using GestionMagasin.Infrastructure;
using Xunit;

namespace GestionMagasin.Tests;

public sealed class BusinessRulesTests
{
    [Fact(DisplayName="T01 - premier administrateur sans mot de passe par défaut")]
    public async Task T01_PremierAdministrateurEtAuthentification()
    {
        await using var db=await TestDb.CreateAsync(); var users=new SqliteUserRepository(db.Factory); var auth=new AuthentificationService(users);
        var admin=await auth.CreerPremierAdministrateurAsync("admin","Administrateur","MotDePasse!1");
        Assert.Equal(Roles.Administrateur,admin.Role); Assert.NotNull(await auth.ConnexionAsync("admin","MotDePasse!1")); Assert.Null(await auth.ConnexionAsync("admin","wrong"));
        Assert.False(await auth.PremierDemarrageAsync());
    }

    [Fact(DisplayName="T02 - dernier administrateur actif protégé")]
    public async Task T02_DernierAdminNePeutPasEtreDesactive()
    {
        await using var db=await TestDb.CreateAsync(); var users=new SqliteUserRepository(db.Factory); var auth=new AuthentificationService(users); var admin=await auth.CreerPremierAdministrateurAsync("admin","Admin","MotDePasse!1");
        await Assert.ThrowsAsync<DomainException>(()=>new UtilisateurService(users).DesactiverAsync(admin,admin));
    }

    [Fact(DisplayName="T06 - prix strictement positif")]
    public async Task T06_PrixProduitPositif()
    {
        await using var db=await TestDb.CreateAsync(); var repo=new SqliteProductRepository(db.Factory); var service=new ProduitService(repo);
        await Assert.ThrowsAsync<DomainException>(()=>service.CreerAsync(new Produit(Guid.NewGuid(),"REF","Produit",null,0,0,DateTime.UtcNow)));
    }

    [Fact(DisplayName="T10 - stock initial unique")]
    public async Task T10_StockInitialUnique()
    {
        await using var db=await TestDb.CreateAsync(); var users=new SqliteUserRepository(db.Factory); var admin=await new AuthentificationService(users).CreerPremierAdministrateurAsync("admin","Admin","MotDePasse!1"); var products=new SqliteProductRepository(db.Factory); var product=new Produit(Guid.NewGuid(),"REF","Produit",null,100,2,DateTime.UtcNow); await products.AjouterAsync(product); var stock=new StockService(new SqliteStockRepository(db.Factory),products);
        var movement=new MouvementStock(Guid.NewGuid(),product.Id,5,TypesMouvement.StockInitial,"Inventaire initial",admin.Id,DateTime.UtcNow); await stock.EnregistrerAsync(admin,movement); await Assert.ThrowsAsync<DomainException>(()=>stock.EnregistrerAsync(admin,movement with {Id=Guid.NewGuid()}));
    }

    [Fact(DisplayName="T14 - stock négatif interdit")]
    public async Task T14_StockNegatifInterdit()
    {
        await using var db=await TestDb.CreateAsync(); var users=new SqliteUserRepository(db.Factory); var admin=await new AuthentificationService(users).CreerPremierAdministrateurAsync("admin","Admin","MotDePasse!1"); var products=new SqliteProductRepository(db.Factory); var product=new Produit(Guid.NewGuid(),"REF","Produit",null,100,2,DateTime.UtcNow); await products.AjouterAsync(product); var stock=new StockService(new SqliteStockRepository(db.Factory),products);
        await Assert.ThrowsAsync<DomainException>(()=>stock.EnregistrerAsync(admin,new MouvementStock(Guid.NewGuid(),product.Id,-1,TypesMouvement.Correction,"Test",admin.Id,DateTime.UtcNow,0,0)));
    }
}

internal sealed class TestDb : IAsyncDisposable
{
    public SqliteConnectionFactory Factory {get;} private readonly string path; private TestDb(string path){this.path=path;Factory=new SqliteConnectionFactory(path);}
    public static async Task<TestDb> CreateAsync(){var path=Path.Combine(Path.GetTempPath(),"gestion-magasin-tests",Guid.NewGuid()+".db");var db=new TestDb(path);await new SqliteDatabaseInitializer(db.Factory).InitializeAsync();return db;}
    public ValueTask DisposeAsync(){if(File.Exists(path))File.Delete(path);return ValueTask.CompletedTask;}
}
