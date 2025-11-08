using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.MediosPago
{
    [ServiceFilter(typeof(AuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<MedioPago> MediosPago { get; set; } = new List<MedioPago>();

        public async Task OnGetAsync()
        {
            MediosPago = await _context.MediosPago
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostActivarAsync(int id)
        {
            var medioPago = await _context.MediosPago.FirstOrDefaultAsync(m => m.Id == id && !m.Activo);
            if (medioPago != null)
            {
                medioPago.Activo = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
