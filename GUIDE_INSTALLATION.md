# Guide d'installation

1. Utiliser Windows x64.
2. Décompresser le dossier publié dans un répertoire accessible à l'utilisateur.
3. Lancer `GestionMagasin.WinForms.exe`.
4. Au premier démarrage, créer l'administrateur ; aucun identifiant par défaut n'existe.

Le paquet autonome publié contient le runtime .NET : Visual Studio et le SDK .NET ne sont pas nécessaires. La base active est créée dans `%LOCALAPPDATA%\GestionMagasin\gestion-magasin.db`.

Mise à jour sans perte : fermer l'application, copier le dossier publié, conserver la base existante, puis relancer. Les migrations versionnées s'appliquent automatiquement. Faire une sauvegarde avant toute mise à jour majeure.
