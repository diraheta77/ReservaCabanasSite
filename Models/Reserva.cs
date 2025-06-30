namespace ReservaCabanasSite.Models;
public class Reserva
{
    public int Id { get; set; }
    public int CabanaId { get; set; }
    public Cabana Cabana { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; }

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    public int CantidadPersonas { get; set; }
    public string? Contacto { get; set; }
    public decimal Total { get; set; }
}