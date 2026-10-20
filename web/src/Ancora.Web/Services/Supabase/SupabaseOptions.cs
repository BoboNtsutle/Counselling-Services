namespace Ancora.Web.Services.Supabase;

/// <summary>
/// Settings read from appsettings.json → "Supabase".
/// Use the SAME project URL and publishable key as the Flutter app
/// (lib/supabase_runtime_config.dart) so both apps share one database.
/// The publishable key is safe to commit. NEVER commit the service_role / secret key.
/// </summary>
public class SupabaseOptions
{
    public string Url { get; set; } = "";
    public string PublishableKey { get; set; } = "";
}
