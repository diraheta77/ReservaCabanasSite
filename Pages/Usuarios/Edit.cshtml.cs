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
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public EditModel(AppDbContext context, IAuthService authService)
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

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Verificar que sea administrador
            if (_authService.GetCurrentUserRole() != "Administrador")
            {
                return RedirectToPage("/Index");
            }

            // Remover validación del campo Password ya que es opcional en la edición
            ModelState.Remove("Usuario.Password");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var usuarioDb = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == Usuario.Id);
            if (usuarioDb == null)
            {
                return NotFound();
            }

            // Validación: verificar si el nombre de usuario ya existe (excepto el propio)
            if (_context.Usuarios.Any(u => u.NombreUsuario == Usuario.NombreUsuario && u.Id != Usuario.Id))
            {
                ModelState.AddModelError("Usuario.NombreUsuario", "Ya existe un usuario con ese nombre de usuario.");
                return Page();
            }

            // Actualizar campos
            usuarioDb.NombreUsuario = Usuario.NombreUsuario;
            usuarioDb.NombreCompleto = Usuario.NombreCompleto;
            usuarioDb.Rol = Usuario.Rol;
            usuarioDb.Activo = Usuario.Activo;

            // Solo actualizar contraseña si se proporciona una nueva
            var nuevaPassword = Request.Form["NuevaPassword"].ToString();
            if (!string.IsNullOrWhiteSpace(nuevaPassword))
            {
                // Hash de la contraseña usando SHA256
                usuarioDb.Password = _authService.HashPassword(nuevaPassword);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
