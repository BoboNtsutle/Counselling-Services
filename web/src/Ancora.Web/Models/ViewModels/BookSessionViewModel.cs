using System.ComponentModel.DataAnnotations;

namespace Ancora.Web.Models.ViewModels;

/// <summary>What the student fills in on the "Book a session" page.</summary>
public class BookSessionViewModel
{
    // ---- Step 1: when ----
    [Required(ErrorMessage = "Please choose a date.")]
    public DateTime? AppointmentDate { get; set; }

    [Required(ErrorMessage = "Please choose a time.")]
    public string? AppointmentTime { get; set; }          // "08:00" ... "16:00" (see WorkingHours)

    // ---- Step 2: what ----
    [Required(ErrorMessage = "Please choose what you would like to talk about.")]
    public string? ProblemCategory { get; set; }

    // Nullable on purpose: with <Nullable> on, MVC treats plain "string" properties as [Required].
    public string? Urgency { get; set; } = Ancora.Web.Models.Urgency.Routine;   // Routine / Soon / Urgent
    public string? Mood { get; set; }                      // optional
    public string? Description { get; set; }               // optional

    // ---- Shown read-only (copied from the student's profile, never typed in) ----
    // (Nullable too, because they are not posted back with the form.)
    public string? StudentName { get; set; }
    public string? StudentNumber { get; set; }
    public string? Course { get; set; }

    // ---- Choices for the form ----
    public static readonly string[] Categories =
        { "Relationship problems", "Academic pressure", "HIV and AIDS", "Other" };

    public static readonly string[] Moods =
        { "Good", "Okay", "Anxious", "Low", "Overwhelmed" };

    public static readonly string[] UrgencyLevels =
        { Ancora.Web.Models.Urgency.Routine, Ancora.Web.Models.Urgency.Soon, Ancora.Web.Models.Urgency.Urgent };
}
