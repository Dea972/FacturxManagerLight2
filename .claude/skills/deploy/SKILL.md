---
name: deploy
description: >
  S'active automatiquement quand une tâche implique la préparation d'un déploiement,
  la génération de scripts de migration SQL, la vérification de la configuration
  d'environnement, ou le build Release. Ne modifie jamais la base de données
  ni ne pousse vers git sans confirmation explicite.
allowed-tools: [Read, Bash]
---

# Skill : déploiement FacturX Manager

## Règles absolues

- **Ne jamais exécuter `dotnet ef database update`** — proposer les scripts SQL uniquement
- **Ne jamais faire `git push`** sans confirmation explicite de l'utilisateur
- **Ne jamais utiliser la config Sandbox en Production** — vérifier les URLs systématiquement

## Commandes de build autorisées (exécutables automatiquement)

```bash
dotnet build --configuration Release
dotnet test --configuration Release --no-build
```

## Migrations EF Core — workflow obligatoire

1. Détecter les migrations en attente :
   ```bash
   dotnet ef migrations list --project src/Modules/<Module>/FacturXManager.Modules.<Module>.Infrastructure --startup-project src/Api/FacturXManager.Api
   ```

2. Générer le script idempotent (deux lignes distinctes, ne pas chaîner) :
   ```
   dotnet ef migrations add <NomMigration> --project src/Modules/<Module>/FacturXManager.Modules.<Module>.Infrastructure --startup-project src/Api/FacturXManager.Api --context <Module>DbContext

   dotnet ef migrations script --idempotent --project src/Modules/<Module>/FacturXManager.Modules.<Module>.Infrastructure --startup-project src/Api/FacturXManager.Api --context <Module>DbContext -o src/Modules/<Module>/FacturXManager.Modules.<Module>.Infrastructure/Database/Scripts/AlterTable.sql
   ```

3. **Prévenir** que `AlterTable.sql` sera écrasé — demander de sauvegarder/committer avant

4. **Ne pas exécuter** le script SQL — le présenter pour revue

## Variables d'environnement requises

```
ConnectionStrings__Database          → SQL Server connection string
Piste__Auth__ClientId                → Client ID OAuth2 PISTE
Piste__Auth__ClientSecret            → Client Secret OAuth2 PISTE
Piste__Auth__TokenEndpoint           → URL token PISTE
ChorusPro__BaseUrl                   → URL API Chorus Pro (sandbox ou prod)
```

## Vérification pré-déploiement

Avant tout déploiement, confirmer :
- [ ] Build Release sans warning
- [ ] Tous les tests passent
- [ ] Aucun secret en dur (grep `password|apikey|clientsecret` dans src/)
- [ ] Environnement cible confirmé (Sandbox vs Production)
- [ ] Scripts de migration revus et approuvés
