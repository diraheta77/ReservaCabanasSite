using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.Clientes
{
    [ServiceFilter(typeof(AuthFilter))]
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;
        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public Cliente Cliente { get; set; }
        public Vehiculo Vehiculo { get; set; }

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
            Vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(v => v.ClienteId == Cliente.Id);
            return Page();
        }
    }
}
