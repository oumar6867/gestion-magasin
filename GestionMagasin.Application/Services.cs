using GestionMagasin.Domain;

namespace GestionMagasin.Application;

public sealed class ProduitService(IProduitRepository produits)
{
    public Task<IReadOnlyList<Produit>> RechercherAsync(ProduitFilter filtre, CancellationToken ct = default) => produits.RechercherAsync(filtre, ct);
    public async Task CreerAsync(Produit produit, CancellationToken ct = default) { Validation(produit); await produits.AjouterAsync(produit, ct); }
    public async Task ModifierAsync(Utilisateur acteur, Produit produit, CancellationToken ct = default) { Autorisation.Admin(acteur); Validation(produit); await produits.ModifierAsync(produit, ct); }
    public Task ArchiverAsync(Utilisateur acteur, Produit produit, CancellationToken ct = default) { Autorisation.Admin(acteur); return produits.ModifierAsync(produit with { Actif = false }, ct); }
    static void Validation(Produit p) { if (string.IsNullOrWhiteSpace(p.Reference) || string.IsNullOrWhiteSpace(p.Nom)) throw new DomainException("La référence et le nom sont obligatoires."); ReglesDomaine.VerifierMontant(p.PrixVenteFcfa); if (p.SeuilAlerte < 0) throw new DomainException("Le seuil doit être positif ou nul."); }
}

public sealed class StockService(IStockRepository stock, IProduitRepository produits)
{
    public Task<int> ObtenirStockAsync(Guid produitId, CancellationToken ct = default) => stock.ObtenirStockAsync(produitId, ct);
    public Task<IReadOnlyList<MouvementStockDto>> ListerAsync(Guid? produitId = null, CancellationToken ct = default) => stock.ListerAsync(produitId, ct);
    public async Task EnregistrerAsync(Utilisateur acteur, MouvementStock mouvement, CancellationToken ct = default)
    {
        Autorisation.Admin(acteur); var produit = await produits.GetByIdAsync(mouvement.ProduitId, ct) ?? throw new DomainException("Produit introuvable.");
        if (!produit.Actif) throw new DomainException("Le produit archivé ne peut plus recevoir de stock.");
        if (string.IsNullOrWhiteSpace(mouvement.Motif)) throw new DomainException("Le motif est obligatoire.");
        var historiques = await stock.ListerAsync(mouvement.ProduitId, ct);
        if (mouvement.Type == TypesMouvement.StockInitial && historiques.Any(x => x.Mouvement.Type == TypesMouvement.StockInitial)) throw new DomainException("Le stock initial existe déjà pour ce produit.");
        if (mouvement.Type == TypesMouvement.StockInitial && mouvement.Quantite <= 0) throw new DomainException("Le stock initial doit être positif.");
        if (mouvement.Type == TypesMouvement.Reapprovisionnement && mouvement.Quantite <= 0) throw new DomainException("Le réapprovisionnement doit être positif.");
        if (mouvement.Type == TypesMouvement.Correction && (mouvement.QuantiteAvant is null || mouvement.QuantiteComptee is null)) throw new DomainException("Une correction doit conserver les quantités avant et comptée.");
        if (await stock.ObtenirStockAsync(mouvement.ProduitId, ct) + mouvement.Quantite < 0) throw new DomainException("Le stock négatif est interdit.");
        await stock.AjouterMouvementAsync(mouvement with { UtilisateurId = acteur.Id }, ct);
    }
}

public sealed class AuthentificationService(IUtilisateurRepository utilisateurs)
{
    public async Task<bool> PremierDemarrageAsync(CancellationToken ct = default) => (await utilisateurs.ListerAsync(ct)).Count == 0;
    public async Task<Utilisateur> CreerPremierAdministrateurAsync(string identifiant, string nom, string motDePasse, CancellationToken ct = default) { if (!await PremierDemarrageAsync(ct)) throw new DomainException("Le premier administrateur existe déjà."); Validation(identifiant, nom, motDePasse); var u = new Utilisateur(Guid.NewGuid(), identifiant.Trim(), nom.Trim(), Roles.Administrateur); await utilisateurs.AjouterAsync(u, motDePasse, ct); return u; }
    public async Task<Utilisateur?> ConnexionAsync(string identifiant, string motDePasse, CancellationToken ct = default) { var u = await utilisateurs.AuthentifierAsync(identifiant.Trim(), motDePasse, ct); return u is { Actif: true } ? u : null; }
    static void Validation(string i, string n, string p) { if (string.IsNullOrWhiteSpace(i) || string.IsNullOrWhiteSpace(n) || p.Length < 8) throw new DomainException("Identifiant, nom et mot de passe (8 caractères minimum) sont obligatoires."); }
}

public sealed class UtilisateurService(IUtilisateurRepository utilisateurs)
{
    public Task<IReadOnlyList<Utilisateur>> ListerAsync(Utilisateur acteur, CancellationToken ct = default) { Autorisation.Admin(acteur); return utilisateurs.ListerAsync(ct); }
    public async Task CreerAsync(Utilisateur acteur, Utilisateur utilisateur, string motDePasse, CancellationToken ct = default) { Autorisation.Admin(acteur); if (motDePasse.Length < 8) throw new DomainException("Le mot de passe doit comporter au moins 8 caractères."); await utilisateurs.AjouterAsync(utilisateur, motDePasse, ct); }
    public async Task DesactiverAsync(Utilisateur acteur, Utilisateur cible, CancellationToken ct = default) { Autorisation.Admin(acteur); if (cible.Role == Roles.Administrateur && cible.Actif && (await utilisateurs.ListerAsync(ct)).Count(x => x.Role == Roles.Administrateur && x.Actif) <= 1) throw new DomainException("Le dernier administrateur actif ne peut pas être désactivé."); await utilisateurs.ModifierAsync(cible with { Actif = false }, null, ct); }
}

public static class Autorisation
{
    public static void Admin(Utilisateur u) { if (!u.Actif || u.Role != Roles.Administrateur) throw new DomainException("Cette action est réservée à l'administrateur."); }
    public static void VendeurOuAdmin(Utilisateur u) { if (!u.Actif || (u.Role != Roles.Administrateur && u.Role != Roles.Vendeur)) throw new DomainException("Utilisateur non autorisé."); }
}
