using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.Clientes
{
    [ServiceFilter(typeof(AuthFilter))]
    public class InactivateModel : PageModel
    {
        private readonly AppDbContext _context;
        public InactivateModel(AppDbContext context)
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var clienteDb = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.Activo);
            if (clienteDb == null)
            {
                return NotFound();
            }
            clienteDb.Activo = false;
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
