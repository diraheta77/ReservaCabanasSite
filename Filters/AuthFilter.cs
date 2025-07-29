using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ReservaCabanasSite.Services;

namespace ReservaCabanasSite.Filters
{
    public class AuthFilter : IAuthorizationFilter
    {
        private readonly IAuthService _authService;

        public AuthFilter(IAuthService authService)
        {
            _authService = authService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!_authService.IsAuthenticated())
            {
                context.Result = new RedirectToPageResult("/Login");
            }
        }
    }

    public class AdminAuthFilter : IAuthorizationFilter
    {
        private readonly IAuthService _authService;

        public AdminAuthFilter(IAuthService authService)
        {
            _authService = authService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!_authService.IsAuthenticated())
            {
                context.Result = new RedirectToPageResult("/Login");
                return;
            }

            var userRole = _authService.GetCurrentUserRole();
            if (userRole != "Administrador")
            {
                context.Result = new RedirectToPageResult("/Index");
            }
        }
    }
} 