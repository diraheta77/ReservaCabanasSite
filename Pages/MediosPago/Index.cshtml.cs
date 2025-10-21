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
    }
}
