using Microsoft.AspNetCore.Mvc;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;

namespace ReservaCabanasSite.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagenController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ImagenController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene una imagen de cabaña desde la base de datos
        /// </summary>
        /// <param name="id">ID de la imagen</param>
        /// <returns>Archivo de imagen</returns>
        [HttpGet("cabana/{id}")]
        public async Task<IActionResult> GetImagenCabana(int id)
        {
            var imagen = await _context.CabanaImagenes
                .Where(i => i.Id == id)
                .Select(i => new { i.ImagenData, i.ContentType })
                .FirstOrDefaultAsync();

            if (imagen == null || imagen.ImagenData == null || imagen.ImagenData.Length == 0)
            {
                // Retornar una imagen placeholder o 404
                return NotFound();
            }

            return File(imagen.ImagenData, imagen.ContentType);
        }
    }
}
