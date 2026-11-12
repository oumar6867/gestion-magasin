namespace GestionMagasin.Infrastructure;

internal sealed record Migration(int Version, string Sql);
internal static class Migrations
{
    public static IReadOnlyList<Migration> All { get; } = new[] { new Migration(1, InitialSql), new Migration(2, SecurityAndStockSql), new Migration(3, SalesSql), new Migration(4, AdministrationSql) };
    private const string InitialSql = """
CREATE TABLE Utilisateur (Id TEXT PRIMARY KEY, NomUtilisateur TEXT NOT NULL COLLATE NOCASE UNIQUE, NomComplet TEXT NOT NULL, Role TEXT NOT NULL CHECK (Role IN ('ADMIN','VENDEUR')), Actif INTEGER NOT NULL DEFAULT 1 CHECK (Actif IN (0,1)), CreeLeUtc TEXT NOT NULL, ModifieLeUtc TEXT NOT NULL);
CREATE TABLE Categorie (Id TEXT PRIMARY KEY, Nom TEXT NOT NULL COLLATE NOCASE UNIQUE, Active INTEGER NOT NULL DEFAULT 1 CHECK (Active IN (0,1)), CreeLeUtc TEXT NOT NULL, ModifieLeUtc TEXT NOT NULL);
CREATE TABLE Produit (Id TEXT PRIMARY KEY, Reference TEXT NOT NULL COLLATE NOCASE UNIQUE, Nom TEXT NOT NULL, CategorieId TEXT REFERENCES Categorie(Id), PrixVenteFcfa INTEGER NOT NULL CHECK (PrixVenteFcfa > 0), SeuilAlerte INTEGER NOT NULL DEFAULT 0 CHECK (SeuilAlerte >= 0), Actif INTEGER NOT NULL DEFAULT 1 CHECK (Actif IN (0,1)), CreeLeUtc TEXT NOT NULL, ModifieLeUtc TEXT NOT NULL);
CREATE TABLE Vente (Id TEXT PRIMARY KEY, Numero TEXT NOT NULL UNIQUE, UtilisateurId TEXT NOT NULL REFERENCES Utilisateur(Id), TotalFcfa INTEGER NOT NULL CHECK (TotalFcfa >= 0), DateTechniqueUtc TEXT NOT NULL, DateMetier TEXT NOT NULL, FuseauHoraire TEXT NOT NULL, CreeLeUtc TEXT NOT NULL);
CREATE TABLE LigneVente (Id TEXT PRIMARY KEY, VenteId TEXT NOT NULL REFERENCES Vente(Id), ProduitId TEXT NOT NULL REFERENCES Produit(Id), ReferenceSnapshot TEXT NOT NULL, NomSnapshot TEXT NOT NULL, PrixUnitaireFcfa INTEGER NOT NULL CHECK (PrixUnitaireFcfa >= 0), Quantite INTEGER NOT NULL CHECK (Quantite > 0), UNIQUE(VenteId, ProduitId));
CREATE TABLE AnnulationVente (Id TEXT PRIMARY KEY, VenteId TEXT NOT NULL UNIQUE REFERENCES Vente(Id), UtilisateurId TEXT NOT NULL REFERENCES Utilisateur(Id), Motif TEXT NOT NULL, DateTechniqueUtc TEXT NOT NULL);
CREATE TABLE MouvementStock (Id TEXT PRIMARY KEY, ProduitId TEXT NOT NULL REFERENCES Produit(Id), Quantite INTEGER NOT NULL CHECK (Quantite <> 0), Type TEXT NOT NULL CHECK (Type IN ('STOCK_INITIAL','REAPPROVISIONNEMENT','CORRECTION','VENTE','ANNULATION')), Motif TEXT, QuantiteAvant INTEGER, QuantiteComptee INTEGER, ReferenceSource TEXT, UtilisateurId TEXT NOT NULL REFERENCES Utilisateur(Id), DateTechniqueUtc TEXT NOT NULL);
CREATE TABLE JournalAudit (Id TEXT PRIMARY KEY, UtilisateurId TEXT REFERENCES Utilisateur(Id), Action TEXT NOT NULL, Entite TEXT NOT NULL, EntiteId TEXT, Details TEXT, DateTechniqueUtc TEXT NOT NULL);
CREATE TABLE Parametre (Cle TEXT PRIMARY KEY, Valeur TEXT NOT NULL, ModifieLeUtc TEXT NOT NULL);
CREATE INDEX IX_Produit_Categorie ON Produit(CategorieId); CREATE INDEX IX_Produit_Actif ON Produit(Actif); CREATE INDEX IX_Vente_DateMetier ON Vente(DateMetier); CREATE INDEX IX_Vente_Utilisateur ON Vente(UtilisateurId); CREATE INDEX IX_LigneVente_Vente ON LigneVente(VenteId); CREATE INDEX IX_MouvementStock_ProduitDate ON MouvementStock(ProduitId, DateTechniqueUtc); CREATE INDEX IX_Audit_Date ON JournalAudit(DateTechniqueUtc);
CREATE VIEW StockCourant AS SELECT ProduitId, COALESCE(SUM(Quantite),0) AS Quantite FROM MouvementStock GROUP BY ProduitId;
""";
    private const string SecurityAndStockSql = """
ALTER TABLE Utilisateur ADD COLUMN PasswordHash TEXT NOT NULL DEFAULT '';
CREATE UNIQUE INDEX IF NOT EXISTS UX_MouvementStock_Initial ON MouvementStock(ProduitId) WHERE Type = 'STOCK_INITIAL';
""";
    private const string SalesSql = """
ALTER TABLE Vente ADD COLUMN TokenValidation TEXT NOT NULL DEFAULT '';
ALTER TABLE Vente ADD COLUMN Statut TEXT NOT NULL DEFAULT 'VALIDEE' CHECK (Statut IN ('VALIDEE','ANNULEE'));
ALTER TABLE Vente ADD COLUMN ModePaiement TEXT NOT NULL DEFAULT 'ESPECES' CHECK (ModePaiement IN ('ESPECES','MOBILE_MONEY','CARTE'));
ALTER TABLE Vente ADD COLUMN MontantRecuFcfa INTEGER NOT NULL DEFAULT 0 CHECK (MontantRecuFcfa >= 0);
ALTER TABLE Vente ADD COLUMN MonnaieRendueFcfa INTEGER NOT NULL DEFAULT 0 CHECK (MonnaieRendueFcfa >= 0);
UPDATE Vente SET TokenValidation = Id WHERE TokenValidation = '';
CREATE UNIQUE INDEX IF NOT EXISTS UX_Vente_TokenValidation ON Vente(TokenValidation);
CREATE INDEX IF NOT EXISTS IX_Vente_Statut ON Vente(Statut);
""";
    private const string AdministrationSql = """
CREATE TABLE IF NOT EXISTS Sauvegarde (Id TEXT PRIMARY KEY, Chemin TEXT NOT NULL, DateUtc TEXT NOT NULL, Type TEXT NOT NULL, Succes INTEGER NOT NULL CHECK (Succes IN (0,1)), Message TEXT);
CREATE INDEX IF NOT EXISTS IX_Sauvegarde_Date ON Sauvegarde(DateUtc DESC);
""";
}
