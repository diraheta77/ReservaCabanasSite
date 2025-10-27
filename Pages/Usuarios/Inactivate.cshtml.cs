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
    public class InactivateModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public InactivateModel(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [BindProperty]
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

            // No permitir inactivar el usuario actual
            if (Usuario.Id == _authService.GetCurrentUserId())
            {
                TempData["Error"] = "No puedes inactivar tu propio usuario.";
                return RedirectToPage("Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
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

            var usuarioDb = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuarioDb == null)
            {
                return NotFound();
            }

            // No permitir inactivar el usuario actual
            if (usuarioDb.Id == _authService.GetCurrentUserId())
            {
                TempData["Error"] = "No puedes inactivar tu propio usuario.";
                return RedirectToPage("Index");
            }

            // Cambiar el estado
            usuarioDb.Activo = !usuarioDb.Activo;
            await _context.SaveChangesAsync();

            TempData["Success"] = usuarioDb.Activo
                ? "Usuario activado exitosamente."
                : "Usuario inactivado exitosamente.";

            return RedirectToPage("Index");
        }
    }
}
