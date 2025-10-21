using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;

namespace ReservaCabanasSite.Pages.DatosEmpresa
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Models.DatosEmpresa DatosEmpresa { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            // Obtener el primer registro o crear uno nuevo
            DatosEmpresa = await _context.DatosEmpresa.FirstOrDefaultAsync() ?? new Models.DatosEmpresa();

            // Si no existe, crear un registro inicial
            if (DatosEmpresa.Id == 0)
            {
                _context.DatosEmpresa.Add(DatosEmpresa);
                await _context.SaveChangesAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            DatosEmpresa.FechaActualizacion = DateTime.Now;

            if (DatosEmpresa.Id == 0)
            {
                _context.DatosEmpresa.Add(DatosEmpresa);
            }
            else
            {
                _context.Attach(DatosEmpresa).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Los datos de la empresa se han actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await DatosEmpresaExists(DatosEmpresa.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage();
        }

        private async Task<bool> DatosEmpresaExists(int id)
        {
            return await _context.DatosEmpresa.AnyAsync(e => e.Id == id);
        }
    }
}
