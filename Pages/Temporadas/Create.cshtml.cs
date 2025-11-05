using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.Temporadas
{
    [ServiceFilter(typeof(AuthFilter))]
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;
        public CreateModel(AppDbContext context)
        {
            _context = context;
        }
        [BindProperty]
        public Temporada Temporada { get; set; } = new Temporada();
        public IActionResult OnGet()
        {
            return Page();
        }
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            // Validar que FechaHasta sea mayor que FechaDesde
            if (Temporada.FechaHasta <= Temporada.FechaDesde)
            {
                ModelState.AddModelError("Temporada.FechaHasta", "La fecha de fin debe ser posterior a la fecha de inicio");
                return Page();
            }

            _context.Temporadas.Add(Temporada);
            _context.SaveChanges();
            return RedirectToPage("Index");
        }
    }
} 