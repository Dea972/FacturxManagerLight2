using Microsoft.Extensions.Configuration;

namespace FacturXManager.Sdk;

/// <summary>
/// Extensions pour charger les fichiers de configuration des modules.
/// </summary>
public static class ModuleConfigurationExtensions
{
    /// <summary>
    /// Ajoute les fichiers JSON de configuration de chaque module (modules.{module}.json).
    /// </summary>
    public static void AddModuleConfiguration(this IConfigurationBuilder configurationBuilder, string[] modules)
    {
        string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        foreach (string module in modules)
        {
            configurationBuilder.AddJsonFile($"modules.{module}.json", false, true);
            configurationBuilder.AddJsonFile($"modules.{module}.{environment}.json", true, true);
        }
    }
}
