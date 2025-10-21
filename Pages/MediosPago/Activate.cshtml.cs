using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.MediosPago
{
    [ServiceFilter(typeof(AuthFilter))]
    public class ActivateModel : PageModel
    {
        private readonly AppDbContext _context;

        public ActivateModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medioPago = await _context.MediosPago.FindAsync(id);

            if (medioPago == null)
            {
                return NotFound();
            }

            medioPago.Activo = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Medio de pago activado exitosamente.";
            return RedirectToPage("./Index");
        }
    }
}
