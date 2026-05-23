namespace FacturXManager.Common.Domain;

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    Problem = 2,
    NotFound = 3,
    Conflict = 4,

    /// <summary>Échec d'un service externe en amont (Chorus Pro, DGFiP, etc. — typiquement HTTP 5xx).</summary>
    ExternalService = 5,

    /// <summary>Temps d'attente dépassé sur un appel externe.</summary>
    Timeout = 6
}
