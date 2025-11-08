using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;
using Microsoft.EntityFrameworkCore;

namespace ReservaCabanasSite.Pages.MediosContacto
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
        public MedioContacto MedioContacto { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MedioContacto = await _context.MediosContacto.FirstOrDefaultAsync(m => m.Id == id && m.Activo);
            if (MedioContacto == null)
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
            var medioContactoDb = await _context.MediosContacto.FirstOrDefaultAsync(m => m.Id == id && m.Activo);
            if (medioContactoDb == null)
            {
                return NotFound();
            }
            medioContactoDb.Activo = false;
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
