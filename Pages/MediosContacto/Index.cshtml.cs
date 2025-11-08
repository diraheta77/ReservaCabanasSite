using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.MediosContacto
{
    [ServiceFilter(typeof(AuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<MedioContacto> MediosContacto { get; set; } = new List<MedioContacto>();

        public async Task OnGetAsync()
        {
            MediosContacto = await _context.MediosContacto
                .OrderBy(m => m.Nombre)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostActivarAsync(int id)
        {
            var medioContacto = await _context.MediosContacto.FirstOrDefaultAsync(m => m.Id == id && !m.Activo);
            if (medioContacto != null)
            {
                medioContacto.Activo = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
