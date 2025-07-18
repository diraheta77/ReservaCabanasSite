using System;
using System.ComponentModel.DataAnnotations;

namespace ReservaCabanasSite.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        [Required]
        public string Dni { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Apellido { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Nacionalidad { get; set; }
        public string Direccion { get; set; }
        public string Ciudad { get; set; }
        public string Provincia { get; set; }
        public string Pais { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Observaciones { get; set; }
        public bool Activo { get; set; } = true;
    }
}