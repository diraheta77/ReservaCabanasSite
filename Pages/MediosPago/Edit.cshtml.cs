using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.MediosPago
{
    [ServiceFilter(typeof(AuthFilter))]
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MedioPago MedioPago { get; set; } = new MedioPago();

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

            MedioPago = medioPago;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var medioPagoDb = await _context.MediosPago.FindAsync(MedioPago.Id);

            if (medioPagoDb == null)
            {
                return NotFound();
            }

            medioPagoDb.Nombre = MedioPago.Nombre;
            medioPagoDb.Descripcion = MedioPago.Descripcion;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Medio de pago actualizado exitosamente.";
            return RedirectToPage("./Index");
        }
    }
}
