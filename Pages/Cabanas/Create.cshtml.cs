using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.Cabanas
{
    [ServiceFilter(typeof(AuthFilter))]
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;
        [BindProperty]
        public Cabana Cabana { get; set; } = new();

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync(List<IFormFile> Imagenes)
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Cabanas.Add(Cabana);
            await _context.SaveChangesAsync();

            // Solo permitir 1 imagen - guardar como blob en BD
            if (Imagenes != null && Imagenes.Count > 0 && Imagenes[0].Length > 0)
            {
                var imagen = Imagenes[0]; // Tomar solo la primera imagen

                using (var memoryStream = new MemoryStream())
                {
                    await imagen.CopyToAsync(memoryStream);
                    var imageData = memoryStream.ToArray();

                    var cabanaImagen = new CabanaImagen
                    {
                        CabanaId = Cabana.Id,
                        ImagenData = imageData,
                        ContentType = imagen.ContentType,
                        NombreArchivo = imagen.FileName,
                        ImagenUrl = null // Ya no usamos archivos físicos
                    };
                    _context.CabanaImagenes.Add(cabanaImagen);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToPage("Index");
        }
    }
}