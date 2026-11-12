using GestionMagasin.Domain;

namespace GestionMagasin.Application;

public sealed class ReportingService(IReportingRepository repository)
{
    public Task<DashboardMetrics> DashboardAsync(Utilisateur acteur, DateTime jour, string fuseau, CancellationToken ct=default){Autorisation.VendeurOuAdmin(acteur);return repository.DashboardAsync(acteur,jour.Date,fuseau,ct);}
    public Task<IReadOnlyList<RapportProduit>> VentesParProduitAsync(Utilisateur acteur,DateTime debut,DateTime fin,string fuseau,CancellationToken ct=default){Autorisation.Admin(acteur);return repository.VentesParProduitAsync(acteur,debut,fin,fuseau,ct);}
    public Task<RapportSynthese> SyntheseAsync(Utilisateur acteur,DateTime debut,DateTime fin,string fuseau,CancellationToken ct=default){Autorisation.Admin(acteur);return repository.SyntheseAsync(acteur,debut,fin,fuseau,ct);}
    public Task<IReadOnlyList<MouvementStockDto>> StockAsync(Utilisateur acteur,CancellationToken ct=default){Autorisation.Admin(acteur);return repository.StockAsync(ct);}
}

public sealed class ParametresService(ISettingsRepository repository)
{
    public Task<ParametresMagasin> ObtenirAsync(Utilisateur acteur,CancellationToken ct=default){Autorisation.Admin(acteur);return repository.ObtenirAsync(ct);}
    public async Task EnregistrerAsync(Utilisateur acteur,ParametresMagasin parametres,CancellationToken ct=default){Autorisation.Admin(acteur);if(string.IsNullOrWhiteSpace(parametres.NomCommercial)||string.IsNullOrWhiteSpace(parametres.Devise)||string.IsNullOrWhiteSpace(parametres.FuseauHoraire))throw new DomainException("Nom commercial, devise et fuseau horaire sont obligatoires.");await repository.EnregistrerAsync(parametres,ct);}
}

public sealed class SauvegardeService(IBackupManager manager)
{
    public Task<SauvegardeInfo> SauvegarderAsync(Utilisateur acteur,string destination,string type="MANUELLE",CancellationToken ct=default){Autorisation.Admin(acteur);return manager.SauvegarderAsync(destination,type,acteur.Id,ct);}
    public Task RestaurerAsync(Utilisateur acteur,string source,CancellationToken ct=default){Autorisation.Admin(acteur);return manager.RestaurerAsync(source,ct);}
}
