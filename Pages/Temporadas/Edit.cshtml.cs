using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using System.Linq;

namespace ReservaCabanasSite.Pages.Temporadas
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;
        public EditModel(AppDbContext context)
        {
            _context = context;
        }
        [BindProperty]
        public Temporada Temporada { get; set; } = new Temporada();
        public IActionResult OnGet(int id)
        {
            Temporada = _context.Temporadas.FirstOrDefault(t => t.Id == id);
            if (Temporada == null)
                return NotFound();
            return Page();
        }
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();
            var temporadaDb = _context.Temporadas.FirstOrDefault(t => t.Id == Temporada.Id);
            if (temporadaDb == null)
                return NotFound();
            temporadaDb.Nombre = Temporada.Nombre;
            temporadaDb.PrecioPorPersona = Temporada.PrecioPorPersona;
            temporadaDb.Activa = Temporada.Activa;
            _context.SaveChanges();
            return RedirectToPage("Index");
        }
    }
} 