using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.Cabanas
{
    [ServiceFilter(typeof(AuthFilter))]
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;
        [BindProperty]
        public Cabana Cabana { get; set; } = new();

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(int id)
        {
            Cabana = _context.Cabanas
                .Include(c => c.Imagenes)
                .FirstOrDefault(c => c.Id == id)!;

            if (Cabana == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile[]? Imagenes)
        {
            if (!ModelState.IsValid)
                return Page();

            var cabanaDb = _context.Cabanas
                .Include(c => c.Imagenes)
                .FirstOrDefault(c => c.Id == Cabana.Id);

            if (cabanaDb == null)
                return NotFound();

            // Actualizar campos
            cabanaDb.Nombre = Cabana.Nombre;
            cabanaDb.Capacidad = Cabana.Capacidad;
            cabanaDb.CamasMatrimonial = Cabana.CamasMatrimonial;
            cabanaDb.CamasIndividuales = Cabana.CamasIndividuales;
            cabanaDb.Observaciones = Cabana.Observaciones;
            cabanaDb.Activa = Cabana.Activa;

            // Guardar imágenes nuevas
            if (Imagenes != null && Imagenes.Length > 0)
            {
                if (cabanaDb.Imagenes == null)
                    cabanaDb.Imagenes = new List<CabanaImagen>();

                foreach (var imagen in Imagenes)
                {
                    if (imagen.Length > 0)
                    {
                        var fileName = Path.GetFileName(imagen.FileName);
                        var filePath = Path.Combine("wwwroot/img/cabanas", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imagen.CopyToAsync(stream);
                        }
                        cabanaDb.Imagenes.Add(new CabanaImagen { ImagenUrl = $"/img/cabanas/{fileName}" });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}