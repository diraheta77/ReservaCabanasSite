using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;
using Microsoft.EntityFrameworkCore;

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

        [BindProperty]
        public MedioPago MedioPago { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MedioPago = await _context.MediosPago.FirstOrDefaultAsync(m => m.Id == id && m.Activo);
            if (MedioPago == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var medioPagoDb = await _context.MediosPago.FirstOrDefaultAsync(m => m.Id == id && m.Activo);
            if (medioPagoDb == null)
            {
                return NotFound();
            }
            medioPagoDb.Activo = false;
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
