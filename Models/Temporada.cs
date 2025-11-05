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

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha Desde")]
        public DateTime FechaDesde { get; set; }

        [Required(ErrorMessage = "La fecha de fin es requerida")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha Hasta")]
        public DateTime FechaHasta { get; set; }
    }
} 