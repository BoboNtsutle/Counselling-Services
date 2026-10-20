using System.Globalization;

namespace Ancora.Web.Models.Admin;

/// <summary>
/// One month of availability for ONE psychologist.
/// A slot is "booked" when that psychologist has an Approved appointment at that hour.
/// </summary>
public class AvailabilityViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }

    /// <summary>Everyone in the drop-down, and the one currently shown.</summary>
    public List<Profile> Psychologists { get; set; } = new();
    public Profile? Selected { get; set; }

    /// <summary>Booked slots only: day → ("09:00" → the appointment that fills it).</summary>
    public Dictionary<DateTime, Dictionary<string, Appointment>> Booked { get; set; } = new();

    public static AvailabilityViewModel ForMonth(int year, int month, List<Profile> psychologists,
                                                 Guid? selectedId, List<Appointment> approved)
    {
        var model = new AvailabilityViewModel
        {
            Year = year,
            Month = month,
            Psychologists = psychologists,
            // First psychologist is shown when none was picked.
            Selected = psychologists.FirstOrDefault(p => p.Id == selectedId) ?? psychologists.FirstOrDefault()
        };
        if (model.Selected == null) return model;

        foreach (var appt in approved.Where(a => a.PsychologistId == model.Selected.Id))
        {
            if (!TryReadSlot(appt, out var date, out var slot)) continue;
            if (date.Year != year || date.Month != month) continue;

            if (!model.Booked.ContainsKey(date.Date)) model.Booked[date.Date] = new();
            model.Booked[date.Date][slot] = appt;
        }
        return model;
    }

    /// <summary>
    /// Turns the appointment's text date/time into a real date and a slot like "09:00".
    /// Works for "2026-10-12" / "09:00" (web) and for "Fri, Oct 9" / "9:00 AM" (older mobile data).
    /// </summary>
    private static bool TryReadSlot(Appointment a, out DateTime date, out string slot)
    {
        slot = "";
        if (!DateTime.TryParse(a.AppointmentDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            return false;

        TimeSpan time;
        if (!TimeSpan.TryParse(a.AppointmentTime, CultureInfo.InvariantCulture, out time))
        {
            if (!DateTime.TryParse(a.AppointmentTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
                return false;
            time = t.TimeOfDay;
        }

        slot = $"{time.Hours:00}:00";   // round down to the hour
        return true;
    }
}
