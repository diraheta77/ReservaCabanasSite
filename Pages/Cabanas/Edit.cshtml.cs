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

            // Reemplazar imagen si se sube una nueva (solo 1 imagen permitida)
            if (Imagenes != null && Imagenes.Length > 0 && Imagenes[0].Length > 0)
            {
                var imagen = Imagenes[0]; // Tomar solo la primera imagen

                // Eliminar imagen anterior si existe
                if (cabanaDb.Imagenes != null && cabanaDb.Imagenes.Any())
                {
                    var imagenAnterior = cabanaDb.Imagenes.First();

                    // Si tenía ImagenUrl (sistema antiguo), eliminar el archivo físico
                    if (!string.IsNullOrEmpty(imagenAnterior.ImagenUrl))
                    {
                        var rutaImagenAnterior = Path.Combine("wwwroot", imagenAnterior.ImagenUrl.TrimStart('/'));
                        if (System.IO.File.Exists(rutaImagenAnterior))
                        {
                            System.IO.File.Delete(rutaImagenAnterior);
                        }
                    }

                    _context.CabanaImagenes.Remove(imagenAnterior);
                }

                // Guardar nueva imagen como blob en la base de datos
                using (var memoryStream = new MemoryStream())
                {
                    await imagen.CopyToAsync(memoryStream);
                    var imageData = memoryStream.ToArray();

                    if (cabanaDb.Imagenes == null)
                        cabanaDb.Imagenes = new List<CabanaImagen>();

                    cabanaDb.Imagenes.Add(new CabanaImagen
                    {
                        CabanaId = cabanaDb.Id,
                        ImagenData = imageData,
                        ContentType = imagen.ContentType,
                        NombreArchivo = imagen.FileName,
                        ImagenUrl = null // Ya no usamos archivos físicos
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}