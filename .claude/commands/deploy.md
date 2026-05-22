Prépare le déploiement du module $ARGUMENTS (ou de l'application complète si non précisé).

## Checklist pré-déploiement

### Build et tests
- [ ] `dotnet build --configuration Release` sans warning
- [ ] `dotnet test` — tous les tests passent
- [ ] Aucun secret en dur dans le code (grep sur `password`, `apikey`, `secret`)

### Migrations EF Core
- [ ] Vérifier les migrations en attente pour chaque module
- [ ] Si migrations présentes, **proposer les scripts SQL idempotents** et attendre confirmation
- [ ] Ne jamais exécuter `dotnet ef database update` automatiquement en production

### Configuration
- [ ] Variables d'environnement requises documentées
- [ ] `appsettings.Production.json` ne contient pas de secrets
- [ ] Options OAuth2 PISTE (`PisteAuthOptions`) configurées

### Réglementaire
- [ ] Environnement cible confirmé : Sandbox (`sandbox-api.aife.economie.gouv.fr`) ou Production (`api.aife.economie.gouv.fr`)
- [ ] Ne jamais déployer avec la config Sandbox en Production

## Commandes de build

```
dotnet build --configuration Release
dotnet test --configuration Release --no-build
dotnet publish src/Api/FacturXManager.Api --configuration Release --output ./publish
```

## ⚠️ Actions interdites sans confirmation explicite
- Exécuter des migrations en base
- Pousser vers la branche principale (`git push origin main`)
- Modifier des variables d'environnement de production
