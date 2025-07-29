using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Services;

namespace ReservaCabanasSite.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IAuthService _authService;

    public IndexModel(ILogger<IndexModel> logger, IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    public IActionResult OnGet()
    {
        // Si no está autenticado, redirigir al login
        if (!_authService.IsAuthenticated())
        {
            return RedirectToPage("/Login");
        }

        // Si está autenticado, redirigir al dashboard de reservas
        return RedirectToPage("/Reservas/Index");
    }
}
