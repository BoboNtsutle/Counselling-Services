using Ancora.Web.Models;
using Ancora.Web.Services.Supabase;

namespace Ancora.Web.Services;

/// <summary>Numbers for the admin dashboard cards.</summary>
public class DashboardCounts
{
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int CompletedThisMonth { get; set; }
    public int PendingRoleRequests { get; set; }
}

/// <summary>
/// MODULE 6 — Admin dashboard, statistics &amp; reports (FR-11)
/// The SRS says statistics are NON-graphical (raw numbers/tables) — graphs are "Won't have".
/// Owner: see TEAM-PLAN.md
/// </summary>
public class ReportService
{
    private readonly SupabaseClient _db;

    public ReportService(SupabaseClient db) => _db = db;

    public async Task<DashboardCounts> GetDashboardCountsAsync()
    {
        var counts = new DashboardCounts
        {
            // Done as an example of CountAsync:
            Pending = await _db.CountAsync("appointments", $"status=eq.{AppointmentStatus.Pending}"),
        };
        // Fill remaining dashboard counts
        counts.Approved = await _db.CountAsync("appointments", $"status=eq.{AppointmentStatus.Approved}");
        counts.PendingRoleRequests = await _db.CountAsync("profiles", "role_request_status=eq.pending");
        var firstOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).ToString("yyyy-MM-dd");
        counts.CompletedThisMonth = await _db.CountAsync("appointments", $"status=eq.{AppointmentStatus.Complete}&updated_at=gte.{firstOfMonth}");
        return counts;
    }

    // Basic non-graphical statistics implementation: return counts grouped by various fields.
    public async Task<object> GetStatisticsAsync(DateTime? from, DateTime? to)
    {
        // Build filter for date range on created_at if provided
        var filter = "";
        if (from.HasValue) filter += $"created_at=gte.{from.Value:yyyy-MM-dd}";
        if (to.HasValue)
        {
            if (filter.Length > 0) filter += "&";
            // include the end of 'to' day
            filter += $"created_at=lte.{to.Value:yyyy-MM-dd}";
        }

        // Load needed fields once
        var query = string.IsNullOrEmpty(filter) ? "select=status,urgency,reason,mood,psychologist_name,created_at" : $"select=status,urgency,reason,mood,psychologist_name,created_at&{filter}";
        var all = await _db.GetAsync<Appointment>("appointments", query);

        var byStatus = all.GroupBy(a => a.Status ?? "").Select(g => new { Status = g.Key, Count = g.Count() }).ToList();
        var byUrgency = all.GroupBy(a => a.Urgency ?? "").Select(g => new { Urgency = g.Key, Count = g.Count() }).ToList();
        var byPsych = all.GroupBy(a => a.PsychologistName ?? "(Unassigned)").Select(g => new { Psychologist = g.Key, Count = g.Count() }).ToList();
        var byMonth = all.GroupBy(a => a.CreatedAt.ToString("yyyy-MM")).Select(g => new { Month = g.Key, Count = g.Count() }).ToList();

        return new { ByStatus = byStatus, ByUrgency = byUrgency, ByPsychologist = byPsych, ByMonth = byMonth, Total = all.Count };
    }
}
