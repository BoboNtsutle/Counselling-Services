namespace Ancora.Web.Models.ViewModels;

/// <summary>Everything the student dashboard shows.</summary>
public class StudentDashboardViewModel
{
    public Profile? Profile { get; set; }

    /// <summary>Appointments that are still current: Pending or Approved, soonest first.</summary>
    public List<Appointment> Upcoming { get; set; } = new();

    /// <summary>The newest announcements published by the admin.</summary>
    public List<Announcement> News { get; set; } = new();
}
