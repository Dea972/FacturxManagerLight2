# FacturX Manager

Application .NET 10 permettant à une entreprise française de générer des factures
électroniques conformes **Factur-X** (PDF/A-3b + XML CII embarqué) et de les transmettre
sur **Chorus Pro** via l'API PISTE (AIFE), dans le cadre de la réforme française de
facturation électronique obligatoire à partir du **1er septembre 2026**.

---

## Fonctionnalités

- **Certification Factur-X** — génération d'un XML CII conforme EN16931 (profil Comfort/Extended)
  embarqué dans un PDF/A-3b valide
- **Transmission Chorus Pro** — dépôt via l'API REST PISTE avec authentification OAuth2,
  gestion des statuts et des rejets
- **E-Reporting DGFiP** *(à venir)* — déclaration périodique des transactions B2C et internationales

---

## Stack technique

| Composant | Technologie |
|-----------|-------------|
| Framework | .NET 10 / C# |
| Architecture | Modulaire monolith DDD + CQRS |
| Mediator | MediatR |
| Validation | FluentValidation |
| HTTP clients | Refit |
| Logs | Serilog |
| ORM | EF Core 10 |
| Base de données | SQL Server |
| Mapping | Riok.Mapperly |
| Tests | xUnit + FluentAssertions + NSubstitute + WireMock.Net |

---

## Architecture

```
src/
├── Modules/
│   ├── Compliance/          # Validation EN16931, génération XML CII + PDF/A-3b
│   ├── Transmission/        # Acheminement Chorus Pro / PDP / PPF
│   └── EReporting/          # Reporting B2C / international DGFiP (futur)
├── Common/                  # SharedKernel : Result<T>, value objects, abstractions
└── Api/                     # Point d'entrée ASP.NET Core
tests/
├── Modules/
│   ├── Compliance/
│   │   ├── Compliance.UnitTests
│   │   ├── Compliance.IntegrationTests
│   │   └── Compliance.ArchitectureTests
│   ├── Transmission/
│   │   ├── Transmission.UnitTests
│   │   ├── Transmission.IntegrationTests
│   │   └── Transmission.ArchitectureTests
│   └── EReporting/
│       ├── EReporting.UnitTests
│       ├── EReporting.IntegrationTests
│       └── EReporting.ArchitectureTests
postman/
├── FacturXManager.postman_collection.json
├── FacturXManager.Local.postman_environment.json
├── FacturXManager.Sandbox.postman_environment.json  ← gitignored
└── FacturXManager.Production.postman_environment.json ← gitignored
```

---

## Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local ou Docker)
- Accès API PISTE (sandbox ou production) — [portail AIFE](https://developer.aife.economie.gouv.fr/)

---

## Démarrage rapide

### 1. Cloner le projet

```bash
git clone https://github.com/votre-org/facturx-manager.git
cd facturx-manager
```

### 2. Configuration

Copier le fichier de configuration et renseigner les valeurs :

```bash
cp src/Api/appsettings.Example.json src/Api/appsettings.Development.json
```

Variables obligatoires :

```json
{
  "ConnectionStrings": {
    "Database": "Server=localhost;Database=FacturXManager;Trusted_Connection=true;"
  },
  "Piste": {
    "Auth": {
      "TokenEndpoint": "https://sandbox-api.aife.economie.gouv.fr/token",
      "ClientId": "VOTRE_CLIENT_ID",
      "ClientSecret": "VOTRE_CLIENT_SECRET"
    }
  },
  "ChorusPro": {
    "BaseUrl": "https://qualif-chorus-pro.gouv.fr"
  }
}
```

> ⚠️ Ne jamais committer `appsettings.Development.json` ni aucun fichier contenant des secrets.

### 3. Build et lancement

```bash
dotnet build
dotnet run --project src/Api/FacturXManager.Api
```

L'API est accessible sur `https://localhost:7000`.
Swagger UI : `https://localhost:7000/swagger`

---

## Tests

```bash
# Tous les tests
dotnet test

# Par type
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=Architecture"
```

---

## Environnements Chorus Pro

| Environnement | API PISTE | Chorus Pro |
|---------------|-----------|------------|
| Sandbox | `sandbox-api.aife.economie.gouv.fr` | `qualif-chorus-pro.gouv.fr` |
| Production | `api.aife.economie.gouv.fr` | `chorus-pro.gouv.fr` |

L'environnement cible se configure via `ChorusPro:BaseUrl` et `Piste:Auth:TokenEndpoint`.

---

## Réforme facturation électronique 2026

| Échéance | Périmètre |
|----------|-----------|
| 1er septembre 2026 | Grandes entreprises et ETI — **émission et réception** obligatoires |
| 1er septembre 2027 | PME, TPE, micro-entreprises |

Champs devenus obligatoires avec la réforme :
- SIREN de l'acheteur
- Nature de l'opération
- Adresse de livraison (si différente de l'adresse de facturation)
- Mention TVA sur encaissement si applicable

---

## Contribution

Les conventions de développement sont dans [`CLAUDE.md`](./CLAUDE.md) et [`copilot-instructions.md`](.github/copilot-instructions.md).
