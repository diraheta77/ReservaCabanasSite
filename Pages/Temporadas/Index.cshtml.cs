using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using System.Collections.Generic;
using System.Linq;

namespace ReservaCabanasSite.Pages.Temporadas
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context)
        {
            _context = context;
        }
        public IList<Temporada> Temporadas { get; set; } = new List<Temporada>();
        public void OnGet()
        {
            Temporadas = _context.Temporadas.ToList();
        }
    }
} 