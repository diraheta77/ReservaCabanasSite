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

            // Solo permitir 1 imagen
            if (Imagenes != null && Imagenes.Count > 0 && Imagenes[0].Length > 0)
            {
                var imagen = Imagenes[0]; // Tomar solo la primera imagen
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(imagen.FileName)}";
                var filePath = Path.Combine("wwwroot/img/cabanas", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }
                var cabanaImagen = new CabanaImagen
                {
                    CabanaId = Cabana.Id,
                    ImagenUrl = $"/img/cabanas/{fileName}"
                };
                _context.CabanaImagenes.Add(cabanaImagen);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Index");
        }
    }
}