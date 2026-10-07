using CounsellingServices.Api.DTOs;
using CounsellingServices.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CounsellingServices.Api.Controllers;

[ApiController]
[Route("api/counsellors")]
public sealed class CounsellorsController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CounsellorResponse>>> Get(CancellationToken cancellationToken)
    {
        var counsellors = await dbContext.CounsellorProfiles
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.AvailabilitySlots.Where(slot => !slot.IsBooked && slot.StartsAt > DateTimeOffset.UtcNow))
            .OrderBy(x => x.User.FullName)
            .ToListAsync(cancellationToken);

        var response = counsellors.Select(c => new CounsellorResponse(
            c.Id,
            c.User.FullName,
            c.Specialty,
            c.AvailabilitySlots
                .OrderBy(slot => slot.StartsAt)
                .Select(slot => new CounsellorSlotResponse(slot.Id, slot.StartsAt, slot.EndsAt))
                .ToList())).ToList();

        return Ok(response);
    }
}
