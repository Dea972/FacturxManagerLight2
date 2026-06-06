using FacturXManager.Common.Application.Transmission;
using FacturXManager.Modules.Transmission.IntegrationTests.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace FacturXManager.Modules.Transmission.IntegrationTests.Transmissions;

/// <summary>
/// Vérifie que <c>ChorusProTransmitter</c> propage correctement le libellé d'erreur
/// retourné par Chorus Pro dans le <see cref="InvoiceTransmissionResult"/> — que l'erreur
/// soit dans le body HTTP ou dans un champ applicatif <c>libelle</c>.
/// </summary>
public class ChorusProTransmitterTests : BaseIntegrationTest
{
    private readonly IInvoiceTransmitter _transmitter;

    public ChorusProTransmitterTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _transmitter = ServiceProvider.GetRequiredService<IInvoiceTransmitter>();
    }

    [Fact]
    public async Task TransmitAsync_ShouldIncludeLibelle_WhenChorusProReturns400WithJsonBody()
    {
        // Arrange
        WireMock
            .Given(Request.Create().WithPath("/cpro/factures/v1/deposer/flux").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(400)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"codeRetour":1,"libelle":"SIRET destinataire inconnu de Chorus Pro"}"""));

        InvoiceTransmissionRequest request = BuildRequest();

        // Act
        InvoiceTransmissionResult result = await _transmitter.TransmitAsync(request);

        // Assert
        result.Outcome.Should().Be(TransmissionOutcome.Rejected);
        result.Message.Should().Contain("SIRET destinataire inconnu de Chorus Pro");
    }

    [Fact]
    public async Task TransmitAsync_ShouldIncludeLibelle_WhenChorusProReturns500WithJsonBody()
    {
        // Arrange
        WireMock
            .Given(Request.Create().WithPath("/cpro/factures/v1/deposer/flux").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(500)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"codeRetour":-1,"libelle":"Erreur interne du serveur Chorus Pro"}"""));

        InvoiceTransmissionRequest request = BuildRequest();

        // Act
        InvoiceTransmissionResult result = await _transmitter.TransmitAsync(request);

        // Assert
        result.Outcome.Should().Be(TransmissionOutcome.TechnicalFailure);
        result.Message.Should().Contain("Erreur interne du serveur Chorus Pro");
    }

    [Fact]
    public async Task TransmitAsync_ShouldIncludeLibelle_WhenChorusProReturnsApplicativeRejection()
    {
        // Arrange : HTTP 200 mais codeRetour != 0 — rejet applicatif Chorus Pro
        WireMock
            .Given(Request.Create().WithPath("/cpro/factures/v1/deposer/flux").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"codeRetour":42,"libelle":"Facture déjà déposée avec ce numéro de flux"}"""));

        InvoiceTransmissionRequest request = BuildRequest();

        // Act
        InvoiceTransmissionResult result = await _transmitter.TransmitAsync(request);

        // Assert
        result.Outcome.Should().Be(TransmissionOutcome.Rejected);
        result.Message.Should().Contain("Facture déjà déposée avec ce numéro de flux");
    }

    [Fact]
    public async Task TransmitAsync_ShouldReturnRawBody_WhenChorusProReturns401WithPlainText()
    {
        // Arrange : body non-JSON — fallback sur le raw body
        WireMock
            .Given(Request.Create().WithPath("/cpro/factures/v1/deposer/flux").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(401)
                .WithHeader("Content-Type", "text/plain")
                .WithBody("Unauthorized"));

        InvoiceTransmissionRequest request = BuildRequest();

        // Act
        InvoiceTransmissionResult result = await _transmitter.TransmitAsync(request);

        // Assert
        result.Outcome.Should().Be(TransmissionOutcome.Rejected);
        result.Message.Should().Contain("401");
    }

    private static InvoiceTransmissionRequest BuildRequest() => new(
        Channel: TransmissionChannel.ChorusPro,
        DocumentReference: "TEST-DOC-001",
        RecipientSiret: "12345678901234",
        FacturXPdfBase64: "JVBERi0xLjcK",
        ExecutingService: null);
}
