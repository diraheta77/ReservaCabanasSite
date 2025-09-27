using System.ComponentModel.DataAnnotations;

namespace ReservaCabanasSite.Models
{
    public class ReporteReservacionesViewModel
    {
        [Display(Name = "Fecha Desde")]
        [DataType(DataType.Date)]
        public DateTime FechaDesde { get; set; } = new DateTime(2025, 7, 1);

        [Display(Name = "Fecha Hasta")]
        [DataType(DataType.Date)]
        public DateTime FechaHasta { get; set; } = new DateTime(2025, 7, 31);

        [Display(Name = "DNI Cliente (opcional)")]
        public string? DniCliente { get; set; }

        public List<ReporteReservacionItem> Reservaciones { get; set; } = new();

        // Propiedades de paginación
        public int PaginaActual { get; set; } = 1;
        public int TamanoPagina { get; set; } = 10;
        public int TotalRegistros { get; set; }
        public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / TamanoPagina);
        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;

        // Estadísticas (basadas en todos los registros, no solo la página actual)
        public int TotalClientesCompleto { get; set; }
        public int TotalReservacionesCompleto { get; set; }
        public decimal TotalIngresosCompleto { get; set; }
        public int TotalPersonasCompleto { get; set; }

        // Para compatibilidad (basadas en la página actual)
        public int TotalClientes => Reservaciones.Count;
        public int TotalReservaciones => Reservaciones.Sum(r => r.TotalReservaciones);
        public decimal TotalIngresos => Reservaciones.Sum(r => r.MontoTotalAcumulado);
        public int TotalPersonas => Reservaciones.Sum(r => r.TotalPersonasAcumulado);
    }

    public class ReporteReservacionItem
    {
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = "";
        public string ClienteApellido { get; set; } = "";
        public string ClienteDni { get; set; } = "";
        public string ClienteEmail { get; set; } = "";
        public string ClienteTelefono { get; set; } = "";
        public int TotalReservaciones { get; set; }
        public decimal MontoTotalAcumulado { get; set; }
        public int TotalPersonasAcumulado { get; set; }
        public DateTime UltimaReservaFecha { get; set; }
        public string UltimaCabana { get; set; } = "";
        public string UltimoEstado { get; set; } = "";
        public DateTime PrimeraReservaFecha { get; set; }
        public string ClienteCompleto => $"{ClienteNombre} {ClienteApellido}";
        public decimal PromedioMontoReserva => TotalReservaciones > 0 ? MontoTotalAcumulado / TotalReservaciones : 0;
        public decimal PromedioPersonasPorReserva => TotalReservaciones > 0 ? (decimal)TotalPersonasAcumulado / TotalReservaciones : 0;
        public string FrecuenciaTexto => TotalReservaciones == 1 ? "Primera vez" : $"{TotalReservaciones} reservas";
    }
}