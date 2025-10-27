using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Filters;
using ReservaCabanasSite.Services;

namespace ReservaCabanasSite.Pages.Usuarios
{
    [ServiceFilter(typeof(AuthFilter))]
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public DetailsModel(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public Usuario Usuario { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Verificar que sea administrador
            if (_authService.GetCurrentUserRole() != "Administrador")
            {
                return RedirectToPage("/Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            Usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

            if (Usuario == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
