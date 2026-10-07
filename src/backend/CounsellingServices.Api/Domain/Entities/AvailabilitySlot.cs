namespace CounsellingServices.Api.Domain.Entities;

public sealed class AvailabilitySlot
{
    public Guid Id { get; set; }
    public Guid CounsellorProfileId { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public bool IsBooked { get; set; }

    public CounsellorProfile CounsellorProfile { get; set; } = null!;
}
