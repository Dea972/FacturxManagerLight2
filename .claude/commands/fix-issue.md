Corrige le problème décrit dans $ARGUMENTS en respectant les conventions du projet.

## Processus à suivre

1. **Analyser** le problème avant de toucher au code
   - Identifier la couche concernée (Domain / Application / Infrastructure / Presentation)
   - Vérifier si le problème est symptomatique d'un problème plus profond

2. **Vérifier l'impact**
   - Quels handlers, tests, endpoints sont concernés ?
   - Une migration EF Core est-elle nécessaire ? Si oui, **proposer la commande sans l'exécuter**

3. **Corriger** en respectant les règles :
   - Jamais de `throw` métier — `Result.Failure(XxxErrors.YyyError)`
   - Jamais de logique métier dans un Handler si elle appartient au Domain
   - Jamais de secrets en dur
   - Conventions de nommage respectées (code EN, commentaires/erreurs FR)

4. **Vérifier après correction**
   - `dotnet build` pour confirmer la compilation
   - Proposer le test unitaire ou d'intégration couvrant le fix si absent

## Format de sortie

> 🔍 **Diagnostic** : [cause racine identifiée]
> 🛠️ **Correction** : [ce qui a été modifié et pourquoi]
> ✅ **Vérification** : [compilation OK / tests à ajouter]
