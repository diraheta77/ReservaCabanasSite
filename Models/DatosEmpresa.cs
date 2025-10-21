using System.ComponentModel.DataAnnotations;

namespace ReservaCabanasSite.Models
{
    public class DatosEmpresa
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la empresa es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        [Display(Name = "Nombre de la Empresa")]
        public string NombreEmpresa { get; set; } = "Aldea Auriel";

        [StringLength(300, ErrorMessage = "La dirección no puede exceder 300 caracteres")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; } = "Ojo de Agua 131";

        [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
        [Display(Name = "Ciudad")]
        public string? Ciudad { get; set; } = "Villa General Belgrano";

        [StringLength(100, ErrorMessage = "La provincia no puede exceder 100 caracteres")]
        [Display(Name = "Provincia")]
        public string? Provincia { get; set; } = "Córdoba";

        [StringLength(100, ErrorMessage = "El país no puede exceder 100 caracteres")]
        [Display(Name = "País")]
        public string? Pais { get; set; } = "Argentina";

        [StringLength(50, ErrorMessage = "El teléfono no puede exceder 50 caracteres")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; } = "03546-15-404114";

        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(100, ErrorMessage = "El sitio web no puede exceder 100 caracteres")]
        [Display(Name = "Sitio Web")]
        public string? SitioWeb { get; set; }

        [Display(Name = "Mostrar en Reportes")]
        public bool MostrarEnReportes { get; set; } = true;

        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }
}
