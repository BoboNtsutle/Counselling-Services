using System.ComponentModel.DataAnnotations;

namespace Ancora.Web.Models.ViewModels;

public class RegisterViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password), MinLength(6)]
    public string Password { get; set; } = "";

    [Required]
    public string DisplayName { get; set; } = "";

    public string? StudentNumber { get; set; }

    public string? PhoneNumber { get; set; }

    [Required]
    public string RequestedRole { get; set; } = "psychologist"; // psychologist or student
}
