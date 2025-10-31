using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.MediosContacto
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

            var medioContacto = await _context.MediosContacto.FindAsync(id);

            if (medioContacto == null)
            {
                return NotFound();
            }

            medioContacto.Activo = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Medio de contacto activado exitosamente.";
            return RedirectToPage("./Index");
        }
    }
}
