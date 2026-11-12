# Dépendances et licences

- .NET 10 LTS / Windows Forms : licence MIT et composants Microsoft, selon les notices du runtime.
- `Microsoft.Data.Sqlite` 9.0.0 : licence MIT.
- xUnit v3, Microsoft.NET.Test.Sdk et coverlet.collector : licences open source indiquées dans leurs paquets NuGet.
- SQLitePCLRaw transitivement utilisé par Microsoft.Data.Sqlite : vérifier les notices du paquet lors de la redistribution.

Le paquet autonome doit conserver les fichiers `.deps.json` et `.runtimeconfig.json` générés par `dotnet publish`.
