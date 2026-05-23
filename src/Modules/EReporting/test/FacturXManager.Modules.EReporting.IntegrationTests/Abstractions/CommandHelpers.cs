using FacturXManager.Common.Domain;
using FacturXManager.Modules.EReporting.Application.Declarations.AddDeclarationEntry;
using FacturXManager.Modules.EReporting.Application.Declarations.CreateDeclaration;
using FacturXManager.Modules.EReporting.Application.Declarations.FinalizeDeclaration;
using FluentAssertions;
using MediatR;

namespace FacturXManager.Modules.EReporting.IntegrationTests.Abstractions;

/// <summary>
/// Extensions sur <see cref="ISender"/> pour les commandes récurrentes du module EReporting.
/// Chaque helper asserte <c>IsSuccess</c> afin de simplifier le code des tests qui en dépendent.
/// </summary>
internal static class CommandHelpers
{
    public static async Task<Guid> CreateDeclarationAsync(
        this ISender sender,
        int year = 2026,
        int month = 9,
        string transactionType = "B2C")
    {
        Result<Guid> result = await sender.Send(new CreateDeclarationCommand(year, month, transactionType));
        result.IsSuccess.Should().BeTrue(
            because: $"la création doit réussir, erreur : {(result.IsFailure ? result.Error.Description : string.Empty)}");
        return result.Value;
    }

    public static async Task AddEntryAsync(
        this ISender sender,
        Guid declarationId,
        decimal taxableAmount = 100m,
        decimal vatAmount = 20m,
        string countryCode = "FR",
        string operationNature = "Goods")
    {
        Result result = await sender.Send(new AddDeclarationEntryCommand(
            declarationId, taxableAmount, vatAmount, countryCode, operationNature));
        result.IsSuccess.Should().BeTrue(
            because: $"l'ajout d'une transaction doit réussir, erreur : {(result.IsFailure ? result.Error.Description : string.Empty)}");
    }

    public static async Task FinalizeAsync(this ISender sender, Guid declarationId)
    {
        Result result = await sender.Send(new FinalizeDeclarationCommand(declarationId));
        result.IsSuccess.Should().BeTrue(
            because: $"la finalisation doit réussir, erreur : {(result.IsFailure ? result.Error.Description : string.Empty)}");
    }
}
