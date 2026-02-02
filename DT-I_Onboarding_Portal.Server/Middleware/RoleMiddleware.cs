using System.Security.Claims;
using DT_I_Onboarding_Portal.Core.Attributes;

namespace DT_I_Onboarding_Portal.Server.Middleware
{
    // Middleware enforces the RequireRolesAttribute (returns 401 or 403)
    public class RoleMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleMiddleware(RequestDelegate next) => _next = next;
         
        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var attr = endpoint.Metadata.GetMetadata<RequireRolesAttribute>();
                if (attr != null)
                {
                    if (!(context.User?.Identity?.IsAuthenticated ?? false))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new { message = "Authentication required" });
                        return;
                    }

                    var userRoles = context.User.Claims
                        .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                        .Select(c => c.Value)
                        .ToArray();

                    var required = attr.Roles ?? System.Array.Empty<string>();

                    if (!required.Any(r => userRoles.Contains(r)))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(new { message = "Forbidden: insufficient role" });
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}