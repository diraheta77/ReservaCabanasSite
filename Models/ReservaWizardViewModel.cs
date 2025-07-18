using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReservaCabanasSite.Models
{
    public class ReservaWizardViewModel
    {
        // Paso 1: Datos de la reserva
        [Required]
        public int? CabanaId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime? FechaDesde { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime? FechaHasta { get; set; }
        public string Temporada { get; set; }
        public int CantidadPersonas { get; set; }
        public string MedioContacto { get; set; }
        public string Observaciones { get; set; }

        // Paso 2: Cliente
        public int? ClienteId { get; set; }
        public string ClienteDni { get; set; }
        public string ClienteNombre { get; set; }
        public string ClienteApellido { get; set; }
        public string ClienteEmail { get; set; }

        // Paso 3: Pago
        public decimal ImporteTotal { get; set; }
        public decimal Sena { get; set; }
        public decimal Saldo { get; set; }
        public string MedioPago { get; set; }

        // Paso 4: Confirmación
        public bool Confirmado { get; set; }
    }
}
