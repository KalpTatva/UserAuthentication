using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static UserAuth.Repository.helpers.Enums;

namespace UserAuth.Core.Filters;

public class CustomAuthorizationFilter : Attribute, IAuthorizationFilter
{
    private readonly string[] _requiredRoles;

    public CustomAuthorizationFilter(params string[] requiredRoles)
    {
        _requiredRoles = requiredRoles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!user?.Identity?.IsAuthenticated ?? true)
        {
            // when user's identity is not authenticated, redirect to login page
            context.Result = new RedirectToActionResult("Error403", "Home", null);
            return;
        }
        bool hasAccess = _requiredRoles.Any(r => user.IsInRole(r));

        if(!hasAccess)
        {
            context.Result = new RedirectToActionResult("Error403", "Home", null);
            return;
        }
    }
}

