using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using System.Linq;

namespace ReservaCabanasSite.Pages.Temporadas
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;
        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }
        public Temporada Temporada { get; set; }
        public void OnGet(int id)
        {
            Temporada = _context.Temporadas.FirstOrDefault(t => t.Id == id);
        }
    }
} 