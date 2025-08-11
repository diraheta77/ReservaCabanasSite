using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReservaCabanasSite.Models
{
    public class ReservaWizardViewModel
    {
        // Paso 1: Datos de la reserva
        public int? CabanaId { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? FechaDesde { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? FechaHasta { get; set; }
        
        public string Temporada { get; set; }
        
        public int CantidadPersonas { get; set; }
        
        public string MedioContacto { get; set; }
        
        public int? TemporadaId { get; set; }
        public decimal PrecioPorPersona { get; set; }
        
        // Paso 2: Datos del cliente
        public string Dni { get; set; }
        
        public string Nombre { get; set; }
        
        public string Apellido { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }
        
        public string? Nacionalidad { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? Provincia { get; set; }
        public string? Pais { get; set; }
        
        public string Telefono { get; set; }
        
        public string Email { get; set; }
        
        public string? Observaciones { get; set; }

        // Datos del vehículo
        public string? Patente { get; set; }
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? Color { get; set; }

        // Paso 3: Datos de pago
        public decimal MontoTotal { get; set; }
        
        public string MetodoPago { get; set; }
        
        public string EstadoPago { get; set; }
        
        // Propiedades de navegación
        public int? ClienteId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Cabana? Cabana { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Cliente? Cliente { get; set; }
        
        // Propiedades para el wizard
        public int PasoActual { get; set; } = 1;
        public bool EsPasoFinal { get; set; } = false;
        
        // Opciones para los dropdowns
        public List<string> Temporadas { get; set; } = new List<string>
        {
            "Alta",
            "Media", 
            "Baja"
        };
        
        public List<string> MediosContacto { get; set; } = new List<string>
        {
            "WhatsApp",
            "Teléfono",
            "Email",
            "Instagram",
            "Facebook"
        };
        
        public List<string> MediosPago { get; set; } = new List<string>
        {
            "Efectivo",
            "Transferencia",
            "Tarjeta de Crédito",
            "Tarjeta de Débito",
            "Mercado Pago"
        };
    }
}
