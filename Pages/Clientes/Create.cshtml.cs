using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;

namespace ReservaCabanasSite.Pages.Clientes
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;
        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Cliente Cliente { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            Cliente.Activo = true;
            // Validación personalizada para Dirección
            if (string.IsNullOrWhiteSpace(Cliente.Direccion) ||
                (!System.Text.RegularExpressions.Regex.IsMatch(Cliente.Direccion, @"\\d") && Cliente.Direccion.Trim().ToUpper() != "S/N"))
            {
                ModelState.AddModelError("Cliente.Direccion", "La dirección debe contener calle y número o decir S/N (sin numeración)");
                return Page();
            }
            _context.Clientes.Add(Cliente);
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
