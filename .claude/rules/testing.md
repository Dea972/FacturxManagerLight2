# Stratégie de tests

## Framework
- xUnit + FluentAssertions + Bogus
- Un projet de test par module

## Types de projets

| Type         | Suffix               | Frameworks                              |
|--------------|----------------------|-----------------------------------------|
| Unitaires    | `.UnitTests`         | xUnit, FluentAssertions, Bogus          |
| Intégration  | `.IntegrationTests`  | xUnit, FluentAssertions, Testcontainers |
| Architecture | `.ArchitectureTests` | NetArchTest                             |

## Nomenclature
```
<Méthode>_Should<Résultat>_When<Condition>
ex: Handle_ShouldReturnNotFound_WhenInvoiceDoesNotExist
```

## Structure des tests
Toujours commenter les trois sections :
```csharp
// Arrange
// Act
// Assert
```

## BaseIntegrationTest
Expose : `Faker` (Bogus), `Sender` (ISender), `ServiceProvider`, `DbContext` du module.
`IntegrationTestWebAppFactory` : Testcontainers SQL Server, méthode `SeedXxxAsync()` pour les données de base nécessaires aux tests.

## CommandHelpers / QueryHelpers
Chaque module de tests d'intégration expose deux classes d'extension sur `ISender` :
- `CommandHelpers` (`Abstractions/CommandHelpers.cs`) — commandes récurrentes, assertion `IsSuccess` intégrée
- `QueryHelpers` (`Abstractions/QueryHelpers.cs`) — queries récurrentes, assertion `IsSuccess` intégrée

Pattern obligatoire :
```csharp
public static async Task<TResponse> GetXxxAsync(this ISender sender, Guid id)
{
    Result<TResponse> result = await sender.Send(new GetXxxQuery(id));
    result.IsSuccess.Should().BeTrue(because: $"la récupération doit réussir, erreur : {result.Error.Description}");
    return result.Value;
}
```

## Règles
- **Ne jamais utiliser `DbContext` dans les assertions** quand une query CQRS couvre le besoin
  → Utiliser le helper `QueryHelpers` correspondant
  → `DbContext` acceptable uniquement pour les tables sans query exposée
- Pour vérifier une suppression : asserter `result.IsFailure` avec code `NotFound`, pas `AnyAsync`
- Les handlers CQRS sont testés via `ISender`, jamais via l'endpoint HTTP
- Toujours tester les cas `Result.Failure` autant que les cas nominaux

## Tests obligatoires à chaque Command/Query
Pour chaque Command ou Query implémentée :
1. **Test d'intégration** dans `<Module>.IntegrationTests` — via `ISender`
2. **Si flux inter-modules** via `IntegrationEvent` → test avec `Poller.WaitAsync(...)` pour les assertions asynchrones
