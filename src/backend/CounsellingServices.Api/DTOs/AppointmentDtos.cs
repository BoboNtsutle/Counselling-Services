using CounsellingServices.Api.Domain.Entities;

namespace CounsellingServices.Api.DTOs;

public sealed record CreateAvailabilitySlotRequest(DateTimeOffset StartsAt, DateTimeOffset EndsAt);

public sealed record BookAppointmentRequest(Guid SlotId, string Reason);

public sealed record UpdateAppointmentStatusRequest(AppointmentStatus Status);

public sealed record AppointmentItem(
    Guid AppointmentId,
    Guid SlotId,
    string Counsellor,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    AppointmentStatus Status,
    string Reason);
