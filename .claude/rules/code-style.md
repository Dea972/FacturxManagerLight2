# Conventions de code

## Langue
- Code (classes, méthodes, variables, propriétés) : **anglais**
- `<summary>` XML, messages d'erreur, logs métier : **français**

## Nommage
- Agrégat : nom du concept métier, pas de suffixe technique (`Invoice`, pas `InvoiceEntity`)
- Command : `{Verbe}{Nom}Command` — ex : `GenerateInvoiceCommand`
- Query : `Get{Nom}Query` / `List{Nom}Query`
- QueryHandler response : `{UseCase}Response` — suffixe **réservé aux queries**
- Command summary (si T structuré) : `{UseCase}Summary`, jamais `{UseCase}Response`
- Domain Event : `{Sujet}{Action}DomainEvent` — ex : `InvoiceGeneratedDomainEvent`
- Integration Event : `{Sujet}{Action}IntegrationEvent`
- Handler : `{CommandName}Handler` / `{QueryName}Handler`
- Service partagé : suffixe `Service`, jamais `Orchestrator`

## Typage des Commands
- Propriétés d'une Command : types simples uniquement (`string`, `Guid`, `decimal`, `int`, `bool`, `enum`)
- **Jamais de Value Objects du Domain dans une Command** — le handler instancie les VOs à partir des scalaires
- La frontière DTO/type complexe appartient à la couche Presentation (request bodies), pas Application

## Résultats et erreurs
- **Jamais** de `throw` métier dans Application ou Domain
- Toujours `Result.Failure(XxxErrors.YyyError)` depuis les use cases
- Exceptions autorisées uniquement dans Infrastructure (I/O, réseau), immédiatement wrappées

## Structure fichiers
- Un fichier = une classe / interface / record
- Pas de fichiers fourre-tout (`Helpers.cs`, `Extensions.cs` génériques)
- `sealed record` immuable pour les value objects, DTOs, Commands, Queries

## `var` vs type explicite
- `var` quand le type est apparent : LINQ, `new()`, méthodes au nom explicite
- Type explicite quand il ne l'est pas : `await`, résolution DI, factories

## Central Package Management
- Versions déclarées dans `Directory.Packages.props` uniquement
- Ne jamais spécifier de version dans les `.csproj`
