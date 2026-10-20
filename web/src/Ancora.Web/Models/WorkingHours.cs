namespace Ancora.Web.Models;

/// <summary>
/// The days and hours the Wellness Centre takes appointments (Mon-Fri, 08:00-17:00). One place to change them.
/// Slots are one hour long and are written as "08:00", "09:00" ... "16:00".
/// </summary>
public static class WorkingHours
{
    public const int FirstHour = 8;   // first session starts at 08:00
    public const int LastHour = 16;   // last session starts at 16:00 (ends 17:00)

    /// <summary>The centre is open Monday to Friday only.</summary>
    public static bool IsWorkingDay(DateTime date) =>
        date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;

    /// <summary>All slot start times as text, e.g. "08:00".</summary>
    public static readonly string[] Slots = Enumerable
        .Range(FirstHour, LastHour - FirstHour + 1)
        .Select(h => $"{h:00}:00")
        .ToArray();
}
