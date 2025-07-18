
namespace ReservaCabanasSite.Models;
public class Cabana
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public int Capacidad { get; set; }
    public int CamasMatrimonial { get; set; }      // Nuevo
    public int CamasIndividuales { get; set; }     // Nuevo
    public decimal PrecioPorNoche { get; set; }
    public decimal PrecioAlta { get; set; }
    public decimal PrecioBaja { get; set; }
    public string? Observaciones { get; set; }     // Nuevo
    public string? ImagenUrl { get; set; }         // Nuevo (ruta de la imagen)
    public bool Activa { get; set; } = true;
    public ICollection<CabanaImagen> Imagenes { get; set; } = new List<CabanaImagen>();
}