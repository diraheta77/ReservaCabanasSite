using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.Temporadas
{
    [ServiceFilter(typeof(AuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context)
        {
            _context = context;
        }
        public IList<Temporada> Temporadas { get; set; } = new List<Temporada>();

        public async Task OnGetAsync()
        {
            Temporadas = await _context.Temporadas.ToListAsync();
        }

        public async Task<IActionResult> OnPostActivarAsync(int id)
        {
            var temporada = await _context.Temporadas.FirstOrDefaultAsync(t => t.Id == id && !t.Activa);
            if (temporada != null)
            {
                temporada.Activa = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
} 