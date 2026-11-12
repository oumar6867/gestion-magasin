param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0",
    [switch]$RunInnoSetup
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$publish = Join-Path $root "livraison\GestionMagasin-win-x64"

& dotnet publish (Join-Path $root "GestionMagasin.WinForms\GestionMagasin.WinForms.csproj") `
    --configuration $Configuration --runtime win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    --output $publish

Copy-Item (Join-Path $root "LICENSE"), (Join-Path $root "README.md"), (Join-Path $root "GUIDE_INSTALLATION.md"), (Join-Path $root "GUIDE_UTILISATEUR.md"), (Join-Path $root "GUIDE_SAUVEGARDE.md"), (Join-Path $root "DEPENDANCES_LICENCES.md") -Destination $publish -Force

if ($RunInnoSetup) {
    $iscc = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if (-not $iscc) { throw "Inno Setup 6 (ISCC.exe) est requis pour créer l'installateur." }
    & $iscc.Source (Join-Path $root "installer\GestionMagasin.iss")
}

Write-Host "Paquet autonome créé : $publish"
if (-not $RunInnoSetup) { Write-Host "Pour créer le setup Inno Setup : .\deploy.ps1 -RunInnoSetup" }
