namespace CounsellingServices.Api.DTOs;

public sealed record CounsellorSlotResponse(Guid SlotId, DateTimeOffset StartsAt, DateTimeOffset EndsAt);

public sealed record CounsellorResponse(Guid CounsellorProfileId, string Name, string Specialty, IReadOnlyList<CounsellorSlotResponse> AvailableSlots);
