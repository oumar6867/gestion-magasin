# Procès-verbal de recette

| Cas | Précondition / action | Résultat attendu | Résultat obtenu | Statut |
|---|---|---|---|---|
| T01 | Base vierge, créer le premier administrateur | Création sans compte par défaut, connexion valide | Test automatisé présent | À exécuter avec runner fonctionnel |
| T02 | Dernier administrateur actif | Désactivation refusée | Test automatisé présent | À exécuter avec runner fonctionnel |
| T03 | Stock disponible, vente espèces | Vente atomique, monnaie calculée | Test automatisé présent | À exécuter avec runner fonctionnel |
| T04 | Quantité supérieure au stock | Vente refusée sans écriture | Test automatisé présent | À exécuter avec runner fonctionnel |
| T05 | Prix catalogue modifié | Confirmation/relecture requise | Test automatisé présent | À exécuter avec runner fonctionnel |
| T06 | Produit à prix nul | Création refusée | Test automatisé présent | À exécuter avec runner fonctionnel |
| T07 | Vente puis réinitialisation | Données conservées | Test automatisé présent | À exécuter avec runner fonctionnel |
| T08 | Vente validée | Annulation inverse le stock | Test automatisé présent | À exécuter avec runner fonctionnel |
| T09 | Vente déjà annulée | Deuxième annulation refusée | Test automatisé présent | À exécuter avec runner fonctionnel |
| T10 | Deux stocks initiaux | Un seul autorisé | Test automatisé présent | À exécuter avec runner fonctionnel |
| T11 | Vente annulée dans la journée | Dashboard exclut la vente | Test automatisé présent | À exécuter avec runner fonctionnel |
| T12 | Paiement carte | Montant reçu forcé au total | Test automatisé présent | À exécuter avec runner fonctionnel |
| T13 | Vendeur connecté | Historique limité à ses ventes | Test automatisé présent | À exécuter avec runner fonctionnel |
| T14 | Correction donnant un stock négatif | Refusée | Test automatisé présent | À exécuter avec runner fonctionnel |
| T15 | Export avec texte `=FORMULE` | UTF-8 BOM et cellule protégée | Test automatisé présent | À exécuter avec runner fonctionnel |
| T16 | Erreur avant validation | Aucune vente ni mouvement partiel | Test automatisé présent | À exécuter avec runner fonctionnel |
| T17 | Base volumineuse | Listes/recherche sous 2 s, pagination | Pagination et benchmark automatisé à compléter | Non validé |
| T18 | Sauvegarde SQLite | Copie intègre et vérifiable | Test automatisé présent | À exécuter avec runner fonctionnel |

La compilation Release est validée. L'exécution xUnit reste bloquée après la découverte des tests dans l'environnement de travail ; le rapport distingue donc les tests présents des tests effectivement exécutés.
