using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace LiveExpert.API.Middleware;

/// <summary>
/// Protects the Hangfire dashboard with HTTP Basic authentication.
/// Credentials are read from appsettings: Hangfire:Username / Hangfire:Password
/// (or their HANGFIRE__USERNAME / HANGFIRE__PASSWORD env var equivalents).
/// </summary>
public class HangfireBasicAuthFilter : IDashboardAuthorizationFilter
{
    private readonly string _username;
    private readonly string _password;

    public HangfireBasicAuthFilter(string username, string password)
    {
        _username = username;
        _password = password;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var authHeader = httpContext.Request.Headers.Authorization.ToString();

        if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            Challenge(httpContext);
            return false;
        }

        try
        {
            var credentials = Encoding.UTF8
                .GetString(Convert.FromBase64String(authHeader["Basic ".Length..].Trim()));

            var colon = credentials.IndexOf(':');
            if (colon < 1) { Challenge(httpContext); return false; }

            var user = credentials[..colon];
            var pass = credentials[(colon + 1)..];

            if (user == _username && pass == _password)
                return true;
        }
        catch { /* malformed Base64 */ }

        Challenge(httpContext);
        return false;
    }

    private static void Challenge(HttpContext ctx)
    {
        ctx.Response.StatusCode = 401;
        ctx.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
    }
}
