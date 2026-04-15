using Hangfire.Dashboard;
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

        var header = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            Challenge(httpContext);
            return false;
        }

        try
        {
            var encoded = header["Basic ".Length..].Trim();
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var colon   = decoded.IndexOf(':');
            if (colon < 0) { Challenge(httpContext); return false; }

            var user = decoded[..colon];
            var pass = decoded[(colon + 1)..];

            if (string.Equals(user, _username, StringComparison.Ordinal) &&
                string.Equals(pass, _password, StringComparison.Ordinal))
            {
                return true;
            }
        }
        catch { /* malformed header */ }

        Challenge(httpContext);
        return false;
    }

    private static void Challenge(HttpContext ctx)
    {
        ctx.Response.StatusCode = 401;
        ctx.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
    }
}
