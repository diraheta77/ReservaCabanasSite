using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;
using ReservaCabanasSite.Services;

namespace ReservaCabanasSite.Pages.Reportes.Cabana
{
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class TemporadaModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IExportacionService _exportacionService;

        public TemporadaModel(AppDbContext context, IExportacionService exportacionService)
        {
            _context = context;
            _exportacionService = exportacionService;
        }

        [BindProperty]
        public ReporteTemporadasViewModel ReporteModel { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            // Establecer fechas por defecto si no se proporcionan (mes actual)
            var hoy = DateTime.Today;
            var primerDiaDelMes = new DateTime(hoy.Year, hoy.Month, 1);
            var ultimoDiaDelMes = primerDiaDelMes.AddMonths(1).AddDays(-1);
            
            ReporteModel.FechaDesde = fechaDesde ?? primerDiaDelMes;
            ReporteModel.FechaHasta = fechaHasta ?? ultimoDiaDelMes;

            await CargarReporte();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Validar que fecha desde sea menor que fecha hasta
            if (ReporteModel.FechaDesde > ReporteModel.FechaHasta)
            {
                ModelState.AddModelError("ReporteModel.FechaDesde", "La fecha desde debe ser menor o igual a la fecha hasta.");
                return Page();
            }

            await CargarReporte();
            return Page();
        }

        private async Task CargarReporte()
        {
            // Buscar reservas por fecha de estadía
            var reservas = await _context.Reservas
                .Where(r => r.Activa &&
                           r.FechaDesde.Date >= ReporteModel.FechaDesde.Date &&
                           r.FechaDesde.Date <= ReporteModel.FechaHasta.Date)
                .ToListAsync();

            // Obtener IDs de temporadas
            var temporadaIds = reservas
                .Where(r => r.TemporadaId.HasValue)
                .Select(r => r.TemporadaId.Value)
                .Distinct()
                .ToList();

            // Obtener temporadas
            var temporadas = await _context.Temporadas
                .Where(t => temporadaIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.Nombre);

            // Agrupar por temporada y calcular estadísticas
            var reservasPorTemporada = reservas
                .Where(r => r.TemporadaId.HasValue)
                .GroupBy(r => r.TemporadaId.Value)
                .Select(g => new ReporteTemporadaItem
                {
                    TemporadaId = g.Key,
                    TemporadaNombre = temporadas.ContainsKey(g.Key) ? temporadas[g.Key] : "Sin temporada",
                    CantidadReservas = g.Count(),
                    TotalIngresos = g.Sum(r => r.MontoTotal)
                })
                .OrderByDescending(r => r.CantidadReservas)
                .ThenBy(r => r.TemporadaNombre)
                .ToList();

            // Calcular totales
            var totalReservas = reservasPorTemporada.Sum(r => r.CantidadReservas);
            var totalIngresos = reservasPorTemporada.Sum(r => r.TotalIngresos);

            // Calcular porcentajes
            foreach (var item in reservasPorTemporada)
            {
                item.PorcentajeReservas = totalReservas > 0 ? Math.Round((double)item.CantidadReservas / totalReservas * 100, 1) : 0;
                item.PorcentajeIngresos = totalIngresos > 0 ? Math.Round((double)item.TotalIngresos / (double)totalIngresos * 100, 1) : 0;
            }

            ReporteModel.ReservasPorTemporada = reservasPorTemporada;
            ReporteModel.TotalReservas = totalReservas;
            ReporteModel.TotalIngresos = totalIngresos;
        }

        public async Task<IActionResult> OnGetExportarExcelAsync(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            // Configurar los filtros para exportación
            var hoy = DateTime.Today;
            var primerDiaDelMes = new DateTime(hoy.Year, hoy.Month, 1);
            var ultimoDiaDelMes = primerDiaDelMes.AddMonths(1).AddDays(-1);
            
            ReporteModel.FechaDesde = fechaDesde ?? primerDiaDelMes;
            ReporteModel.FechaHasta = fechaHasta ?? ultimoDiaDelMes;

            // Obtener todos los datos
            await CargarReporte();

            // Preparar datos para exportación
            var datosExportacion = new DatosExportacion
            {
                TituloReporte = "Reporte de Reservas por Temporada",
                Periodo = $"{ReporteModel.FechaDesde:dd/MM/yyyy} - {ReporteModel.FechaHasta:dd/MM/yyyy}",
                NombreEmpresa = "Aldea Auriel"
            };

            // Definir columnas
            datosExportacion.Columnas.AddRange(new List<ColumnaExportacion>
            {
                new() { Nombre = "Temporada", TipoDato = "string", Ancho = 30, Alineacion = "left" },
                new() { Nombre = "Cantidad de Reservas", TipoDato = "number", Ancho = 25, Alineacion = "center" },
                new() { Nombre = "Total Ingresos", TipoDato = "currency", Ancho = 25, Alineacion = "right" },
                new() { Nombre = "% Reservas", TipoDato = "number", Ancho = 20, Alineacion = "center" }
            });

            // Agregar datos
            foreach (var item in ReporteModel.ReservasPorTemporada)
            {
                var fila = new Dictionary<string, object>
                {
                    ["temporada"] = item.TemporadaNombre,
                    ["cantidad_reservas"] = item.CantidadReservas,
                    ["total_ingresos"] = item.TotalIngresos,
                    ["porcentaje_reservas"] = $"{item.PorcentajeReservas}%"
                };
                datosExportacion.Filas.Add(fila);
            }

            // Agregar estadísticas
            datosExportacion.Estadisticas.Add("Total de Reservas", ReporteModel.TotalReservas);
            datosExportacion.Estadisticas.Add("Total de Ingresos", ReporteModel.TotalIngresos);

            // Generar Excel
            var resultado = await _exportacionService.ExportarAExcel(datosExportacion);

            if (resultado.Exito)
            {
                return File(resultado.Archivo, resultado.TipoContenido, resultado.NombreArchivo);
            }
            else
            {
                TempData["Error"] = resultado.Error;
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnGetExportarPdfAsync(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            // Configurar los filtros para exportación
            var hoy = DateTime.Today;
            var primerDiaDelMes = new DateTime(hoy.Year, hoy.Month, 1);
            var ultimoDiaDelMes = primerDiaDelMes.AddMonths(1).AddDays(-1);
            
            ReporteModel.FechaDesde = fechaDesde ?? primerDiaDelMes;
            ReporteModel.FechaHasta = fechaHasta ?? ultimoDiaDelMes;

            // Obtener todos los datos
            await CargarReporte();

            // Preparar datos para exportación
            var datosExportacion = new DatosExportacion
            {
                TituloReporte = "Reporte de Reservas por Temporada",
                Periodo = $"{ReporteModel.FechaDesde:dd/MM/yyyy} - {ReporteModel.FechaHasta:dd/MM/yyyy}",
                NombreEmpresa = "Aldea Auriel"
            };

            // Definir columnas
            datosExportacion.Columnas.AddRange(new List<ColumnaExportacion>
            {
                new() { Nombre = "Temporada", TipoDato = "string", Ancho = 30, Alineacion = "left" },
                new() { Nombre = "Cantidad de Reservas", TipoDato = "number", Ancho = 25, Alineacion = "center" },
                new() { Nombre = "Total Ingresos", TipoDato = "currency", Ancho = 25, Alineacion = "right" },
                new() { Nombre = "% Reservas", TipoDato = "number", Ancho = 20, Alineacion = "center" }
            });

            // Agregar datos
            foreach (var item in ReporteModel.ReservasPorTemporada)
            {
                var fila = new Dictionary<string, object>
                {
                    ["temporada"] = item.TemporadaNombre,
                    ["cantidad_reservas"] = item.CantidadReservas,
                    ["total_ingresos"] = item.TotalIngresos,
                    ["porcentaje_reservas"] = $"{item.PorcentajeReservas}%"
                };
                datosExportacion.Filas.Add(fila);
            }

            // Agregar estadísticas
            datosExportacion.Estadisticas.Add("Total de Reservas", ReporteModel.TotalReservas);
            datosExportacion.Estadisticas.Add("Total de Ingresos", ReporteModel.TotalIngresos);

            // Generar PDF
            var resultado = await _exportacionService.ExportarAPdf(datosExportacion);

            if (resultado.Exito)
            {
                return File(resultado.Archivo, resultado.TipoContenido, resultado.NombreArchivo);
            }
            else
            {
                TempData["Error"] = resultado.Error;
                return RedirectToPage();
            }
        }
    }
}