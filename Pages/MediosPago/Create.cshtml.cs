using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.MediosPago
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
        public MedioPago MedioPago { get; set; } = new MedioPago();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            MedioPago.FechaCreacion = DateTime.Now;
            MedioPago.Activo = true;

            _context.MediosPago.Add(MedioPago);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Medio de pago creado exitosamente.";
            return RedirectToPage("./Index");
        }
    }
}
