using System.ComponentModel.DataAnnotations;

namespace ReservaCabanasSite.Models
{
    public class Temporada
    {
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public decimal PrecioPorPersona { get; set; }
        public bool Activa { get; set; } = true;
    }
} 