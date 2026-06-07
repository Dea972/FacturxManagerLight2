using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.Directory.Application.Users.LookupChorusProUser;

/// <summary>
/// Consulte un utilisateur Chorus Pro à partir de son login (adresse e-mail ou identifiant technique).
/// </summary>
public sealed record LookupChorusProUserQuery(string Login) : IQuery<ChorusProUserLookupResponse>;
