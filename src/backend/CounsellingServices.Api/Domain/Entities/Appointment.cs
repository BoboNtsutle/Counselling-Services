namespace CounsellingServices.Api.Domain.Entities;

public sealed class Appointment
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid CounsellorProfileId { get; set; }
    public Guid AvailabilitySlotId { get; set; }
    public AppointmentStatus Status { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public User Student { get; set; } = null!;
    public CounsellorProfile CounsellorProfile { get; set; } = null!;
    public AvailabilitySlot AvailabilitySlot { get; set; } = null!;
}
