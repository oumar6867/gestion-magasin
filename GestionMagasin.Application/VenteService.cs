using GestionMagasin.Domain;

namespace GestionMagasin.Application;

public sealed class VenteService(IVenteRepository ventes)
{
    public async Task<VenteResultat> ValiderAsync(Utilisateur vendeur, IReadOnlyList<PanierLigne> panier, string modePaiement, long montantRecuFcfa, string fuseauHoraire, CancellationToken ct = default)
    {
        Autorisation.VendeurOuAdmin(vendeur); if (panier.Count == 0) throw new DomainException("Le panier doit contenir au moins une ligne.");
        if (panier.Any(x => x.Quantite <= 0)) throw new DomainException("Les quantités doivent être des entiers strictement positifs.");
        if (modePaiement is not (ModesPaiement.Especes or ModesPaiement.MobileMoney or ModesPaiement.Carte)) throw new DomainException("Mode de paiement invalide.");
        if (montantRecuFcfa < 0) throw new DomainException("Le montant reçu est invalide.");
        return await ventes.ValiderAsync(vendeur, panier, modePaiement, montantRecuFcfa, fuseauHoraire, ct);
    }
    public Task<IReadOnlyList<VenteHistorique>> RechercherAsync(Utilisateur acteur,string? numero,DateTime? du,DateTime? au,Guid? vendeurId,string? statut,CancellationToken ct=default){Autorisation.VendeurOuAdmin(acteur);return ventes.RechercherAsync(numero,du,au,vendeurId,statut,acteur,ct);}
    public Task<VenteDetail?> DetailAsync(Utilisateur acteur,Guid id,CancellationToken ct=default){Autorisation.VendeurOuAdmin(acteur);return ventes.ObtenirDetailAsync(id,ct);}
    public Task AnnulerAsync(Utilisateur acteur,Guid id,string motif,CancellationToken ct=default){Autorisation.Admin(acteur);if(string.IsNullOrWhiteSpace(motif))throw new DomainException("Le motif d'annulation est obligatoire.");return ventes.AnnulerAsync(id,acteur,motif,ct);}
}
