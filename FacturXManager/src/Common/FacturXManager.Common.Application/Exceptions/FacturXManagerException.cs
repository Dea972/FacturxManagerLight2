using FacturXManager.Common.Domain;

namespace FacturXManager.Common.Application.Exceptions;

public sealed class FacturXManagerException : Exception
{
    public FacturXManagerException(string requestName, Error? error = default, Exception? innerException = default)
        : base("Application exception", innerException)
    {
        RequestName = requestName;
        Error = error;
    }

    public string RequestName { get; }

    public Error? Error { get; }
}
