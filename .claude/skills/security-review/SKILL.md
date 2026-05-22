---
name: security-review
description: >
  S'active automatiquement quand une tâche touche à l'authentification OAuth2 PISTE,
  aux secrets de configuration, aux appels HTTP externes (Chorus Pro), ou à toute
  donnée sensible (SIREN, données fiscales, tokens). Fournit les règles de sécurité
  à appliquer et les patterns à vérifier.
allowed-tools: [Read, Grep]
---

# Skill : sécurité FacturX Manager

## Secrets et configuration

- **Jamais** de secret, token, mot de passe ou clé API en dur dans le code
- Tous les secrets via `IOptions<T>` chargé depuis `appsettings` / variables d'environnement
- Pattern obligatoire pour les options sensibles :
  ```csharp
  services.Configure<PisteAuthOptions>(configuration.GetSection("Piste:Auth"));
  // PisteAuthOptions.ClientSecret lu depuis env var ou secrets manager
  ```
- Vérifier qu'aucun secret n'est loggué (Serilog structured logging peut capturer des objets entiers)

## Token OAuth2 PISTE

- Cache via `IMemoryCache` avec expiration = durée du token - 60 secondes
- Implémenté comme `DelegatingHandler` — un seul point de renouvellement
- `SemaphoreSlim` pour éviter les race conditions sous charge concurrente
- Ne **jamais** logguer le token, même partiellement
- En cas d'échec de renouvellement → `Result.Failure` avec `ErrorType.ExternalService`, pas d'exception nue

## Données sensibles dans les logs

Ne jamais logguer :
- Le contenu Base64 du PDF (volume + données comptables confidentielles)
- Le token OAuth2 PISTE
- Les numéros SIREN/SIRET dans les logs de niveau Debug ou Info
- Les montants de factures dans les logs non structurés

Pattern sûr :
```csharp
// ✅ Correct — corrélation par ID uniquement
_logger.LogInformation("Dépôt Chorus Pro initié pour la facture {InvoiceId}", invoice.Id);

// ❌ Incorrect — données sensibles exposées
_logger.LogInformation("Dépôt pour {Siren} montant {Amount}", siren, amount);
```

## Environnements Chorus Pro

- Sandbox  : `sandbox-api.aife.economie.gouv.fr` / `qualif-chorus-pro.gouv.fr`
- Production : `api.aife.economie.gouv.fr` / `chorus-pro.gouv.fr`
- La configuration d'environnement doit être une option (`IOptions<ChorusProOptions>`)
- Jamais d'URL en dur dans le code — toujours via configuration

## Vérifications à effectuer lors d'une review sécurité

- [ ] Aucun secret en dur (grep : `password`, `apikey`, `clientsecret`, `bearer`)
- [ ] `PisteAuthOptions` chargées depuis configuration, pas instanciées en dur
- [ ] `DelegatingHandler` présent pour le renouvellement automatique du token
- [ ] Contenu Base64 du PDF non loggué
- [ ] URLs Chorus Pro/PISTE configurables, pas en dur
- [ ] `IInvoiceTransmitter` utilisé depuis Application — jamais `ChorusProTransmitter` directement
