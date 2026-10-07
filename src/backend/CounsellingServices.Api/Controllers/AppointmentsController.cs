using CounsellingServices.Api.DTOs;
using CounsellingServices.Api.Domain.Entities;
using CounsellingServices.Api.Extensions;
using CounsellingServices.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CounsellingServices.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/appointments")]
public sealed class AppointmentsController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Student,Admin")]
    public async Task<IActionResult> Book(BookAppointmentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest("Reason is required.");
        }

        var slot = await dbContext.AvailabilitySlots
            .Include(x => x.CounsellorProfile)
            .SingleOrDefaultAsync(x => x.Id == request.SlotId, cancellationToken);

        if (slot is null || slot.IsBooked)
        {
            return Conflict("Slot is not available.");
        }

        if (slot.StartsAt <= DateTimeOffset.UtcNow)
        {
            return BadRequest("Only future slots can be booked.");
        }

        slot.IsBooked = true;
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            StudentId = User.GetUserId(),
            CounsellorProfileId = slot.CounsellorProfileId,
            AvailabilitySlotId = slot.Id,
            Reason = request.Reason.Trim(),
            Status = AppointmentStatus.Confirmed,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Appointments.Add(appointment);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetMine), new { id = appointment.Id }, appointment.Id);
    }

    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<AppointmentItem>>> GetMine(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var role = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

        var query = dbContext.Appointments
            .AsNoTracking()
            .Include(x => x.CounsellorProfile).ThenInclude(x => x.User)
            .Include(x => x.AvailabilitySlot)
            .AsQueryable();

        if (string.Equals(role, nameof(UserRole.Counsellor), StringComparison.OrdinalIgnoreCase))
        {
            var counsellor = await dbContext.CounsellorProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
            if (counsellor is null)
            {
                return Ok(Array.Empty<AppointmentItem>());
            }

            query = query.Where(x => x.CounsellorProfileId == counsellor.Id);
        }
        else
        {
            query = query.Where(x => x.StudentId == userId);
        }

        var response = await query
            .OrderByDescending(x => x.AvailabilitySlot.StartsAt)
            .Select(x => new AppointmentItem(
                x.Id,
                x.AvailabilitySlotId,
                x.CounsellorProfile.User.FullName,
                x.AvailabilitySlot.StartsAt,
                x.AvailabilitySlot.EndsAt,
                x.Status,
                x.Reason))
            .ToListAsync(cancellationToken);

        return Ok(response);
    }

    [HttpPatch("{appointmentId:guid}/cancel")]
    [Authorize(Roles = "Student,Admin")]
    public async Task<IActionResult> Cancel(Guid appointmentId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var appointment = await dbContext.Appointments.Include(x => x.AvailabilitySlot).SingleOrDefaultAsync(x => x.Id == appointmentId, cancellationToken);
        if (appointment is null)
        {
            return NotFound();
        }

        if (appointment.StudentId != userId && !User.IsInRole(nameof(UserRole.Admin)))
        {
            return Forbid();
        }

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.AvailabilitySlot.IsBooked = false;

        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("{appointmentId:guid}/status")]
    [Authorize(Roles = "Counsellor,Admin")]
    public async Task<IActionResult> UpdateStatus(Guid appointmentId, UpdateAppointmentStatusRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Status) || request.Status == AppointmentStatus.Cancelled)
        {
            return BadRequest("Invalid appointment status.");
        }

        var appointment = await dbContext.Appointments.SingleOrDefaultAsync(x => x.Id == appointmentId, cancellationToken);
        if (appointment is null)
        {
            return NotFound();
        }

        appointment.Status = request.Status;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
