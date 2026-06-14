using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.Directory.Application.Structures.ConsultChorusProStructure;

/// <summary>
/// Retourne le détail d'une structure Chorus Pro à partir de son identifiant CPP.
/// </summary>
public sealed record ConsultChorusProStructureQuery(long IdStructureCpp)
    : IQuery<ChorusProStructureDetailResponse>;
