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
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;
        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }
        [BindProperty]
        public Temporada Temporada { get; set; }
        public IActionResult OnGet(int id)
        {
            Temporada = _context.Temporadas.FirstOrDefault(t => t.Id == id);
            if (Temporada == null)
                return NotFound();
            return Page();
        }
        public IActionResult OnPost()
        {
            var temporadaDb = _context.Temporadas.FirstOrDefault(t => t.Id == Temporada.Id);
            if (temporadaDb == null)
                return NotFound();
            _context.Temporadas.Remove(temporadaDb);
            _context.SaveChanges();
            return RedirectToPage("Index");
        }
    }
} 