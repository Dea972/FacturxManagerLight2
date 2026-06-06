namespace FacturXManager.BackOffice.Web.Common;

/// <summary>
/// Résultat léger côté front : encapsule succès/échec et un message d'erreur lisible.
/// Découple le frontend de FacturXManager.Common.Domain (logique de vue uniquement).
/// </summary>
public sealed record Result
{
    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string? Error { get; }

    /// <summary>Message d'erreur garanti non-null — à utiliser uniquement après avoir vérifié IsFailure.</summary>
    public string ErrorMessage => Error ?? string.Empty;

    public static Result Success() => new(true, null);

    public static Result Failure(string error) => new(false, error);
}

/// <summary>
/// Résultat léger côté front portant une valeur en cas de succès.
/// </summary>
public sealed record Result<TValue>
{
    private Result(bool isSuccess, TValue? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public TValue? Value { get; }

    public string? Error { get; }

    /// <summary>Message d'erreur garanti non-null — à utiliser uniquement après avoir vérifié IsFailure.</summary>
    public string ErrorMessage => Error ?? string.Empty;

    public static Result<TValue> Success(TValue value) => new(true, value, null);

    public static Result<TValue> Failure(string error) => new(false, default, error);
}
