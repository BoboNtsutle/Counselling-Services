using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Ancora.Web.Services.Supabase;

/// <summary>
/// Catches SupabaseException anywhere in a controller so nobody has to write try/catch everywhere:
///  - expired session (401) → sign out and go to the login page
///  - anything else       → show the message as a red alert on the page the user came from
/// You can still catch SupabaseException yourself when you want custom handling.
/// </summary>
public class SupabaseErrorFilter : IAsyncExceptionFilter
{
    private readonly ITempDataDictionaryFactory _tempData;

    public SupabaseErrorFilter(ITempDataDictionaryFactory tempData) => _tempData = tempData;

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is not SupabaseException ex) return;

        if (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
        }
        else
        {
            var tempData = _tempData.GetTempData(context.HttpContext);
            tempData[Flash.Error] = ex.Message;
            var referer = context.HttpContext.Request.Headers.Referer.ToString();
            var isGet = HttpMethods.IsGet(context.HttpContext.Request.Method);
            context.Result = !isGet && !string.IsNullOrEmpty(referer)
                ? new RedirectResult(referer)
                : new ViewResult { ViewName = "~/Views/Shared/SupabaseError.cshtml" };
        }
        context.ExceptionHandled = true;
    }
}

/// <summary>Keys for one-time messages shown at the top of the page (see _Layout.cshtml).</summary>
public static class Flash
{
    public const string Success = "FlashSuccess";
    public const string Error = "FlashError";
}
