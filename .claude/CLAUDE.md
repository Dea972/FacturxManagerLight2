# FacturX Manager

Application .NET 10 / C# de génération et transmission de factures électroniques
au format Factur-X (PDF/A-3b + XML CII embarqué), conformes EN16931, vers Chorus Pro
via l'API PISTE (AIFE). Réforme française de facturation électronique obligatoire
à partir du 1er septembre 2026.

---

## Architecture

Modulaire monolith DDD + CQRS (sans Event Sourcing).

Chaque module est autonome et suit strictement cette structure de projets :

```
FacturXManager.Modules.<Module>.Domain/
FacturXManager.Modules.<Module>.Application/
FacturXManager.Modules.<Module>.Infrastructure/   ← contient aussi <Module>Module.cs
FacturXManager.Modules.<Module>.Presentation/     ← endpoints Minimal API via IEndpoint
FacturXManager.Modules.<Module>.IntegrationEvents/ ← uniquement si communication inter-modules nécessaire
```

### Bounded contexts

| Module         | Responsabilité                                                    |
|----------------|-------------------------------------------------------------------|
| `Compliance`   | Validation EN16931, génération XML CII + PDF/A-3b                 |
| `Transmission` | Acheminement vers Chorus Pro / PDP / PPF (Y-model)                |
| `EReporting`   | Reporting B2C / international DGFiP (futur)                       |

Shared Kernel : `FacturXManager.Common.*` (Domain, Application, Infrastructure)

---

## Structure interne des modules

### Domain
```
<Agrégat>/
├── <Agrégat>.cs                        ← sealed partial class, constructeur private, static Create(...)
├── <Agrégat>Errors.cs                  ← static class avec static readonly Error
└── <Sujet><Action>DomainEvent.cs       ← uniquement si un event de domaine est nécessaire
<ValueObject>.cs                        ← hérite de ValueObject
```

### Application
```
<UseCase>/                              ← un dossier par use case, pas de sous-dossiers Commands/ ou Queries/
├── <UseCase>Command.cs                 ← sealed record, implémente ICommand ou ICommand<T>
├── <UseCase>CommandHandler.cs          ← internal sealed, implémente ICommandHandler<,>
├── <UseCase>CommandValidator.cs        ← AbstractValidator<T>, uniquement sur les Commands
├── <UseCase>Query.cs                   ← sealed record, implémente IQuery<T>
├── <UseCase>QueryHandler.cs            ← internal sealed, implémente IQueryHandler<,>
└── <UseCase>Response.cs                ← sealed record, suffixe Response réservé aux queries
Abstractions/
└── Data/IUnitOfWork.cs
```

### Infrastructure
```
Database/
├── <Module>DbContext.cs                ← un DbContext par module, HasDefaultSchema(Schemas.<Module>)
├── Schemas.cs                          ← internal sealed class avec const string
└── Migrations/
<Agrégat>/
├── <Entité>Configuration.cs            ← IEntityTypeConfiguration<T>
├── <Entité>Repository.cs
└── <Entité>Service.cs                  ← si logique partagée entre commands
<Module>Module.cs                       ← point d'entrée DI, AddXxxModuleAsync(services, config, ...)
```

### Presentation
```
<Agrégat>/
└── <UseCase>.cs                        ← internal sealed class : IEndpoint
                                           request record interne à la classe
                                           mapping manuel vers la Command
```

---

## Conventions DDD

- **Agrégat** : `sealed partial class`, constructeur `private`, factory `static Create(...)`
- **Entité** : hérite de `Entity` (domain events via `Raise()`) ou `AuditableEntity`
- **Value Object** : hérite de `ValueObject`, implémente `GetEqualityComponents()`
- **Domain Event** : hérite de `DomainEvent`, nommé `<Sujet><Action>DomainEvent` — créer uniquement si nécessaire
- **Erreurs** : classe statique `<Entité>Errors` avec champs `static readonly Error`
- `Raise()` uniquement dans l'agrégat lui-même, jamais depuis l'extérieur
- Propriétés : `private set` ou `init`, jamais de setter public

---

## Conventions CQRS (MediatR)

- Interfaces : `ICommand`, `ICommand<T>`, `IQuery<T>` — handlers via `ICommandHandler` / `IQueryHandler`
- Tous retournent `Result` ou `Result<T>` — **jamais d'exception métier**
- **Validators FluentValidation uniquement sur les Commands** — jamais sur les Queries
  Le `ValidationPipelineBehavior` est filtré sur `IBaseCommand` et n'intercepte pas les queries.
  Pour les queries, utiliser des **guards explicites** dans le handler (`if` + `Result.Failure`)
  avec les erreurs déclarées dans `<Entité>Errors`
- **Ne jamais appeler une Command dans une autre Command**
  Extraire la logique partagée dans un Service (interface dans Application, implémentation dans Infrastructure)
- **Suffixe `Response` réservé aux queries** — une Command retourne `Result` ou `Result<T>` directement
  Si T doit être structuré, nommer `<UseCase>Summary`, pas `<UseCase>Response`
- **QueryHandlers toujours dans Application**, jamais dans Infrastructure
  - Query sur entité → passer par le repository
  - Query sur DTO de lecture → passer par un data service (ex: `IDapper*Service`)
- Les interfaces déclarées dans Application doivent être consommées dans Application
  Si une interface n'est appelée que dans Infrastructure → la déclarer `internal` dans Infrastructure

---

## Communication inter-modules

Uniquement si nécessaire — ne pas créer d'IntegrationEvents par défaut.

Quand c'est nécessaire :
- `DomainEventHandler` publie un `IntegrationEvent` via `IEventBus`
- `IntegrationEvent` : hérite de `IntegrationEvent`, nommé `<Sujet><Action>IntegrationEvent`
- Events exposés dans un projet séparé `<Module>.IntegrationEvents`
- Outbox/Inbox via `InsertOutboxMessagesInterceptor` + jobs Quartz

---

## Conventions Infrastructure

- **EF Core + SQL Server** — un `DbContext` par module, schéma SQL dédié (`Schemas.<Module>`)
- `UseSnakeCaseNamingConvention()` sur tous les DbContext
- Configuration EF : une `IEntityTypeConfiguration<T>` par entité
- **Dapper** pour les lectures via `IDbConnectionFactory` quand pertinent
- DI : `AddXxxModuleAsync(services, config, ...)` dans `<Module>Module.cs`
- Mapping : **Riok.Mapperly**
- HTTP clients externes : **Refit**, interfaces séparées lecture/écriture si applicable
- Enregistrement DI des clients HTTP : centraliser la configuration `BaseAddress`
- **Migrations EF Core** : ne **jamais** exécuter automatiquement
  Proposer la commande et attendre confirmation avant d'exécuter
  Migrations dans `<Module>.Infrastructure/Database/Migrations/`

  Commandes (deux lignes distinctes, ne pas chaîner avec `&&`) :
  ```
  dotnet ef migrations add <NomMigration> --project src/Modules/<Module>/FacturXManager.Modules.<Module>.Infrastructure --startup-project src/Api/FacturXManager.Api --context <Module>DbContext
  dotnet ef migrations script --idempotent --project src/Modules/<Module>/FacturXManager.Modules.<Module>.Infrastructure --startup-project src/Api/FacturXManager.Api --context <Module>DbContext -o src/Modules/<Module>/FacturXManager.Modules.<Module>.Infrastructure/Database/Scripts/AlterTable.sql
  ```

---

## Conventions Presentation (Minimal API)

- Un fichier par use case : `internal sealed class <UseCase> : IEndpoint`
- La request est un `sealed record` déclaré **à l'intérieur** de la classe endpoint
- Mapping manuel de la request vers la Command dans `MapEndpoint`
- Pattern : `Result<T> result = await sender.Send(command); return result.Match(Results.Ok, ApiResults.Problem);`
- Les types du Domain (enums, value objects) ne doivent **jamais** apparaître dans une Command
  C'est le handler qui instancie les types Domain à partir des scalaires reçus
- Les propriétés d'une Command sont des types simples (string, Guid, decimal, int, bool, enum)

---

## Conventions de tests

| Type          | Projet suffix         | Frameworks                                    |
|---------------|-----------------------|-----------------------------------------------|
| Unitaires     | `.UnitTests`          | xUnit, FluentAssertions, Bogus                |
| Intégration   | `.IntegrationTests`   | xUnit, FluentAssertions, Testcontainers       |
| Architecture  | `.ArchitectureTests`  | NetArchTest                                   |

- Structure : **Arrange / Act / Assert** (commenté)
- Nommage : `<Méthode>_Should<Résultat>_When<Condition>`
- `BaseIntegrationTest` fournit `Faker` (Bogus), `Sender` (ISender), `ServiceProvider`, `DbContext`
- `IntegrationTestWebAppFactory` : Testcontainers SQL Server, seeds dans `SeedXxxAsync()`

### CommandHelpers / QueryHelpers

Chaque module de tests d'intégration expose deux classes statiques d'extension sur `ISender` :
- `CommandHelpers` (`Abstractions/CommandHelpers.cs`) — commandes récurrentes avec assertion `IsSuccess`
- `QueryHelpers` (`Abstractions/QueryHelpers.cs`) — queries récurrentes avec assertion `IsSuccess`

**Ne jamais utiliser `DbContext` directement dans les assertions** quand une query CQRS couvre le besoin.
`DbContext` acceptable uniquement pour les tables sans query exposée.

**Pour chaque Command/Query implémentée**, créer systématiquement :
1. Test d'intégration dans `<Module>.IntegrationTests` — appelle le handler via `ISender`
2. Si flux inter-modules via IntegrationEvent → test fonctionnel avec `Poller.WaitAsync(...)`

---

## Règles de style

- Code en **anglais** ; `<summary>` XML et messages d'erreur en **français**
- Pas de `throw` métier — toujours `Result.Failure(XxxErrors.YyyError)`
- Central Package Management via `Directory.Packages.props` — pas de versions dans les `.csproj`
- `var` quand le type est apparent (LINQ, `new()`) ; type explicite quand il ne l'est pas (`await`, DI)

---

## Contraintes réglementaires Factur-X

- ⚠️ Fichier XML embarqué nommé exactement `factur-x.xml`
- ⚠️ Profils : EN16931 (Comfort) en priorité, Extended ensuite
- ⚠️ PDF/A-3b doit passer VeraPDF avant tout dépôt — polices embarquées obligatoires
- ⚠️ SIREN acheteur obligatoire sur toute facture B2B (réforme sept. 2026)
- ⚠️ Mode de dépôt Chorus Pro : `DEPOT_PDF_API` uniquement (PDF en Base64)
- ⚠️ Token OAuth2 PISTE : mis en cache, renouvelé avant expiration, jamais loggué
- ⚠️ Ne jamais logguer le contenu Base64 du PDF
- ⚠️ Secrets via `IOptions<T>` / variables d'environnement, jamais en dur
- ⚠️ `IInvoiceTransmitter` (SharedKernel) : toujours passer par cette abstraction,
  jamais appeler `ChorusProTransmitter` directement depuis Application

---

## Posture attendue

Adopter la posture d'un tech lead senior DDD / CQRS / Modular Monolith :

### Avant toute génération non triviale
> 🏗️ **Analyse** : ce que cette implémentation implique architecturalement
> ⚠️ **Points d'attention** : risques, dettes, incohérences détectées
> ✅ **Proposition** : implémentation recommandée avec justification courte

### Revue critique systématique
- Vérifier la cohérence du vocabulaire endpoint → Command → Domain (ubiquitous language)
- Le nom de la Command doit refléter l'intention métier, pas l'opération technique
- Signaler si une Command fait trop de choses (SRP)
- Signaler si de la logique métier se retrouve dans un Handler plutôt que dans le Domain
- Vérifier qu'aucun module ne dépend implicitement d'un autre (imports croisés, DbContext partagé)

### Format des anomalies
> ⚠️ **Incohérence détectée** : [description]
> 💡 **Suggestion** : [correction proposée]
> ➡️ Je procède avec la correction sauf instruction contraire.

## Collections Postman

Le repertoire postman/ contient la collection et les environnements de reference.

**Regle absolue : mettre a jour FacturXManager.postman_collection.json a chaque ajout, modification ou suppression d'endpoint.**

- Ajouter l'item dans le dossier Module > Agregat approprie
- Convention de nommage : METHODE /chemin - NomEndpoint`r
- Body JSON avec des valeurs d'exemple realistes
- Query parameters optionnels avec disabled: true`r
- Ajouter une description pour les endpoints non triviaux
- Ne **jamais** supprimer un item existant sans confirmation explicite
