using System.Security.Claims;
using Ancora.Web.Models;
using Ancora.Web.Models.ViewModels;
using Ancora.Web.Services;
using Ancora.Web.Services.Supabase;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Controllers;

/// <summary>
/// MODULE 1 — Login / logout / forgot password (FR-01).
/// Uses the SAME Supabase accounts as the Flutter app.
/// Only admins and psychologists may use the web app.
/// </summary>
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SupabaseAuthService _auth;
    private readonly SupabaseSessionStore _session;
    private readonly ProfileService _profiles;
    private readonly SupabaseClient _db;
    private readonly ILogger<AccountController> _logger;

    public AccountController(SupabaseAuthService auth, SupabaseSessionStore session, ProfileService profiles, SupabaseClient db, ILogger<AccountController> logger)
    {
        _auth = auth;
        _session = session;
        _profiles = profiles;
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register() => View(new Models.ViewModels.RegisterViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(Models.ViewModels.RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var session = await _auth.SignUpAsync(model.Email, model.Password);

            // insert a profile row with requested role; keep role as student until admin approves
            var requested = model.RequestedRole?.Trim().ToLower() == "psychologist" ? Roles.Psychologist : Roles.Student;
            var status = requested == Roles.Psychologist ? "pending" : "none";

            await _db.InsertAsync<object>("profiles", new
            {
                id = session.User.Id,
                email = model.Email.Trim(),
                display_name = model.DisplayName.Trim(),
                requested_role = requested,
                role_request_status = status,
                student_number = model.StudentNumber ?? "",
                phone_number = model.PhoneNumber ?? ""
            });

            TempData["FlashSuccess"] = "Registration successful. If you requested staff access, an admin will review your request.";
            return RedirectToAction(nameof(Login));
        }
        catch (SupabaseException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Could not reach Supabase. Check your internet connection.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var session = await _auth.SignInAsync(model.Email, model.Password);
            var profile = await _profiles.GetProfileAsync(session.User.Id, session.AccessToken);

            if (profile == null)
            {
                ModelState.AddModelError("", "Your account has no profile yet. Ask the admin to run supabase/schema.sql.");
                return View(model);
            }

            // Allow students to sign in as well. Role-based access to pages is enforced via [Authorize] on controllers.
            // If you want to restrict student access to certain pages, keep using role attributes on controllers/actions.

            await _session.SignInAsync(session, profile);

            // Log for diagnostics and set a brief flash so the browser shows the role after login.
            _logger.LogInformation("User {UserId} signed in with role {Role}", session.User.Id, profile?.Role);
            TempData["FlashSuccess"] = $"Signed in as {profile?.Role}";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            // Redirect students to their dashboard after login
            if (profile?.Role == Roles.Student)
                return RedirectToAction("Index", "Dashboard", new { area = "Student" });

            // Default: go to home page for other roles
            return RedirectToAction("Index", "Home");
        }
        catch (SupabaseException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Could not reach Supabase. Check your internet connection.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var token = User.FindFirstValue(SupabaseSessionStore.AccessTokenClaim);
        if (!string.IsNullOrEmpty(token)) await _auth.SignOutAsync(token);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            await _auth.SendPasswordResetAsync(model.Email);
        }
        catch (SupabaseException)
        {
            // Don't reveal whether the email exists.
        }
        TempData[Flash.Success] = "If that email has an account, a reset link is on its way.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
