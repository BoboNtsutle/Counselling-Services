using CounsellingServices.Api.DTOs;
using CounsellingServices.Api.Extensions;
using CounsellingServices.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CounsellingServices.Api.Controllers;

[ApiController]
[Authorize(Roles = "Counsellor,Admin")]
[Route("api/availability")]
public sealed class AvailabilityController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateAvailabilitySlotRequest request, CancellationToken cancellationToken)
    {
        if (request.StartsAt >= request.EndsAt)
        {
            return BadRequest("Start time must be before end time.");
        }

        var userId = User.GetUserId();
        var counsellor = await dbContext.CounsellorProfiles.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (counsellor is null)
        {
            return NotFound("Counsellor profile not found.");
        }

        var overlapExists = await dbContext.AvailabilitySlots.AnyAsync(slot =>
            slot.CounsellorProfileId == counsellor.Id &&
            slot.StartsAt < request.EndsAt &&
            slot.EndsAt > request.StartsAt,
            cancellationToken);

        if (overlapExists)
        {
            return Conflict("This slot overlaps with existing availability.");
        }

        dbContext.AvailabilitySlots.Add(new()
        {
            Id = Guid.NewGuid(),
            CounsellorProfileId = counsellor.Id,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return Created();
    }
}
