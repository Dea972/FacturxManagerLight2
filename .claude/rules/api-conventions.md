# Conventions API et clients HTTP

## Endpoints Minimal API (Presentation)

- Un fichier par use case : `internal sealed class <UseCase> : IEndpoint`
- La request est un `sealed record` déclaré **à l'intérieur** de la classe endpoint
- Mapping manuel request → Command dans `MapEndpoint`, jamais de AutoMapper ici
- Pattern standard de réponse :
  ```csharp
  Result<Guid> result = await sender.Send(command);
  return result.Match(Results.Ok, ApiResults.Problem);
  ```
- `.WithTags(Tags.<Module>)` sur chaque endpoint
- `.WithDescription(...)` en français pour les endpoints non triviaux
- Déclarer `.Produces<T>()` et `.Produces<ProblemDetails>()` sur chaque route

## Clients Refit (Infrastructure)

- Une interface Refit par API externe
- Séparer systématiquement lecture et écriture : `IXxxApiRead` / `IXxxApiWrite`
- Placés dans `<Module>.Infrastructure/<ExternalSystem>/`
- Toujours typés avec des modèles dédiés — pas de `dynamic` ni `JObject`
- Enregistrement `BaseAddress` via méthodes d'extension centralisées dans Common.Infrastructure
  Ne pas dupliquer la logique de configuration dans chaque module

## OAuth2 / PISTE (Chorus Pro)

- Token mis en cache via `IMemoryCache`, renouvelé **60 secondes avant expiration**
- Implémenté comme `DelegatingHandler` injecté dans le `HttpClient` Refit
- Options via `IOptions<PisteAuthOptions>` — jamais de secret en dur
- Ne **jamais** logguer le token, ni le contenu Base64 du PDF

## Logs Serilog sur les appels externes

Obligatoire sur chaque appel HTTP sortant (Chorus Pro, PISTE) :
- Avant l'appel : méthode + URL (sans données sensibles)
- Après succès : code HTTP + durée en ms
- Après échec : code HTTP + message d'erreur + contexte de corrélation (`InvoiceId`, etc.)

## Gestion des erreurs HTTP

- Codes 4xx → `Result.Failure` avec `ErrorType.Validation` ou `ErrorType.NotFound`
- Codes 5xx → `Result.Failure` avec `ErrorType.ExternalService`
- Timeout → `Result.Failure` avec `ErrorType.Timeout`
- Jamais de `HttpRequestException` qui remonte jusqu'au handler CQRS

## Migrations EF Core

Ne **jamais** exécuter automatiquement. Proposer et attendre confirmation.
Deux commandes sur **deux lignes distinctes** (ne pas chaîner avec `&&`) :

```
dotnet ef migrations add <NomMigration> \
  --project src/Modules/<Module>/FacturXManager.Modules.<Module>.Infrastructure \
  --startup-project src/Api/FacturXManager.Api \
  --context <Module>DbContext

dotnet ef migrations script --idempotent \
  --project src/Modules/<Module>/FacturXManager.Modules.<Module>.Infrastructure \
  --startup-project src/Api/FacturXManager.Api \
  --context <Module>DbContext \
  -o src/Modules/<Module>/FacturXManager.Modules.<Module>.Infrastructure/Database/Scripts/AlterTable.sql
```

Le script `AlterTable.sql` est **écrasé** à chaque appel — prévenir de sauvegarder avant exécution.
