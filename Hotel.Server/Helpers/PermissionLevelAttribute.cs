using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hotel.Server.Data;
using Hotel.Server.Users;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Hotel.Server.Helpers;

/// <summary>
/// This attribute can be used on any controller endpoint and will check if the caller has required
/// permission level using the tenant data prepared by the PermissionLevelMiddleware
/// </summary>
public class PermissionLevelAttribute : ActionFilterAttribute
{
    private readonly string _requiredLevel;

    public PermissionLevelAttribute(string requiredLevel)
    {
        _requiredLevel = requiredLevel;
    }

    private bool isAuthorizedRole(string userRole, string requiredRole)
    {
        if (requiredRole == UserRole.Any) return true;

        if (requiredRole == UserRole.Admin && userRole is UserRole.Admin) return true;
        if (requiredRole == UserRole.User && userRole is UserRole.Admin or UserRole.User) return true;

        return false;
    }

    private HotelContext getHotelContext(ActionExecutingContext context)
    {
        return (HotelContext)context.HttpContext.RequestServices.GetService(typeof(HotelContext));
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userId = (Guid?)context.HttpContext.Items["UserId"];

        if (!userId.HasValue)
        {
            throw new UnauthorizedException("Unauthorized. Please provide valid JWT token.");
        }

        var role = context.HttpContext.Items["UserRole"].ToString();

        if (!isAuthorizedRole(role, _requiredLevel))
        {
            throw new UnauthorizedException(
                $"Unauthorized. Authorization of '{_requiredLevel}' level is required.");
        }


        await base.OnActionExecutionAsync(context, next);
    }
}
