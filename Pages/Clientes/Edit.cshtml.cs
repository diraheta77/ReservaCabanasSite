using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;

namespace ReservaCabanasSite.Pages.Clientes
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;
        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Cliente Cliente { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.Activo);
            if (Cliente == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var clienteDb = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == Cliente.Id && c.Activo);
            if (clienteDb == null)
            {
                return NotFound();
            }
            // Actualizar campos
            clienteDb.Dni = Cliente.Dni;
            clienteDb.Nombre = Cliente.Nombre;
            clienteDb.Apellido = Cliente.Apellido;
            clienteDb.FechaNacimiento = Cliente.FechaNacimiento;
            clienteDb.Nacionalidad = Cliente.Nacionalidad;
            clienteDb.Direccion = Cliente.Direccion;
            clienteDb.Ciudad = Cliente.Ciudad;
            clienteDb.Provincia = Cliente.Provincia;
            clienteDb.Pais = Cliente.Pais;
            clienteDb.Telefono = Cliente.Telefono;
            clienteDb.Email = Cliente.Email;
            clienteDb.Observaciones = Cliente.Observaciones;
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
