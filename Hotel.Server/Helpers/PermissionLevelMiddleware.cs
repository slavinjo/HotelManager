using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Hotel.Server.Data;
using Microsoft.AspNetCore.Builder;

namespace Hotel.Server.Helpers;

/// <summary>
/// This middleware is extracting project, user and api key data from the request so it can
/// be used later in the pipeline
/// </summary>
public class PermissionLevelMiddleware
{
    private readonly RequestDelegate _next;

    public PermissionLevelMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, HotelContext hotelContext)
    {
        var isUserAuthenticated = false;
        var role = "";

        // try to find the user role JWT
        if (context.User != null && context.User.Identity.IsAuthenticated &&
            context.User.Identity.AuthenticationType != "ApiKey")
        {
            isUserAuthenticated = true;
            role = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value;
        }

        if (isUserAuthenticated && role == "member")
        {
            context.Items["MemberId"] = Guid.Parse(context.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value.ToString());
        }
        else if (isUserAuthenticated)
        {
            context.Items["UserId"] =
                Guid.Parse(context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value);

            context.Items["UserRole"] = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value.ToString();
        }

        // Call the next delegate/middleware in the pipeline
        await _next(context);
    }
}

public static class PermissionLevelMiddlewareExtensions
{
    public static IApplicationBuilder UsePermissionLevel(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PermissionLevelMiddleware>();
    }
}
