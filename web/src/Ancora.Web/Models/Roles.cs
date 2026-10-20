namespace Ancora.Web.Models;

/// <summary>
/// The three roles stored in profiles.role in Supabase.
/// The web app only lets Admin and Psychologist in (students use the Flutter app).
/// </summary>
public static class Roles
{
    public const string Student = "student";
    public const string Psychologist = "psychologist";
    public const string Admin = "admin";
}

/// <summary>Allowed values for appointments.status (must match the database check constraint).</summary>
public static class AppointmentStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Complete = "Complete";

    public static readonly string[] All = { Pending, Approved, Rejected, Complete };
}

/// <summary>Allowed values for appointments.urgency.</summary>
public static class Urgency
{
    public const string Routine = "Routine";
    public const string Soon = "Soon";
    public const string Urgent = "Urgent";
}
