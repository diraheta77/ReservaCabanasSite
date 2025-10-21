using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.MediosPago
{
    [ServiceFilter(typeof(AuthFilter))]
    public class InactivateModel : PageModel
    {
        private readonly AppDbContext _context;

        public InactivateModel(AppDbContext context)
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

            medioPago.Activo = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Medio de pago inactivado exitosamente.";
            return RedirectToPage("./Index");
        }
    }
}
