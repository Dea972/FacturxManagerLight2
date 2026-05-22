using FacturXManager.Common.Application.Clock;

namespace FacturXManager.Common.Infrastructure.Clock;
internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
