
namespace ReservaCabanasSite.Models;

public class CabanaImagen
{
    public int Id { get; set; }
    public int CabanaId { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;

    public Cabana Cabana { get; set; }
}