using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.Temporadas
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
        public Temporada Temporada { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Temporada = await _context.Temporadas.FirstOrDefaultAsync(t => t.Id == id && t.Activa);
            if (Temporada == null)
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
            var temporadaDb = await _context.Temporadas.FirstOrDefaultAsync(t => t.Id == id && t.Activa);
            if (temporadaDb == null)
            {
                return NotFound();
            }
            temporadaDb.Activa = false;
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
