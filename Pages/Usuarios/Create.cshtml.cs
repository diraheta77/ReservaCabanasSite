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
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public CreateModel(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [BindProperty]
        public Usuario Usuario { get; set; }

        public IActionResult OnGet()
        {
            // Verificar que sea administrador
            if (_authService.GetCurrentUserRole() != "Administrador")
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Verificar que sea administrador
            if (_authService.GetCurrentUserRole() != "Administrador")
            {
                return RedirectToPage("/Index");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Validación: verificar si el nombre de usuario ya existe
            if (_context.Usuarios.Any(u => u.NombreUsuario == Usuario.NombreUsuario))
            {
                ModelState.AddModelError("Usuario.NombreUsuario", "Ya existe un usuario con ese nombre de usuario.");
                return Page();
            }

            // Aquí deberías usar un sistema de hash para la contraseña
            // Por ahora lo dejaré simple, pero en producción usa BCrypt o similar

            Usuario.FechaCreacion = DateTime.Now;
            Usuario.Activo = Usuario.Activo; // Usar el valor del formulario

            _context.Usuarios.Add(Usuario);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
