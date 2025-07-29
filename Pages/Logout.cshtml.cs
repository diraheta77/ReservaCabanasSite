using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Services;

namespace ReservaCabanasSite.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly IAuthService _authService;

        public LogoutModel(IAuthService authService)
        {
            _authService = authService;
        }

        public IActionResult OnGet()
        {
            _authService.Logout();
            return RedirectToPage("/Login");
        }
    }
} 