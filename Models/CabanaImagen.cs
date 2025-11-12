
namespace ReservaCabanasSite.Models;

public class CabanaImagen
{
    public int Id { get; set; }
    public int CabanaId { get; set; }

    // Datos de la imagen almacenada como blob
    public byte[] ImagenData { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "image/jpeg";
    public string NombreArchivo { get; set; } = string.Empty;

    // Mantener ImagenUrl para compatibilidad temporal (puede ser null)
    public string? ImagenUrl { get; set; }

    public Cabana Cabana { get; set; }
}