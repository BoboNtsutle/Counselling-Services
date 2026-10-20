namespace Ancora.Web.Models.ViewModels;

/// <summary>One student's record: who they are, every appointment, and every entry saved on their file.</summary>
public class StudentRecordViewModel
{
    public Profile Student { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();

    /// <summary>Entries saved on the student's permanent record, newest first.</summary>
    public List<StudentRecordEntry> Entries { get; set; } = new();

    /// <summary>Session notes (clinical_notes) the signed-in user is allowed to read, keyed by appointment id.</summary>
    public Dictionary<Guid, string> SessionNotes { get; set; } = new();

    /// <summary>False when this student appears in appointments but never created a profile.</summary>
    public bool HasProfile { get; set; } = true;

    /// <summary>True when the student_records table has not been created in Supabase yet.</summary>
    public bool RecordsTableMissing { get; set; }
}
