using GestionMagasin.Application;
using GestionMagasin.Domain;
using GestionMagasin.Infrastructure;
using Xunit;

namespace GestionMagasin.Tests;

public sealed class AdministrationTests
{
    [Fact(DisplayName="T11 - tableau de bord exclut les ventes annulées")]
    public async Task T11_DashboardExclutAnnulee(){await using var db=await TestDb.CreateAsync();var (admin,product,sales)=await SalesTestsSetup(db,3);var result=await sales.ValiderAsync(admin,[new(product.Id,1,"REF",100)],ModesPaiement.Carte,0,TimeZoneInfo.Local.Id);await sales.AnnulerAsync(admin,result.VenteId,"Erreur");var dashboard=await new ReportingService(new SqliteReportingRepository(db.Factory)).DashboardAsync(admin,DateTime.Today,TimeZoneInfo.Local.Id);Assert.Equal(0,dashboard.VentesFcfa);Assert.Equal(0,dashboard.NombreVentes);}
    [Fact(DisplayName="T15 - export CSV protégé et compatible Excel")]
    public async Task T15_CsvProtegeFormules(){await using var db=await TestDb.CreateAsync();var path=Path.Combine(Path.GetTempPath(),"gestion-magasin-tests",Guid.NewGuid()+".csv");try{await CsvExporter.ExportAsync(path,new[]{"Nom"},new[]{new object?[]{"=FORMULE"}});var bytes=await File.ReadAllBytesAsync(path);Assert.Equal(0xEF,bytes[0]);var text=await File.ReadAllTextAsync(path);Assert.Contains("'=FORMULE",text);}finally{if(File.Exists(path))File.Delete(path);}}
    [Fact(DisplayName="T18 - sauvegarde SQLite vérifiable")]
    public async Task T18_SauvegardeVerifiee(){await using var db=await TestDb.CreateAsync();var path=Path.Combine(Path.GetTempPath(),"gestion-magasin-tests",Guid.NewGuid()+".db");try{var result=await new SqliteBackupManager(db.Factory).SauvegarderAsync(path,"MANUELLE",null);Assert.True(result.Succes);Assert.True(File.Exists(path));await new SqliteBackupManager(db.Factory).VerifierAsync(path);}finally{if(File.Exists(path))File.Delete(path);}}

    static async Task<(Utilisateur,Produit,VenteService)> SalesTestsSetup(TestDb db,int quantity){var users=new SqliteUserRepository(db.Factory);var admin=await new AuthentificationService(users).CreerPremierAdministrateurAsync("admin","Admin","MotDePasse!1");var products=new SqliteProductRepository(db.Factory);var product=new Produit(Guid.NewGuid(),"REF","Produit",null,100,1,DateTime.UtcNow);await products.AjouterAsync(product);var stock=new StockService(new SqliteStockRepository(db.Factory),products);await stock.EnregistrerAsync(admin,new MouvementStock(Guid.NewGuid(),product.Id,quantity,TypesMouvement.StockInitial,"Initial",admin.Id,DateTime.UtcNow));return(admin,product,new VenteService(new SqliteSaleRepository(db.Factory)));}
}
