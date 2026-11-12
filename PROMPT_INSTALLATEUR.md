# Prompt de création de l'installateur

> Crée un paquet Windows x64 autonome pour GestionMagasin à partir de `GestionMagasin.WinForms`. Publie en Release avec `dotnet publish`, `win-x64`, `SelfContained=true`, `PublishSingleFile=true` et `IncludeNativeLibrariesForSelfExtract=true`. Prépare ensuite un installateur Inno Setup qui installe `GestionMagasin.WinForms.exe` dans `Program Files\GestionMagasin`, crée un raccourci dans le menu Démarrer et sur le bureau, conserve `%LOCALAPPDATA%\GestionMagasin` lors des mises à jour, désinstalle uniquement les binaires de l'application et affiche la licence MIT. Vérifie que le paquet démarre sur un ordinateur Windows x64 sans Visual Studio ni SDK .NET.

Le projet est open source sous licence MIT. La licence complète est dans `LICENSE` et doit être incluse dans le paquet livré.
