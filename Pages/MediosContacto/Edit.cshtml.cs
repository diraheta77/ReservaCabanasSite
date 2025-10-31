using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.MediosContacto
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
        public MedioContacto MedioContacto { get; set; } = new MedioContacto();

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

            MedioContacto = medioContacto;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var medioContactoDb = await _context.MediosContacto.FindAsync(MedioContacto.Id);

            if (medioContactoDb == null)
            {
                return NotFound();
            }

            medioContactoDb.Nombre = MedioContacto.Nombre;
            medioContactoDb.Descripcion = MedioContacto.Descripcion;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Medio de contacto actualizado exitosamente.";
            return RedirectToPage("./Index");
        }
    }
}
