using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Services;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.Usuarios
{
    [ServiceFilter(typeof(AuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public IndexModel(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public IList<Usuario> Usuarios { get; set; } = default!;
        public int CurrentUserId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Verificar que sea administrador
            if (_authService.GetCurrentUserRole() != "Administrador")
            {
                return RedirectToPage("/Index");
            }

            CurrentUserId = _authService.GetCurrentUserId();
            Usuarios = await _context.Usuarios.OrderBy(u => u.NombreUsuario).ToListAsync();
            
            return Page();
        }
    }
} 