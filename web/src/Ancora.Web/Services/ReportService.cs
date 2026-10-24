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

/// <summary>One line of a statistics table, e.g. "Pending" → 12.</summary>
public class CountRow
{
    public string Label { get; set; } = "";
    public int Count { get; set; }
}

/// <summary>Everything shown on the admin Statistics page.</summary>
public class StatisticsReport
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Total { get; set; }
    public List<CountRow> ByStatus { get; set; } = new();
    public List<CountRow> ByUrgency { get; set; } = new();
    public List<CountRow> TopReasons { get; set; } = new();
    public List<CountRow> ByPsychologist { get; set; } = new();
    public List<CountRow> ByMonth { get; set; } = new();

    /// <summary>Count for one label (e.g. a status), or 0 if it isn't there.</summary>
    public int CountOf(List<CountRow> rows, string label) =>
        rows.FirstOrDefault(r => string.Equals(r.Label, label, StringComparison.OrdinalIgnoreCase))?.Count ?? 0;

    /// <summary>Share of all appointments in the period, e.g. "25%".</summary>
    public string Percent(int count) => Total == 0 ? "0%" : $"{Math.Round(count * 100.0 / Total)}%";
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

    /// <summary>
    /// Non-graphical statistics for the Statistics page, optionally limited to appointments
    /// created between <paramref name="from"/> and <paramref name="to"/> (both days included).
    /// </summary>
    public async Task<StatisticsReport> GetStatisticsAsync(DateTime? from, DateTime? to)
    {
        var filter = "";
        if (from.HasValue) filter += $"&created_at=gte.{from.Value:yyyy-MM-dd}";
        // "lt" the next day, so appointments made during the 'to' day are included.
        if (to.HasValue) filter += $"&created_at=lt.{to.Value.Date.AddDays(1):yyyy-MM-dd}";

        // Supabase caps each response (1000 rows by default), so read the table in pages.
        const int pageSize = 1000;
        var all = new List<Appointment>();
        while (true)
        {
            var page = await _db.GetAsync<Appointment>("appointments",
                $"select=id,status,urgency,reason,psychologist_name,created_at{filter}&order=created_at,id&limit={pageSize}&offset={all.Count}");
            all.AddRange(page);
            if (page.Count < pageSize) break;
        }

        return new StatisticsReport
        {
            From = from,
            To = to,
            Total = all.Count,
            ByStatus = CountBy(all, a => a.Status, AppointmentStatus.All),
            ByUrgency = CountBy(all, a => a.Urgency, new[] { Urgency.Urgent, Urgency.Soon, Urgency.Routine }),
            TopReasons = CountBy(all, a => ReasonCategory(a.Reason))
                .OrderByDescending(r => r.Count).ThenBy(r => r.Label).Take(10).ToList(),
            ByPsychologist = CountBy(all, a => string.IsNullOrWhiteSpace(a.PsychologistName) ? "(Unassigned)" : a.PsychologistName)
                .OrderByDescending(r => r.Count).ThenBy(r => r.Label).ToList(),
            ByMonth = all.GroupBy(a => new DateTime(a.CreatedAt.Year, a.CreatedAt.Month, 1))
                .OrderBy(g => g.Key)
                .Select(g => new CountRow { Label = g.Key.ToString("MMMM yyyy"), Count = g.Count() })
                .ToList(),
        };
    }

    /// <summary>
    /// The booking form saves reason as "Category: the student's own words", so only the part
    /// before the first ": " is counted — otherwise every free-text reason would be its own row.
    /// </summary>
    private static string ReasonCategory(string? reason)
    {
        var text = reason?.Trim() ?? "";
        var colon = text.IndexOf(": ", StringComparison.Ordinal);
        if (colon > 0) text = text[..colon].Trim();
        return text.Length == 0 ? "(Not given)" : text;
    }

    /// <summary>
    /// Counts rows per value (ignoring case). Values in <paramref name="always"/> come first, in
    /// that order, even when their count is 0; any other values found in the data follow.
    /// </summary>
    private static List<CountRow> CountBy(List<Appointment> rows, Func<Appointment, string?> key, string[]? always = null)
    {
        var counts = rows
            .GroupBy(a => string.IsNullOrWhiteSpace(key(a)) ? "(Not set)" : key(a)!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var result = new List<CountRow>();
        foreach (var value in always ?? Array.Empty<string>())
        {
            result.Add(new CountRow { Label = value, Count = counts.GetValueOrDefault(value) });
            counts.Remove(value);
        }
        result.AddRange(counts.Select(c => new CountRow { Label = c.Key, Count = c.Value }));
        return result;
    }
}
