# Sauvegarde et restauration

Une sauvegarde automatique est créée au premier démarrage de chaque jour dans `%LOCALAPPDATA%\GestionMagasin\backups`. Les sept dernières sauvegardes sont conservées.

La sauvegarde manuelle peut être copiée vers un support externe. Pour restaurer : ouvrir Paramètres, sélectionner la base, confirmer ; la base active est d'abord conservée avec le suffixe `.before-restore-...db`, puis l'intégrité et `VersionSchema` sont vérifiés. En cas d'échec, la copie précédente reste disponible.

Ne jamais supprimer la base active ou les sauvegardes avant d'avoir vérifié la nouvelle installation.
