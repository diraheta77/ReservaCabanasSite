using System.ComponentModel.DataAnnotations;

namespace ReservaCabanasSite.Models
{
    public class Vehiculo
    {
        public int Id { get; set; }
        [Required]
        public string Patente { get; set; }
        [Required]
        public string Marca { get; set; }
        [Required]
        public string Modelo { get; set; }
        [Required]
        public string Color { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }
    }
} 