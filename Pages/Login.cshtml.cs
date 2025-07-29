using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Services;

namespace ReservaCabanasSite.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;

        public LoginModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public string NombreUsuario { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // Si ya está autenticado, redirigir al dashboard
            if (_authService.IsAuthenticated())
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(NombreUsuario) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Por favor ingrese usuario y contraseña.";
                return Page();
            }

            var loginSuccess = await _authService.LoginAsync(NombreUsuario, Password);

            if (loginSuccess)
            {
                return RedirectToPage("/Index");
            }
            else
            {
                ErrorMessage = "Usuario o contraseña incorrectos.";
                return Page();
            }
        }
    }
} 