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
        private readonly IWebHostEnvironment _environment;

        public IndexModel(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public Models.DatosEmpresa DatosEmpresa { get; set; } = default!;

        [BindProperty]
        public IFormFile? ArchivoLogo { get; set; }

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

            // Manejar la carga del logo si se proporciona
            if (ArchivoLogo != null && ArchivoLogo.Length > 0)
            {
                // Validar que sea una imagen
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg" };
                var extension = Path.GetExtension(ArchivoLogo.FileName).ToLowerInvariant();

                if (!extensionesPermitidas.Contains(extension))
                {
                    ModelState.AddModelError("ArchivoLogo", "Solo se permiten archivos de imagen (jpg, jpeg, png, gif, svg).");
                    return Page();
                }

                // Validar tamaño (máximo 5MB)
                if (ArchivoLogo.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ArchivoLogo", "El archivo no puede exceder 5MB.");
                    return Page();
                }

                // Generar nombre único para el archivo
                var nombreArchivo = $"logo_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                var rutaCompleta = Path.Combine(_environment.WebRootPath, "uploads", "empresa", nombreArchivo);

                // Guardar el archivo
                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await ArchivoLogo.CopyToAsync(stream);
                }

                // Actualizar la ruta en el modelo
                DatosEmpresa.RutaLogo = $"/uploads/empresa/{nombreArchivo}";
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
