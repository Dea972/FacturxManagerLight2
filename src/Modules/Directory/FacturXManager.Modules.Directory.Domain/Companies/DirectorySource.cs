namespace FacturXManager.Modules.Directory.Domain.Companies;

/// <summary>
/// Source de l'information annuaire. Permettra de distinguer les résolutions SIRENE
/// du futur annuaire PPF (plateforme agréée de rattachement).
/// </summary>
public enum DirectorySource
{
    Sirene = 0,
    Ppf = 1,
}
