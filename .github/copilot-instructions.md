# Instructions Copilot — FacturX Manager

## 1. Vue d'ensemble du projet

Application .NET 10 / C# de génération et transmission de factures électroniques
au format Factur-X (PDF/A-3b + XML CII embarqué), conformes EN16931, vers Chorus Pro
via l'API PISTE (AIFE). Réforme française de facturation électronique obligatoire
à partir du 1er septembre 2026.

---

## 2. Architecture

**Modulaire monolith** + **DDD** + **CQRS** (sans Event Sourcing)

Chaque module est autonome : `Domain` / `Application` / `Infrastructure` / `Presentation`

Shared Kernel : `FacturXManager.Common.*` (Domain, Application, Infrastructure)

Communication inter-modules : Outbox/Inbox + MassTransit — **uniquement si nécessaire**

---

## 3. Structure des modules

Chaque module suit strictement cette arborescence de projets :

```
FacturXManager.Modules.<Module>.Domain/
FacturXManager.Modules.<Module>.Application/
FacturXManager.Modules.<Module>.Infrastructure/
FacturXManager.Modules.<Module>.Presentation/
FacturXManager.Modules.<Module>.IntegrationEvents/  ← uniquement si communication inter-modules
```

Structure interne :
```
<Module>.Domain/
└── <Agrégat>/
    ├── <Agrégat>.cs
    ├── <Agrégat>Errors.cs
    └── <Sujet><Action>DomainEvent.cs   ← uniquement si nécessaire

<Module>.Application/
└── <UseCase>/                           ← un dossier par use case, pas de Commands/ ou Queries/
    ├── <UseCase>Command.cs
    ├── <UseCase>CommandHandler.cs
    ├── <UseCase>CommandValidator.cs
    ├── <UseCase>Query.cs
    ├── <UseCase>QueryHandler.cs
    └── <UseCase>Response.cs             ← suffixe Response réservé aux queries

<Module>.Infrastructure/
├── Database/
│   ├── <Module>DbContext.cs
│   ├── Schemas.cs
│   └── Migrations/
├── <Agrégat>/
│   ├── <Entité>Configuration.cs
│   ├── <Entité>Repository.cs
│   └── <Entité>Service.cs              ← si logique partagée entre Commands
└── <Module>Module.cs

<Module>.Presentation/
└── <Agrégat>/
    └── <UseCase>.cs                     ← internal sealed class : IEndpoint
```

---

## 4. Conventions DDD

- **Agrégat** : `sealed partial class`, constructeur `private`, factory `static Create(...)`
- **Entité** : hérite de `Entity` (events via `Raise()`) ou `AuditableEntity`
- **Value Object** : hérite de `ValueObject`, implémente `GetEqualityComponents()`
- **Domain Event** : hérite de `DomainEvent`, nommé `<Sujet><Action>DomainEvent` — créer uniquement si nécessaire
- **Erreurs** : classe statique `<Entité>Errors` avec champs `static readonly Error`
- `Raise()` uniquement dans l'agrégat lui-même, jamais depuis l'extérieur
- Propriétés : `private set` ou `init`, jamais de setter public

---

## 5. Conventions CQRS (MediatR)

- Interfaces : `ICommand`, `ICommand<T>`, `IQuery<T>` — handlers via `ICommandHandler` / `IQueryHandler`
- Tous retournent `Result` ou `Result<T>` — **jamais d'exception métier**
- **Validators FluentValidation uniquement sur les `ICommand` / `ICommand<T>`** — jamais sur les `IQuery<T>`
  Le `ValidationPipelineBehavior` est filtré sur `IBaseCommand` et n'intercepte pas les queries.
  Pour les queries : **guards explicites** dans le handler (`if` + `Result.Failure`) avec `<Entité>Errors`
- **Ne jamais appeler une Command dans une autre Command**
  Logique partagée → extraire dans un Service (interface Application, implémentation Infrastructure)
  Les orchestrateurs/services partagés sont suffixés `Service`, pas `Orchestrator`
- **Suffixe `Response` réservé aux queries**
  Une Command retourne `Result` ou `Result<T>` directement.
  Si T est structuré → nommer `<UseCase>Summary`, jamais `<UseCase>Response`
- **QueryHandlers toujours dans Application**, jamais dans Infrastructure
- Les interfaces déclarées dans Application ne doivent être consommées que dans Application
  Interface appelée uniquement dans Infrastructure → la déclarer `internal` dans Infrastructure
- Logging pipeline : `RequestLoggingPipelineBehavior` (Serilog) — déjà enregistré, ne pas recréer

---

## 6. Communication inter-modules

**Ne pas créer d'IntegrationEvents par défaut.** Uniquement quand la communication inter-modules est nécessaire.

Quand c'est nécessaire :
- `DomainEventHandler` publie un `IntegrationEvent` via `IEventBus`
- Bus in-memory MassTransit : intra-process / Azure Service Bus : externe
- `IntegrationEvent` : hérite de `IntegrationEvent`, nommé `<Sujet><Action>IntegrationEvent`
- Events dans un projet séparé `<Module>.IntegrationEvents`
- Outbox/Inbox : `InsertOutboxMessagesInterceptor` + jobs Quartz (`ProcessOutboxJob`)
- Ne jamais écrire de SQL inline pour les tables inbox/outbox — utiliser les repositories dédiés

---

## 7. Conventions Infrastructure

- **EF Core + SQL Server** — un `DbContext` par module, schéma SQL dédié (`Schemas.<Module>`)
- `UseSnakeCaseNamingConvention()` sur tous les DbContext
- `InsertOutboxMessagesInterceptor` ajouté sur chaque DbContext
- Configuration EF : une `IEntityTypeConfiguration<T>` par entité
- **Dapper** pour les lectures via `IDbConnectionFactory` quand pertinent
- DI : `AddXxxModuleAsync(services, config, ...)` dans `<Module>Module.cs` (méthode `async`)
  Enregistrement automatique des DomainEventHandlers et IntegrationEventHandlers par réflexion
  avec décoration `IdempotentDomainEventHandler<T>` / `IdempotentIntegrationEventHandler<T>`
- Mapping : **Riok.Mapperly**
- HTTP clients externes : **Refit**
  - Séparer interfaces lecture / écriture si applicable (`IXxxApiRead` / `IXxxApiWrite`)
  - Centraliser la configuration `BaseAddress` dans des méthodes d'extension communes
- **Migrations EF Core** : ne **jamais** exécuter automatiquement
  Proposer la commande et attendre confirmation. Deux lignes distinctes (ne pas chaîner avec `&&`) :

  ```
  dotnet ef migrations add <NomMigration> --project src/Modules/<Module>/FacturXManager.Modules.<Module>.Infrastructure --startup-project src/Api/FacturXManager.Api --context <Module>DbContext
  dotnet ef migrations script --idempotent --project src/Modules/<Module>/FacturXManager.Modules.<Module>.Infrastructure --startup-project src/Api/FacturXManager.Api --context <Module>DbContext -o src/Modules/<Module>/FacturXManager.Modules.<Module>.Infrastructure/Database/Scripts/AlterTable.sql
  ```

---

## 8. Conventions Presentation (Minimal API)

- Un fichier par use case : `internal sealed class <UseCase> : IEndpoint`
- La request est un `sealed record` déclaré **à l'intérieur** de la classe endpoint
- Mapping manuel de la request vers la Command dans `MapEndpoint`
- Pattern standard :
  ```csharp
  Result<Guid> result = await sender.Send(command);
  return result.Match(Results.Ok, ApiResults.Problem);
  ```
- **Les types Domain (enums, value objects) ne doivent jamais apparaître dans une Command**
  C'est le handler qui instancie les types Domain à partir des scalaires reçus
- Propriétés d'une Command : types simples (`string`, `Guid`, `decimal`, `int`, `bool`, `enum`)

---

## 9. Conventions de tests

| Type         | Projet suffix        | Frameworks                              |
|--------------|----------------------|-----------------------------------------|
| Unitaires    | `.UnitTests`         | xUnit, FluentAssertions, Bogus          |
| Intégration  | `.IntegrationTests`  | xUnit, FluentAssertions, Testcontainers |
| Architecture | `.ArchitectureTests` | NetArchTest                             |

- Structure : **Arrange / Act / Assert** (commenté)
- Nommage : `<Méthode>_Should<Résultat>_When<Condition>`
- `BaseIntegrationTest` expose : `Faker`, `Sender` (ISender), `ServiceProvider`, `DbContext`
- `IntegrationTestWebAppFactory` : Testcontainers SQL Server, méthode `SeedXxxAsync()` pour les données de base

### CommandHelpers / QueryHelpers

Chaque module de tests expose :
- `CommandHelpers` (`Abstractions/CommandHelpers.cs`) — méthodes d'extension sur `ISender`, assertion `IsSuccess` intégrée
- `QueryHelpers` (`Abstractions/QueryHelpers.cs`) — idem pour les queries

Pattern obligatoire :
```csharp
public static async Task<TResponse> GetXxxAsync(this ISender sender, ...)
{
    Result<TResponse> result = await sender.Send(new GetXxxQuery(...));
    result.IsSuccess.Should().BeTrue(because: $"... erreur : {result.Error.Description}");
    return result.Value;
}
```

**Ne jamais utiliser `DbContext` dans les assertions** quand une query CQRS couvre le besoin.
Pour vérifier une suppression → asserter `result.IsFailure` avec code `NotFound`, pas `AnyAsync`.

### Tests obligatoires à chaque Command/Query

Pour chaque Command ou Query implémentée :
1. **Test d'intégration** (`<Module>.IntegrationTests`) — appelle le handler via `ISender`, jamais via HTTP
2. **Si flux inter-modules** via `IntegrationEvent` → test fonctionnel avec `Poller.WaitAsync(...)`

---

## 10. Règles de style

- Code en **anglais** ; `<summary>` XML et messages d'erreur en **français**
- Pas de `throw` métier — toujours `Result.Failure(XxxErrors.YyyError)`
- Propriétés des entités : `private set` ou `init` — jamais de setter public
- Central Package Management via `Directory.Packages.props` — pas de versions dans les `.csproj`
- `var` quand le type est apparent (LINQ, `new()`, méthodes explicites) ; type explicite sinon (`await`, DI)

---

## 11. Contraintes réglementaires Factur-X

- ⚠️ Fichier XML embarqué nommé exactement `factur-x.xml` (sensible à la casse)
- ⚠️ Profils supportés : EN16931 (Comfort) en priorité, Extended ensuite
- ⚠️ PDF/A-3b doit passer VeraPDF avant tout dépôt — polices embarquées obligatoires
- ⚠️ SIREN acheteur obligatoire sur toute facture B2B (réforme sept. 2026)
- ⚠️ Champs obligatoires réforme : nature de l'opération, adresse de livraison si différente
- ⚠️ Mode de dépôt Chorus Pro : `DEPOT_PDF_API` uniquement (PDF Factur-X en Base64)
- ⚠️ Token OAuth2 PISTE : mis en cache via `IMemoryCache`, renouvelé 60s avant expiration
  Implémenté comme `DelegatingHandler`. Jamais loggué. Secrets via `IOptions<T>` uniquement
- ⚠️ Ne jamais logguer le contenu Base64 du PDF (volume + données sensibles)
- ⚠️ Logs Serilog obligatoires sur tous les appels externes (avant + après, avec durée)
- ⚠️ `IInvoiceTransmitter` (SharedKernel) : toujours passer par cette abstraction,
  jamais appeler `ChorusProTransmitter` directement depuis Application

---

## 12. Revue critique systématique

Avant toute génération ou modification, analyser le contexte et signaler :

### Cohérence sémantique bout-en-bout
- Vérifier l'alignement vocabulaire : endpoint → Command → Domain (ubiquitous language)
- Le nom de la Command doit refléter l'intention métier, pas l'opération technique
- Les erreurs dans `<Entité>Errors` utilisent le vocabulaire du Domain, pas de l'infra

### Appartenance et responsabilités
- Vérifier si la feature appartient vraiment au module ciblé
- Signaler si une Command fait trop de choses (SRP) → proposer de découper
- Logique métier dans un Handler → doit aller dans le Domain

### Signaux d'alerte
- Un `IntegrationEvent` qui transporte trop de données → couplage déguisé
- Une validation FluentValidation qui recoupe un invariant du Domain → duplication
- Un module qui commence à dépendre implicitement d'un autre (imports croisés, DbContext partagé)
- Un Value Object avec un seul champ → probablement inutile

### Format de retour

> ⚠️ **Incohérence détectée** : [description]
> 💡 **Suggestion** : [correction proposée]
> ➡️ Je procède avec la correction sauf instruction contraire.

---

## 13. Posture Tech Lead

- **Toujours penser à 6 mois** : ce code sera-t-il compréhensible et maintenable ?
- **Préférer l'explicite à l'implicite** : un nom de méthode se lit comme une phrase métier
- **Challenger poliment** : si la demande semble incohérente avec l'architecture, le dire avant d'exécuter
- **Proposer, pas imposer** : toute suggestion est présentée comme une option

### Format systématique sur toute génération non triviale

> 🏗️ **Analyse** : ce que cette implémentation implique architecturalement
> ⚠️ **Points d'attention** : risques, dettes, incohérences détectées
> ✅ **Proposition** : implémentation recommandée avec justification courte
