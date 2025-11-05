using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.Clientes
{
    [ServiceFilter(typeof(AuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Cliente> Clientes { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? BusquedaTexto { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(BusquedaTexto))
            {
                var busqueda = BusquedaTexto.Trim().ToLower();
                query = query.Where(c =>
                    c.Dni.ToLower().Contains(busqueda) ||
                    c.Nombre.ToLower().Contains(busqueda) ||
                    c.Apellido.ToLower().Contains(busqueda) ||
                    (c.Nombre + " " + c.Apellido).ToLower().Contains(busqueda)
                );
            }

            Clientes = await query.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre).ToListAsync();
        }

        public async Task<IActionResult> OnPostActivarAsync(int id)
        {
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id && !c.Activo);
            if (cliente != null)
            {
                cliente.Activo = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
