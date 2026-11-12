using GestionMagasin.Application;
using GestionMagasin.Domain;
using GestionMagasin.Infrastructure;
using Xunit;

namespace GestionMagasin.Tests;

public sealed class SalesTests
{
    [Fact(DisplayName="T03 - vente espèces et monnaie")]
    public async Task T03_VenteEspeces(){await using var db=await TestDb.CreateAsync();var (admin,product,sales)=await SetupAsync(db,10);var result=await sales.ValiderAsync(admin,[new(product.Id,2,"REF",100)],ModesPaiement.Especes,250,TimeZoneInfo.Local.Id);Assert.Equal(200,result.TotalFcfa);Assert.Equal(50,result.MonnaieRendueFcfa);}
    [Fact(DisplayName="T04 - stock insuffisant refusé")]
    public async Task T04_StockInsuffisant(){await using var db=await TestDb.CreateAsync();var (admin,product,sales)=await SetupAsync(db,1);await Assert.ThrowsAsync<StockInsuffisantException>(()=>sales.ValiderAsync(admin,[new(product.Id,2,"REF",100)],ModesPaiement.Carte,0,TimeZoneInfo.Local.Id));}
    [Fact(DisplayName="T05 - prix relu dans la transaction")]
    public async Task T05_PrixChange(){await using var db=await TestDb.CreateAsync();var (admin,product,sales)=await SetupAsync(db,3);await Assert.ThrowsAsync<PrixProduitModifieException>(()=>sales.ValiderAsync(admin,[new(product.Id,1,"REF",99)],ModesPaiement.Carte,0,TimeZoneInfo.Local.Id));}
    [Fact(DisplayName="T07 - vente conservée après redémarrage")]
    public async Task T07_PersistanceRedemarrage(){await using var db=await TestDb.CreateAsync();var (admin,product,sales)=await SetupAsync(db,3);var result=await sales.ValiderAsync(admin,[new(product.Id,1,"REF",100)],ModesPaiement.MobileMoney,0,TimeZoneInfo.Local.Id);await new SqliteDatabaseInitializer(db.Factory).InitializeAsync();var detail=await new VenteService(new SqliteSaleRepository(db.Factory)).DetailAsync(admin,result.VenteId);Assert.NotNull(detail);Assert.Equal("MOBILE_MONEY",detail!.Vente.ModePaiement);}
    [Fact(DisplayName="T08 - annulation inverse le stock")]
    public async Task T08_Annulation(){await using var db=await TestDb.CreateAsync();var (admin,product,sales)=await SetupAsync(db,3);var result=await sales.ValiderAsync(admin,[new(product.Id,2,"REF",100)],ModesPaiement.Carte,0,TimeZoneInfo.Local.Id);await sales.AnnulerAsync(admin,result.VenteId,"Erreur de saisie");Assert.Equal(3,await new SqliteStockRepository(db.Factory).ObtenirStockAsync(product.Id));Assert.Equal("ANNULEE",(await sales.RechercherAsync(admin,result.Numero,null,null,null,null)).Single().Statut);}
    [Fact(DisplayName="T09 - double annulation refusée")]
    public async Task T09_DoubleAnnulation(){await using var db=await TestDb.CreateAsync();var (admin,product,sales)=await SetupAsync(db,3);var result=await sales.ValiderAsync(admin,[new(product.Id,1,"REF",100)],ModesPaiement.Carte,0,TimeZoneInfo.Local.Id);await sales.AnnulerAsync(admin,result.VenteId,"Test");await Assert.ThrowsAsync<DomainException>(()=>sales.AnnulerAsync(admin,result.VenteId,"Encore"));}
    [Fact(DisplayName="T12 - paiement non espèces au total")]
    public async Task T12_PaiementNonEspeces(){await using var db=await TestDb.CreateAsync();var (admin,product,sales)=await SetupAsync(db,2);var result=await sales.ValiderAsync(admin,[new(product.Id,1,"REF",100)],ModesPaiement.Carte,1,TimeZoneInfo.Local.Id);Assert.Equal(result.TotalFcfa,result.MontantRecuFcfa);}
    [Fact(DisplayName="T13 - vendeur limité à ses ventes")]
    public async Task T13_VendeurVoitSesVentes(){await using var db=await TestDb.CreateAsync();var (admin,product,sales)=await SetupAsync(db,2);var users=new SqliteUserRepository(db.Factory);var seller=new Utilisateur(Guid.NewGuid(),"vendeur","Vendeur",Roles.Vendeur);await users.AjouterAsync(seller,"MotDePasse!1");var result=await sales.ValiderAsync(seller,[new(product.Id,1,"REF",100)],ModesPaiement.Carte,0,TimeZoneInfo.Local.Id);var rows=await sales.RechercherAsync(seller,null,null,null,null,null);Assert.Single(rows);Assert.Equal(result.Numero,rows[0].Numero);}
    [Fact(DisplayName="T16 - transaction atomique sur erreur")]
    public async Task T16_EchecNeLaissePasDeVente(){await using var db=await TestDb.CreateAsync();var (admin,product,sales)=await SetupAsync(db,1);await Assert.ThrowsAsync<StockInsuffisantException>(()=>sales.ValiderAsync(admin,[new(product.Id,2,"REF",100)],ModesPaiement.Carte,0,TimeZoneInfo.Local.Id));Assert.Empty(await sales.RechercherAsync(admin,null,null,null,null,null));Assert.Equal(1,await new SqliteStockRepository(db.Factory).ObtenirStockAsync(product.Id));}

    static async Task<(Utilisateur Admin,Produit Product,VenteService Sales)> SetupAsync(TestDb db,int quantity){var users=new SqliteUserRepository(db.Factory);var admin=await new AuthentificationService(users).CreerPremierAdministrateurAsync("admin","Admin","MotDePasse!1");var products=new SqliteProductRepository(db.Factory);var product=new Produit(Guid.NewGuid(),"REF","Produit",null,100,1,DateTime.UtcNow);await products.AjouterAsync(product);var stock=new StockService(new SqliteStockRepository(db.Factory),products);await stock.EnregistrerAsync(admin,new MouvementStock(Guid.NewGuid(),product.Id,quantity,TypesMouvement.StockInitial,"Initial",admin.Id,DateTime.UtcNow));return(admin,product,new VenteService(new SqliteSaleRepository(db.Factory)));}
}
