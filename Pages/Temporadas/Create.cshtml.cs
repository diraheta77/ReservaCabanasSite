using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;

namespace ReservaCabanasSite.Pages.Temporadas
{
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
            _context.Temporadas.Add(Temporada);
            _context.SaveChanges();
            return RedirectToPage("Index");
        }
    }
} 