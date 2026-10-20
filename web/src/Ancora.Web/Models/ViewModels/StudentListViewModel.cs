namespace Ancora.Web.Models.ViewModels;

/// <summary>One row of the "Student records" list.</summary>
public class StudentListItem
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = "";
    public string StudentNumber { get; set; } = "";
    public string Course { get; set; } = "";
    public string Email { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public int AppointmentCount { get; set; }
    public int RecordCount { get; set; }
    public string LastSession { get; set; } = "";
    public bool HasProfile { get; set; } = true;
}

/// <summary>The "Student records" list page: the matching students plus the search that produced them.</summary>
public class StudentListViewModel
{
    public List<StudentListItem> Students { get; set; } = new();
    public int TotalStudents { get; set; }
    public string? Query { get; set; }

    /// <summary>"any" (name, number or email), "name" or "number".</summary>
    public string SearchBy { get; set; } = StudentSearchBy.Any;
    public bool RecordsTableMissing { get; set; }
}

public static class StudentSearchBy
{
    public const string Any = "any";
    public const string Name = "name";
    public const string Number = "number";

    public static string Normalise(string? value) =>
        value?.Trim().ToLowerInvariant() switch { Name => Name, Number => Number, _ => Any };
}
