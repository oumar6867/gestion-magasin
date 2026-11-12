# GestionMagasin

Socle technique d'une application de gestion de magasin en français. Le cahier des charges synchronisé dans `sources/` étant vide, cette version formalise le modèle minimal explicitement demandé et laisse les écrans métier à implémenter par itérations.

## Spécification technique

- Solution `GestionMagasin.sln`, C# et .NET 10 LTS figé dans chaque projet (`net10.0`, `net10.0-windows`).
- Architecture : `Domain` (modèle et contrats), `Application` (cas d'utilisation), `Infrastructure` (SQLite/ADO.NET/migrations), `WinForms` (composition et présentation), `Tests` (xUnit).
- SQLite via `Microsoft.Data.Sqlite`, exclusivement avec requêtes paramétrées dans les repositories.
- Les montants sont des `INTEGER` en FCFA et les quantités des `INTEGER` ; aucune quantité n'est stockée dans `Produit`.
- Les dates techniques sont UTC. `Vente.DateMetier` et `Vente.FuseauHoraire` conservent la date métier calculée à partir du fuseau configuré.
- Les produits, utilisateurs et ventes sont désactivables ou annulables ; aucune suppression physique n'est prévue.
- Les lignes de vente conservent `ReferenceSnapshot`, `NomSnapshot` et `PrixUnitaireFcfa`.
- Le stock est la somme signée des `MouvementStock`, exposée par la vue `StockCourant`.
- Les clés étrangères sont activées dans la chaîne de connexion et répétées par PRAGMA à chaque ouverture. Les CHECK, contraintes uniques et index sont définis dans la migration.

## Migrations et bases

`VersionSchema` est la table de suivi. Chaque migration est exécutée dans une transaction et marquée uniquement après succès ; relancer l'initialisation est donc sans effet destructif. La base de production est `%LOCALAPPDATA%\GestionMagasin\gestion-magasin.db`. La base de démonstration est distincte dans `%LOCALAPPDATA%\GestionMagasin\demo\gestion-magasin-demo.db` et peut être créée avec `DemoDatabase.CreateAsync()`.

## Commandes

```powershell
$env:DOTNET_CLI_HOME = "$pwd\.dotnet-home"
$env:NUGET_PACKAGES = "$pwd\.nuget-packages"
dotnet restore GestionMagasin.sln --ignore-failed-sources
dotnet build GestionMagasin.sln --configuration Release -m:1
dotnet test GestionMagasin.Tests\GestionMagasin.Tests.csproj --configuration Release -m:1
dotnet run --project GestionMagasin.WinForms --configuration Release
```

## Déploiement Windows x64

Le script `deploy.ps1` produit le paquet autonome sans Visual Studio ni runtime .NET préinstallé :

```powershell
.\deploy.ps1
```

Avec Inno Setup 6 installé et `ISCC.exe` disponible dans le `PATH`, il produit aussi l'installateur Windows :

```powershell
.\deploy.ps1 -Version 1.0.0 -RunInnoSetup
```

L'installateur affiche la licence MIT, crée les raccourcis et ne supprime pas les données de `%LOCALAPPDATA%\GestionMagasin` lors d'une désinstallation ou d'une mise à jour. Le prompt réutilisable est documenté dans `PROMPT_INSTALLATEUR.md`.

## Fonctionnalités du socle actuel

- Premier démarrage avec création interactive d'un administrateur, puis connexion obligatoire.
- PBKDF2-HMAC-SHA256, 210 000 itérations, sel aléatoire de 128 bits et clé dérivée de 256 bits. Le format stocké est `PBKDF2-SHA256:iterations:salt:key` ; aucun mot de passe en clair n'est persisté.
- Autorisations métier dans `GestionMagasin.Application` : administrateur pour la gestion, vendeur pour la consultation et ses opérations autorisées. Le dernier administrateur actif ne peut pas être désactivé.
- Écrans WinForms : connexion, utilisateurs, catégories, produits, mouvements de stock et inventaire. La navigation est latérale, redimensionnable et utilise un libellé textuel en plus des couleurs de stock : rouge/rupture, orange/faible, vert/disponible.
- Mouvements : stock initial unique, réapprovisionnement, correction avec quantités avant/comptée, vente et annulation réservés aux services métier. Le stock négatif et le réapprovisionnement d'un produit archivé sont refusés.
- Tests métier nommés `T01`, `T02`, `T06`, `T10` et `T14` dans `GestionMagasin.Tests/BusinessRulesTests.cs`.
- Cycle de vente complet : panier en mémoire avec fusion des lignes, relecture transactionnelle des prix et stocks, paiements espèces/Mobile Money/carte, jeton de validation, historique filtrable, détail, ticket imprimable et annulation administrative atomique.
- Les tests `T03`, `T04`, `T05`, `T07`, `T08`, `T09`, `T12`, `T13` et `T16` couvrent les ventes, les conflits de prix, le stock, l'annulation et la persistance après réinitialisation.
- Administration : tableau de bord métier, rapports par période/fuseau, export CSV UTF-8 BOM protégé contre les formules, paramètres du magasin, sauvegarde/restauration SQLite via `BackupDatabase`, rétention automatique de sept sauvegardes et tests `T11`, `T15`, `T18`.

Toute évolution de schéma doit ajouter une migration numérotée, sans modifier rétroactivement une migration appliquée.
