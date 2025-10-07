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
    public class MesesModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IExportacionService _exportacionService;

        public MesesModel(AppDbContext context, IExportacionService exportacionService)
        {
            _context = context;
            _exportacionService = exportacionService;
        }

        [BindProperty]
        public ReporteMesesViewModel ReporteModel { get; set; } = new();

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

            // Agrupar por mes y calcular estadísticas
            var reservasPorMes = reservas
                .GroupBy(r => new { r.FechaDesde.Year, r.FechaDesde.Month })
                .Select(g => new ReporteMesItem
                {
                    Ano = g.Key.Year,
                    Mes = g.Key.Month,
                    NombreMes = GetNombreMes(g.Key.Month),
                    CantidadReservas = g.Count(),
                    TotalIngresos = g.Sum(r => r.MontoTotal)
                })
                .OrderBy(r => r.Ano)
                .ThenBy(r => r.Mes)
                .ToList();

            // Calcular totales
            var totalReservas = reservasPorMes.Sum(r => r.CantidadReservas);
            var totalIngresos = reservasPorMes.Sum(r => r.TotalIngresos);

            // Calcular porcentajes
            foreach (var item in reservasPorMes)
            {
                item.PorcentajeReservas = totalReservas > 0 ? Math.Round((double)item.CantidadReservas / totalReservas * 100, 1) : 0;
                item.PorcentajeIngresos = totalIngresos > 0 ? Math.Round((double)item.TotalIngresos / (double)totalIngresos * 100, 1) : 0;
            }

            ReporteModel.ReservasPorMes = reservasPorMes;
            ReporteModel.TotalReservas = totalReservas;
            ReporteModel.TotalIngresos = totalIngresos;
        }

        private string GetNombreMes(int mes)
        {
            return mes switch
            {
                1 => "Enero",
                2 => "Febrero",
                3 => "Marzo",
                4 => "Abril",
                5 => "Mayo",
                6 => "Junio",
                7 => "Julio",
                8 => "Agosto",
                9 => "Septiembre",
                10 => "Octubre",
                11 => "Noviembre",
                12 => "Diciembre",
                _ => "Desconocido"
            };
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
                TituloReporte = "Reporte de Reservas por Meses",
                Periodo = $"{ReporteModel.FechaDesde:dd/MM/yyyy} - {ReporteModel.FechaHasta:dd/MM/yyyy}",
                NombreEmpresa = "Aldea Auriel"
            };

            // Definir columnas
            datosExportacion.Columnas.AddRange(new List<ColumnaExportacion>
            {
                new() { Nombre = "Mes", TipoDato = "string", Ancho = 20, Alineacion = "left" },
                new() { Nombre = "Cantidad de Reservas", TipoDato = "number", Ancho = 20, Alineacion = "center" },
                new() { Nombre = "Total Ingresos", TipoDato = "currency", Ancho = 25, Alineacion = "right" },
                new() { Nombre = "% Reservas", TipoDato = "number", Ancho = 15, Alineacion = "center" },
                new() { Nombre = "% Ingresos", TipoDato = "number", Ancho = 15, Alineacion = "center" }
            });

            // Agregar datos
            foreach (var item in ReporteModel.ReservasPorMes)
            {
                var fila = new Dictionary<string, object>
                {
                    ["mes"] = item.NombreMes,
                    ["cantidad_reservas"] = item.CantidadReservas,
                    ["total_ingresos"] = item.TotalIngresos,
                    ["porcentaje_reservas"] = $"{item.PorcentajeReservas}%",
                    ["porcentaje_ingresos"] = $"{item.PorcentajeIngresos}%"
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
                TituloReporte = "Reporte de Reservas por Meses",
                Periodo = $"{ReporteModel.FechaDesde:dd/MM/yyyy} - {ReporteModel.FechaHasta:dd/MM/yyyy}",
                NombreEmpresa = "Aldea Auriel"
            };

            // Definir columnas
            datosExportacion.Columnas.AddRange(new List<ColumnaExportacion>
            {
                new() { Nombre = "Mes", TipoDato = "string", Ancho = 20, Alineacion = "left" },
                new() { Nombre = "Cantidad de Reservas", TipoDato = "number", Ancho = 20, Alineacion = "center" },
                new() { Nombre = "Total Ingresos", TipoDato = "currency", Ancho = 25, Alineacion = "right" },
                new() { Nombre = "% Reservas", TipoDato = "number", Ancho = 15, Alineacion = "center" },
                new() { Nombre = "% Ingresos", TipoDato = "number", Ancho = 15, Alineacion = "center" }
            });

            // Agregar datos
            foreach (var item in ReporteModel.ReservasPorMes)
            {
                var fila = new Dictionary<string, object>
                {
                    ["mes"] = item.NombreMes,
                    ["cantidad_reservas"] = item.CantidadReservas,
                    ["total_ingresos"] = item.TotalIngresos,
                    ["porcentaje_reservas"] = $"{item.PorcentajeReservas}%",
                    ["porcentaje_ingresos"] = $"{item.PorcentajeIngresos}%"
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