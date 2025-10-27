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

        [Display(Name = "Cabaña")]
        public int? CabanaId { get; set; }

        [Display(Name = "Temporada")]
        public int? TemporadaId { get; set; }

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

    public class ReporteEdadesViewModel
    {
        [Display(Name = "Fecha Desde")]
        [DataType(DataType.Date)]
        public DateTime FechaDesde { get; set; } = new DateTime(2025, 7, 1);

        [Display(Name = "Fecha Hasta")]
        [DataType(DataType.Date)]
        public DateTime FechaHasta { get; set; } = new DateTime(2025, 7, 31);

        [Display(Name = "DNI Cliente (opcional)")]
        public string? DniCliente { get; set; }

        [Display(Name = "Cabaña")]
        public int? CabanaId { get; set; }

        [Display(Name = "Temporada")]
        public int? TemporadaId { get; set; }

        public List<DatosEdadRango> DatosGrafica { get; set; } = new();
        public int TotalClientes { get; set; }
        public int TotalReservas { get; set; }
        public decimal TotalIngresos { get; set; }
        public double PromedioEdad { get; set; }
    }

    public class DatosEdadRango
    {
        public string Rango { get; set; } = "";
        public int Cantidad { get; set; }
        public double Porcentaje { get; set; }
    }

    public class ReporteCabanasViewModel
    {
        [Display(Name = "Fecha Desde")]
        [DataType(DataType.Date)]
        public DateTime FechaDesde { get; set; } = new DateTime(2025, 7, 1);

        [Display(Name = "Fecha Hasta")]
        [DataType(DataType.Date)]
        public DateTime FechaHasta { get; set; } = new DateTime(2025, 7, 31);

        [Display(Name = "Cabaña")]
        public int? CabanaId { get; set; }

        [Display(Name = "Temporada")]
        public int? TemporadaId { get; set; }

        public List<ReporteCabanaItem> ReservasPorCabana { get; set; } = new();
        public int TotalReservas { get; set; }
        public int TotalDias { get; set; }
        public decimal TotalIngresos { get; set; }
    }

    public class ReporteCabanaItem
    {
        public int CabanaId { get; set; }
        public string CabanaNombre { get; set; } = "";
        public int CantidadReservas { get; set; }
        public int CantidadDiasReservados { get; set; }
        public decimal TotalIngresos { get; set; }
        public double PorcentajeReservas { get; set; }
        public double PorcentajeDias { get; set; }
        public double PorcentajeIngresos { get; set; }
    }

    public class ReporteTemporadasViewModel
    {
        [Display(Name = "Fecha Desde")]
        [DataType(DataType.Date)]
        public DateTime FechaDesde { get; set; } = new DateTime(2025, 7, 1);

        [Display(Name = "Fecha Hasta")]
        [DataType(DataType.Date)]
        public DateTime FechaHasta { get; set; } = new DateTime(2025, 7, 31);

        [Display(Name = "Cabaña")]
        public int? CabanaId { get; set; }

        [Display(Name = "Temporada")]
        public int? TemporadaId { get; set; }

        public List<ReporteTemporadaItem> ReservasPorTemporada { get; set; } = new();
        public int TotalReservas { get; set; }
        public decimal TotalIngresos { get; set; }
    }

    public class ReporteTemporadaItem
    {
        public int TemporadaId { get; set; }
        public string TemporadaNombre { get; set; } = "";
        public int CantidadReservas { get; set; }
        public decimal TotalIngresos { get; set; }
        public double PorcentajeReservas { get; set; }
        public double PorcentajeIngresos { get; set; }
    }

    public class ReporteMesesViewModel
    {
        [Display(Name = "Año")]
        public int Ano { get; set; } = DateTime.Now.Year;

        [Display(Name = "Meses")]
        public List<int> MesesSeleccionados { get; set; } = new();

        [Display(Name = "Cabaña")]
        public int? CabanaId { get; set; }

        [Display(Name = "Temporada")]
        public int? TemporadaId { get; set; }

        public List<ReporteMesItem> ReservasPorMes { get; set; } = new();
        public int TotalReservas { get; set; }
        public decimal TotalIngresos { get; set; }
    }

    public class ReporteMesItem
    {
        public int Ano { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; } = "";
        public int CantidadReservas { get; set; }
        public decimal TotalIngresos { get; set; }
        public double PorcentajeReservas { get; set; }
        public double PorcentajeIngresos { get; set; }
    }

    public class ReporteMediosPagoViewModel
    {
        [Display(Name = "Fecha Desde")]
        [DataType(DataType.Date)]
        public DateTime FechaDesde { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        [Display(Name = "Fecha Hasta")]
        [DataType(DataType.Date)]
        public DateTime FechaHasta { get; set; } = DateTime.Now;

        [Display(Name = "Cabaña")]
        public int? CabanaId { get; set; }

        public List<ReporteMedioPagoItem> IngresosPorMedioPago { get; set; } = new();
        public int TotalReservas { get; set; }
        public decimal TotalIngresos { get; set; }
    }

    public class ReporteMedioPagoItem
    {
        public string MedioPago { get; set; } = "";
        public int CantidadReservas { get; set; }
        public decimal TotalIngresos { get; set; }
        public double PorcentajeReservas { get; set; }
        public double PorcentajeIngresos { get; set; }
        public decimal PromedioIngreso { get; set; }
    }

    public class ReporteMediosContactoViewModel
    {
        [Display(Name = "Fecha Desde")]
        [DataType(DataType.Date)]
        public DateTime FechaDesde { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        [Display(Name = "Fecha Hasta")]
        [DataType(DataType.Date)]
        public DateTime FechaHasta { get; set; } = DateTime.Now;

        [Display(Name = "Cabaña")]
        public int? CabanaId { get; set; }

        public List<ReporteMedioContactoItem> IngresosPorMedioContacto { get; set; } = new();
        public int TotalReservas { get; set; }
        public decimal TotalIngresos { get; set; }
    }

    public class ReporteMedioContactoItem
    {
        public string MedioContacto { get; set; } = "";
        public int CantidadReservas { get; set; }
        public decimal TotalIngresos { get; set; }
        public double PorcentajeReservas { get; set; }
        public double PorcentajeIngresos { get; set; }
        public decimal PromedioIngreso { get; set; }
    }
}