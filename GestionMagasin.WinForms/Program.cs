using GestionMagasin.Application;
using GestionMagasin.Infrastructure;

namespace GestionMagasin.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize(); var factory=new SqliteConnectionFactory(DatabasePaths.Production()); new SqliteDatabaseInitializer(factory).InitializeAsync().GetAwaiter().GetResult();
        var users=new SqliteUserRepository(factory); var categories=new SqliteCategoryRepository(factory); var products=new SqliteProductRepository(factory); var stock=new SqliteStockRepository(factory); var sales=new SqliteSaleRepository(factory); var reporting=new SqliteReportingRepository(factory); var settings=new SqliteSettingsRepository(factory); var backup=new SqliteBackupManager(factory); var auth=new AuthentificationService(users);
        if(auth.PremierDemarrageAsync().GetAwaiter().GetResult()) using(var first=new FirstAdminForm(auth)) if(first.ShowDialog()!=DialogResult.OK)return;
        using var login=new LoginForm(auth); if(login.ShowDialog()!=DialogResult.OK||login.User is null)return;
        backup.EnsureDailyAsync().GetAwaiter().GetResult();
        System.Windows.Forms.Application.Run(new MainForm(login.User,factory.DatabasePath,new ProduitService(products),new StockService(stock,products),new UtilisateurService(users),categories,products,new VenteService(sales),users,new ReportingService(reporting),new ParametresService(settings),new SauvegardeService(backup)));
    }
}
