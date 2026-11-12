namespace GestionMagasin.Domain;

public static class Roles { public const string Administrateur = "ADMIN"; public const string Vendeur = "VENDEUR"; }
public sealed record Utilisateur(Guid Id, string NomUtilisateur, string NomComplet, string Role, bool Actif = true);
public sealed record Categorie(Guid Id, string Nom, bool Active = true);
public sealed record Produit(Guid Id, string Reference, string Nom, Guid? CategorieId, long PrixVenteFcfa, int SeuilAlerte, DateTime CreeLeUtc, bool Actif = true);
public sealed record Vente(Guid Id, string Numero, Guid UtilisateurId, long TotalFcfa, DateTime DateTechniqueUtc, DateTime DateMetier, string FuseauHoraire);
public sealed record LigneVente(Guid Id, Guid VenteId, Guid ProduitId, string ReferenceSnapshot, string NomSnapshot, long PrixUnitaireFcfa, int Quantite);
public static class ModesPaiement { public const string Especes="ESPECES"; public const string MobileMoney="MOBILE_MONEY"; public const string Carte="CARTE"; }
public sealed record PanierLigne(Guid ProduitId, int Quantite, string? ReferenceAttendue = null, long? PrixAttenduFcfa = null);
public sealed record VenteResultat(Guid VenteId, string Numero, string JetonValidation, long TotalFcfa, long MontantRecuFcfa, long MonnaieRendueFcfa);
public sealed record VenteHistorique(Guid Id, string Numero, DateTime DateMetier, string Vendeur, long TotalFcfa, string ModePaiement, string Statut);
public sealed record VenteDetail(VenteHistorique Vente, IReadOnlyList<LigneVente> Lignes, long MontantRecuFcfa, long MonnaieRendueFcfa);
public sealed record AnnulationVente(Guid Id, Guid VenteId, Guid UtilisateurId, string Motif, DateTime DateTechniqueUtc);
public static class TypesMouvement { public const string StockInitial = "STOCK_INITIAL"; public const string Reapprovisionnement = "REAPPROVISIONNEMENT"; public const string Correction = "CORRECTION"; public const string Vente = "VENTE"; public const string Annulation = "ANNULATION"; }
public sealed record MouvementStock(Guid Id, Guid ProduitId, int Quantite, string Type, string Motif, Guid UtilisateurId, DateTime DateTechniqueUtc, int? QuantiteAvant = null, int? QuantiteComptee = null, string? ReferenceSource = null);
public sealed record MouvementStockDto(MouvementStock Mouvement, int StockApres);
public sealed record ProduitFilter(string? Recherche = null, Guid? CategorieId = null, bool? Disponible = null, bool? Actif = null);
public sealed record JournalAudit(Guid Id, Guid? UtilisateurId, string Action, string Entite, Guid? EntiteId, DateTime DateTechniqueUtc, string? Details);
public sealed record Parametre(string Cle, string Valeur, DateTime ModifieLeUtc);
public sealed record VersionSchema(int Version, DateTime AppliqueLeUtc);
public sealed record DashboardMetrics(DateTime DateMetier, long VentesFcfa, int NombreVentes, int UnitesVendues, int ReferencesEnRupture, int ReferencesStockFaible);
public sealed record RapportProduit(string Reference, string Nom, int UnitesVendues, long MontantFcfa);
public sealed record RapportSynthese(int NombreVentes, int UnitesVendues, long MontantFcfa);
public sealed record ParametresMagasin(string NomCommercial, string Coordonnees, string? LogoPath, string Devise, string FuseauHoraire, string PolitiqueSauvegarde);
public sealed record SauvegardeInfo(Guid Id, string Chemin, DateTime DateUtc, string Type, bool Succes, string? Message);
public sealed record AuditEntry(Guid Id, Guid? UtilisateurId, string Action, string Entite, Guid? EntiteId, DateTime DateUtc, string? Details);

public static class ReglesDomaine
{
    public static void VerifierMontant(long montant) { if (montant <= 0) throw new DomainException("Le prix doit être strictement positif."); }
    public static void VerifierQuantite(int quantite) { if (quantite <= 0) throw new DomainException("La quantité doit être un entier positif."); }
}
public class DomainException(string message) : Exception(message);
