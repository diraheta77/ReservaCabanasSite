using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.MediosContacto
{
    [ServiceFilter(typeof(AuthFilter))]
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MedioContacto MedioContacto { get; set; } = new MedioContacto();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            MedioContacto.FechaCreacion = DateTime.Now;
            MedioContacto.Activo = true;

            _context.MediosContacto.Add(MedioContacto);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Medio de contacto creado exitosamente.";
            return RedirectToPage("./Index");
        }
    }
}
