using FacturXManager.Modules.Transmission.Application.Transmissions.SubmitTransmission;

namespace FacturXManager.Modules.Transmission.IntegrationTests.Abstractions.Builders;

/// <summary>
/// Builder fluent pour <see cref="SubmitTransmissionCommand"/>. Valeurs par défaut : Chorus Pro,
/// SIRET destinataire valide, PDF base64 fictif.
/// </summary>
public sealed class SubmitTransmissionCommandBuilder
{
    private string _documentReference = "COMPLIANCE-DOC-2026-001";
    private string _recipientSiret = "12345678901234";
    private string _channel = "ChorusPro";
    private string _pdfBase64 = "JVBERi0xLjcK"; // "%PDF-1.7" minimal
    private string? _executingService;

    public SubmitTransmissionCommandBuilder WithDocumentReference(string reference)
    {
        _documentReference = reference;
        return this;
    }

    public SubmitTransmissionCommandBuilder WithRecipientSiret(string siret)
    {
        _recipientSiret = siret;
        return this;
    }

    public SubmitTransmissionCommandBuilder WithChannel(string channel)
    {
        _channel = channel;
        return this;
    }

    public SubmitTransmissionCommandBuilder WithPdfBase64(string pdfBase64)
    {
        _pdfBase64 = pdfBase64;
        return this;
    }

    public SubmitTransmissionCommandBuilder WithExecutingService(string? service)
    {
        _executingService = service;
        return this;
    }

    public SubmitTransmissionCommand Build() =>
        new(_documentReference, _recipientSiret, _channel, _pdfBase64, _executingService);
}
