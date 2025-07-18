using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ReservaCabanasSite.Models
{
    public class Reserva
    {
        public int Id { get; set; }

        [Required]
        public int CabanaId { get; set; }

        [ValidateNever]
        public Cabana Cabana { get; set; }

        public int? ClienteId { get; set; }

        [ValidateNever]
        public Cliente Cliente { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaDesde { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaHasta { get; set; }

        public string? Temporada { get; set; }
        public int CantidadPersonas { get; set; }
        public string? MedioContacto { get; set; }

        public string? MetodoPago { get; set; }
        public string? EstadoPago { get; set; }
        public string? EstadoReserva { get; set; }
        public string? Observaciones { get; set; }
        public decimal MontoTotal { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activa { get; set; } = true;
    }
}