using FacturXManager.Common.Domain;
using FacturXManager.Modules.Transmission.Domain.Transmissions;
using FacturXManager.Modules.Transmission.UnitTests.Abstractions;
using FluentAssertions;
using DomainTransmission = FacturXManager.Modules.Transmission.Domain.Transmissions.Transmission;

namespace FacturXManager.Modules.Transmission.UnitTests.Transmissions;

public class TransmissionTests : BaseTest
{
    private static readonly DateTime Now = new(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc);

    private static Result<DomainTransmission> CreateValid(TransmissionChannel channel = TransmissionChannel.ChorusPro) =>
        DomainTransmission.Create("DOC-2026-001", "12345678901234", channel, Now, "EXEC-SERVICE");

    [Fact]
    public void Create_ShouldFail_WhenDocumentReferenceIsEmpty()
    {
        // Act
        Result<DomainTransmission> result = DomainTransmission.Create("", "12345678901234", TransmissionChannel.ChorusPro, Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransmissionErrors.DocumentReferenceRequired);
    }

    [Fact]
    public void Create_ShouldFail_WhenRecipientSiretIsEmpty()
    {
        // Act
        Result<DomainTransmission> result = DomainTransmission.Create("DOC-1", "", TransmissionChannel.ChorusPro, Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransmissionErrors.RecipientSiretRequired);
    }

    [Fact]
    public void Create_ShouldReturnPendingStatus()
    {
        // Act
        DomainTransmission transmission = CreateValid().Value;

        // Assert
        transmission.Status.Should().Be(TransmissionStatus.Pending);
        transmission.Attempts.Should().BeEmpty();
        transmission.ChannelDepositId.Should().BeNull();
    }

    [Fact]
    public void RecordSubmission_ShouldTransitionToSubmitted_AndAppendAttempt()
    {
        // Arrange
        DomainTransmission transmission = CreateValid().Value;

        // Act
        Result result = transmission.RecordSubmission("DEPOT-42", rawResponse: "ok", occurredOnUtc: Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        transmission.Status.Should().Be(TransmissionStatus.Submitted);
        transmission.ChannelDepositId.Should().Be("DEPOT-42");
        transmission.Attempts.Should().ContainSingle(a => a.Status == TransmissionStatus.Submitted);
    }

    [Fact]
    public void RecordRejection_ShouldTransitionToRejected_AndStoreReason()
    {
        // Arrange
        DomainTransmission transmission = CreateValid().Value;

        // Act
        Result result = transmission.RecordRejection("SIRET destinataire invalide", "raw", Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        transmission.Status.Should().Be(TransmissionStatus.Rejected);
        transmission.RejectionReason.Should().Be("SIRET destinataire invalide");
    }

    [Fact]
    public void RecordRejection_ShouldFail_WhenAlreadyAccepted()
    {
        // Arrange
        DomainTransmission transmission = CreateValid().Value;
        transmission.RecordSubmission("DEPOT-1", null, Now);
        transmission.RecordAcceptance(null, Now.AddMinutes(1));

        // Act
        Result result = transmission.RecordRejection("oups", null, Now.AddMinutes(2));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransmissionErrors.AlreadyFinalized);
    }

    [Fact]
    public void RecordTechnicalFailure_ShouldAppendAttempt_AndUpdateLastUpdate()
    {
        // Arrange
        DomainTransmission transmission = CreateValid().Value;
        DateTime later = Now.AddSeconds(30);

        // Act
        transmission.RecordTechnicalFailure("timeout réseau", "raw", later);

        // Assert
        transmission.Status.Should().Be(TransmissionStatus.TechnicalFailure);
        transmission.LastUpdatedOnUtc.Should().Be(later);
        transmission.Attempts.Should().ContainSingle(a => a.Status == TransmissionStatus.TechnicalFailure);
    }

    [Fact]
    public void Attempts_ShouldAppend_InChronologicalOrder()
    {
        // Arrange
        DomainTransmission transmission = CreateValid().Value;

        // Act : retry pattern (échec technique puis succès)
        transmission.RecordTechnicalFailure("indispo PISTE", null, Now);
        transmission.RecordSubmission("DEPOT-99", null, Now.AddMinutes(5));

        // Assert
        transmission.Attempts.Should().HaveCount(2);
        transmission.Attempts[0].Status.Should().Be(TransmissionStatus.TechnicalFailure);
        transmission.Attempts[1].Status.Should().Be(TransmissionStatus.Submitted);
        transmission.Status.Should().Be(TransmissionStatus.Submitted);
    }
}
