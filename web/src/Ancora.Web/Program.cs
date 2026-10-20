using Ancora.Web.Services;
using Ancora.Web.Services.Supabase;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// ---- Supabase (shared with the Flutter app) ----
builder.Services.Configure<SupabaseOptions>(builder.Configuration.GetSection("Supabase"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<SupabaseAuthService>();
builder.Services.AddHttpClient<SupabaseClient>();
builder.Services.AddScoped<SupabaseSessionStore>();

// ---- One service per module (already registered, so nobody needs to edit Program.cs) ----
builder.Services.AddScoped<AppointmentAdminService>();   // Module 2
builder.Services.AddScoped<UserAdminService>();          // Module 3
builder.Services.AddScoped<AnnouncementService>();       // Module 4
builder.Services.AddScoped<PsychologistService>();       // Module 5
builder.Services.AddScoped<ReportService>();             // Module 6
builder.Services.AddScoped<ProfileService>();            // Module 1

// ---- Login cookie ----
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "Ancora.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// Every page needs a logged-in user unless it says [AllowAnonymous].
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<SupabaseErrorFilter>();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
