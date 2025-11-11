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
    public class CabanaModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IExportacionService _exportacionService;

        public CabanaModel(AppDbContext context, IExportacionService exportacionService)
        {
            _context = context;
            _exportacionService = exportacionService;
        }

        [BindProperty]
        public ReporteCabanasViewModel ReporteModel { get; set; } = new();

        public List<Models.Cabana> Cabanas { get; set; } = new();
        public List<Temporada> Temporadas { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(DateTime? fechaDesde, DateTime? fechaHasta, int? cabanaId, int? temporadaId)
        {
            // Establecer fechas por defecto si no se proporcionan (mes actual)
            var hoy = DateTime.Today;
            var primerDiaDelMes = new DateTime(hoy.Year, hoy.Month, 1);
            var ultimoDiaDelMes = primerDiaDelMes.AddMonths(1).AddDays(-1);
            
            ReporteModel.FechaDesde = fechaDesde ?? primerDiaDelMes;
            ReporteModel.FechaHasta = fechaHasta ?? ultimoDiaDelMes;
            ReporteModel.CabanaId = cabanaId;
            ReporteModel.TemporadaId = temporadaId;

            await CargarCatalogos();
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

            await CargarCatalogos();
            await CargarReporte();
            return Page();
        }

        private async Task CargarCatalogos()
        {
            Cabanas = await _context.Cabanas
                .Where(c => c.Activa)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            Temporadas = await _context.Temporadas
                .Where(t => t.Activa)
                .OrderBy(t => t.Nombre)
                .ToListAsync();
        }

        private async Task CargarReporte()
        {
            // Buscar reservas por fecha de estadía - incluir Confirmadas y Finalizadas, excluir Canceladas
            var query = _context.Reservas
                .Include(r => r.Cabana)
                .Where(r => r.EstadoReserva != "Cancelada" &&
                           r.FechaDesde.Date >= ReporteModel.FechaDesde.Date &&
                           r.FechaDesde.Date <= ReporteModel.FechaHasta.Date);

            // Filtrar por cabaña si se seleccionó
            if (ReporteModel.CabanaId.HasValue)
            {
                query = query.Where(r => r.CabanaId == ReporteModel.CabanaId.Value);
            }

            // Filtrar por temporada si se seleccionó
            if (ReporteModel.TemporadaId.HasValue)
            {
                query = query.Where(r => r.TemporadaId == ReporteModel.TemporadaId.Value);
            }

            var reservas = await query.ToListAsync();

            // Agrupar por cabaña y calcular estadísticas
            var reservasPorCabana = reservas
                .Where(r => r.Cabana != null)
                .GroupBy(r => r.CabanaId)
                .Select(g => new ReporteCabanaItem
                {
                    CabanaId = g.Key,
                    CabanaNombre = g.First().Cabana?.Nombre ?? "Sin cabaña",
                    CantidadReservas = g.Count(),
                    CantidadDiasReservados = g.Sum(r => (r.FechaHasta - r.FechaDesde).Days + 1),
                    TotalIngresos = g.Sum(r => r.MontoTotal)
                })
                .OrderByDescending(r => r.CantidadReservas)
                .ThenByDescending(r => r.CantidadDiasReservados)
                .ThenBy(r => r.CabanaNombre)
                .ToList();

            // Calcular totales
            var totalReservas = reservasPorCabana.Sum(r => r.CantidadReservas);
            var totalDias = reservasPorCabana.Sum(r => r.CantidadDiasReservados);
            var totalIngresos = reservasPorCabana.Sum(r => r.TotalIngresos);

            // Calcular porcentajes
            foreach (var item in reservasPorCabana)
            {
                item.PorcentajeReservas = totalReservas > 0 ? Math.Round((double)item.CantidadReservas / totalReservas * 100, 1) : 0;
                item.PorcentajeDias = totalDias > 0 ? Math.Round((double)item.CantidadDiasReservados / totalDias * 100, 1) : 0;
                item.PorcentajeIngresos = totalIngresos > 0 ? Math.Round((double)item.TotalIngresos / (double)totalIngresos * 100, 1) : 0;
            }

            ReporteModel.ReservasPorCabana = reservasPorCabana;
            ReporteModel.TotalReservas = totalReservas;
            ReporteModel.TotalDias = totalDias;
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
                TituloReporte = "Reporte de Reservas por Cabaña",
                Periodo = $"{ReporteModel.FechaDesde:dd/MM/yyyy} - {ReporteModel.FechaHasta:dd/MM/yyyy}",
                NombreEmpresa = "Aldea Auriel"
            };

            // Definir columnas
            datosExportacion.Columnas.AddRange(new List<ColumnaExportacion>
            {
                new() { Nombre = "Cabaña", TipoDato = "string", Ancho = 30, Alineacion = "left" },
                new() { Nombre = "Cantidad de Reservas", TipoDato = "number", Ancho = 20, Alineacion = "center" },
                new() { Nombre = "Días Reservados", TipoDato = "number", Ancho = 20, Alineacion = "center" },
                new() { Nombre = "Total Ingresos", TipoDato = "currency", Ancho = 20, Alineacion = "right" },
                new() { Nombre = "% Reservas", TipoDato = "number", Ancho = 15, Alineacion = "center" }
            });

            // Agregar datos
            foreach (var item in ReporteModel.ReservasPorCabana)
            {
                var fila = new Dictionary<string, object>
                {
                    ["cabaña"] = item.CabanaNombre,
                    ["cantidad_de_reservas"] = item.CantidadReservas,
                    ["días_reservados"] = item.CantidadDiasReservados,
                    ["total_ingresos"] = item.TotalIngresos,
                    ["%_reservas"] = $"{item.PorcentajeReservas}%"
                };
                datosExportacion.Filas.Add(fila);
            }

            // Agregar estadísticas
            datosExportacion.Estadisticas.Add("Total de Reservas", ReporteModel.TotalReservas);
            datosExportacion.Estadisticas.Add("Total de Días", ReporteModel.TotalDias);
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
                TituloReporte = "Reporte de Reservas por Cabaña",
                Periodo = $"{ReporteModel.FechaDesde:dd/MM/yyyy} - {ReporteModel.FechaHasta:dd/MM/yyyy}",
                NombreEmpresa = "Aldea Auriel"
            };

            // Definir columnas
            datosExportacion.Columnas.AddRange(new List<ColumnaExportacion>
            {
                new() { Nombre = "Cabaña", TipoDato = "string", Ancho = 25, Alineacion = "left" },
                new() { Nombre = "Cantidad de Reservas", TipoDato = "number", Ancho = 20, Alineacion = "center" },
                new() { Nombre = "Días Reservados", TipoDato = "number", Ancho = 20, Alineacion = "center" },
                new() { Nombre = "Total Ingresos", TipoDato = "currency", Ancho = 20, Alineacion = "right" },
                new() { Nombre = "% Reservas", TipoDato = "number", Ancho = 15, Alineacion = "center" }
            });

            // Agregar datos
            foreach (var item in ReporteModel.ReservasPorCabana)
            {
                var fila = new Dictionary<string, object>
                {
                    ["cabaña"] = item.CabanaNombre,
                    ["cantidad_de_reservas"] = item.CantidadReservas,
                    ["días_reservados"] = item.CantidadDiasReservados,
                    ["total_ingresos"] = item.TotalIngresos,
                    ["%_reservas"] = $"{item.PorcentajeReservas}%"
                };
                datosExportacion.Filas.Add(fila);
            }

            // Agregar estadísticas
            datosExportacion.Estadisticas.Add("Total de Reservas", ReporteModel.TotalReservas);
            datosExportacion.Estadisticas.Add("Total de Días", ReporteModel.TotalDias);
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