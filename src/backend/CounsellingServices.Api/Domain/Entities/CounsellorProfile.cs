namespace CounsellingServices.Api.Domain.Entities;

public sealed class CounsellorProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;

    public User User { get; set; } = null!;
    public ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = new List<AvailabilitySlot>();
}
