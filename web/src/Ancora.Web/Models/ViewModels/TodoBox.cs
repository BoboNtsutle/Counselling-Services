namespace Ancora.Web.Models.ViewModels;

/// <summary>Used by Views/Shared/_TodoBox.cshtml to show what is still left to build on a page.</summary>
public record TodoBox(string Module, params string[] Items);
