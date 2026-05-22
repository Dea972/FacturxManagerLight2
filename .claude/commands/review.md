Effectue une revue de code complète du module $ARGUMENTS (ou du fichier/dossier courant si non précisé).

## Vérifications architecturales

### Isolation inter-modules
- Aucun `using` vers les internals d'un autre module
- Les DomainEvents et IntegrationEvents ne transportent que des IDs (Guid) et types primitifs

### Patterns DDD
- Les agrégats protègent leurs invariants (`private set`, `static Create(...)`)
- `Raise()` uniquement dans l'agrégat, jamais depuis l'extérieur
- Logique métier dans le Domain, pas dans les Handlers

### Patterns CQRS
- Tous les handlers retournent `Result` ou `Result<T>`, jamais d'exception métier
- Validators FluentValidation uniquement sur les Commands, guards dans les QueryHandlers
- Suffixe `Response` uniquement sur les queries
- Aucune Command appelée depuis une autre Command

### Infrastructure
- Un seul DbContext par module
- `UseSnakeCaseNamingConvention()` présent
- Migrations jamais auto-exécutées

### Conventions de style
- Code en anglais, `<summary>` et messages d'erreur en français
- Propriétés des entités en `private set` ou `init`
- Pas de secrets en dur

### Réglementaire (si module Compliance ou Transmission)
- Fichier XML embarqué nommé `factur-x.xml`
- Token PISTE non loggué, renouvelé avant expiration
- `IInvoiceTransmitter` utilisé, jamais `ChorusProTransmitter` directement

## Format de sortie

Pour chaque problème :
- 🔴 **Bloquant** : violation d'une règle d'architecture ou réglementaire
- 🟡 **Important** : pattern non respecté, risque de dette technique
- 🟢 **Suggestion** : amélioration possible, non bloquante

Conclure par : **"Approuvé ✅"** ou **"Modifications requises ❌"** avec un résumé.
