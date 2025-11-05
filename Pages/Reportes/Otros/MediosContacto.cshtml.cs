using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;
using ReservaCabanasSite.Services;

namespace ReservaCabanasSite.Pages.Reportes.Otros
{
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class MediosContactoModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IExportacionService _exportacionService;

        public MediosContactoModel(AppDbContext context, IExportacionService exportacionService)
        {
            _context = context;
            _exportacionService = exportacionService;
        }

        [BindProperty]
        public ReporteMediosContactoViewModel ReporteModel { get; set; } = new();

        public List<Models.Cabana> Cabanas { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(DateTime? fechaDesde, DateTime? fechaHasta, int? cabanaId)
        {
            // Establecer fechas por defecto (mes actual)
            var hoy = DateTime.Today;
            var primerDiaDelMes = new DateTime(hoy.Year, hoy.Month, 1);
            var ultimoDiaDelMes = primerDiaDelMes.AddMonths(1).AddDays(-1);

            ReporteModel.FechaDesde = fechaDesde ?? primerDiaDelMes;
            ReporteModel.FechaHasta = fechaHasta ?? ultimoDiaDelMes;
            ReporteModel.CabanaId = cabanaId;

            await CargarCatalogos();
            await CargarReporte();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CargarCatalogos();
                return Page();
            }

            // Validar que fecha desde sea menor que fecha hasta
            if (ReporteModel.FechaDesde > ReporteModel.FechaHasta)
            {
                ModelState.AddModelError("ReporteModel.FechaDesde", "La fecha desde debe ser menor o igual a la fecha hasta.");
                await CargarCatalogos();
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
        }

        private async Task CargarReporte()
        {
            // Buscar reservas por fecha de estadía
            var query = _context.Reservas
                .Where(r => r.Activa &&
                           r.FechaDesde.Date >= ReporteModel.FechaDesde.Date &&
                           r.FechaDesde.Date <= ReporteModel.FechaHasta.Date);

            // Filtrar por cabaña si se seleccionó
            if (ReporteModel.CabanaId.HasValue)
            {
                query = query.Where(r => r.CabanaId == ReporteModel.CabanaId.Value);
            }

            var reservas = await query.ToListAsync();

            // Agrupar por medio de contacto y calcular estadísticas
            var ingresosPorMedioContacto = reservas
                .GroupBy(r => string.IsNullOrEmpty(r.MedioContacto) ? "Sin especificar" : r.MedioContacto)
                .Select(g => new ReporteMedioContactoItem
                {
                    MedioContacto = g.Key,
                    CantidadReservas = g.Count(),
                    TotalIngresos = g.Sum(r => r.MontoTotal),
                    PromedioIngreso = g.Average(r => r.MontoTotal)
                })
                .OrderByDescending(r => r.TotalIngresos)
                .ToList();

            // Calcular totales
            var totalReservas = ingresosPorMedioContacto.Sum(r => r.CantidadReservas);
            var totalIngresos = ingresosPorMedioContacto.Sum(r => r.TotalIngresos);

            // Calcular porcentajes
            foreach (var item in ingresosPorMedioContacto)
            {
                item.PorcentajeReservas = totalReservas > 0 ? Math.Round((double)item.CantidadReservas / totalReservas * 100, 1) : 0;
                item.PorcentajeIngresos = totalIngresos > 0 ? Math.Round((double)item.TotalIngresos / (double)totalIngresos * 100, 1) : 0;
            }

            ReporteModel.IngresosPorMedioContacto = ingresosPorMedioContacto;
            ReporteModel.TotalReservas = totalReservas;
            ReporteModel.TotalIngresos = totalIngresos;
        }

        public async Task<IActionResult> OnGetExportarExcelAsync(DateTime? fechaDesde, DateTime? fechaHasta, int? cabanaId)
        {
            // Configurar los filtros para exportación
            var hoy = DateTime.Today;
            var primerDiaDelMes = new DateTime(hoy.Year, hoy.Month, 1);
            var ultimoDiaDelMes = primerDiaDelMes.AddMonths(1).AddDays(-1);

            ReporteModel.FechaDesde = fechaDesde ?? primerDiaDelMes;
            ReporteModel.FechaHasta = fechaHasta ?? ultimoDiaDelMes;
            ReporteModel.CabanaId = cabanaId;

            // Obtener todos los datos
            await CargarReporte();

            // Preparar datos para exportación
            var datosExportacion = new DatosExportacion
            {
                TituloReporte = "Reporte de Ingresos por Medio de Contacto",
                Periodo = $"{ReporteModel.FechaDesde:dd/MM/yyyy} - {ReporteModel.FechaHasta:dd/MM/yyyy}",
                NombreEmpresa = "Aldea Auriel"
            };

            // Definir columnas
            datosExportacion.Columnas.AddRange(new List<ColumnaExportacion>
            {
                new() { Nombre = "Medio de Contacto", TipoDato = "string", Ancho = 30, Alineacion = "left" },
                new() { Nombre = "Cantidad de Reservas", TipoDato = "number", Ancho = 20, Alineacion = "center" },
                new() { Nombre = "Total Ingresos", TipoDato = "currency", Ancho = 20, Alineacion = "right" },
                new() { Nombre = "Promedio por Reserva", TipoDato = "currency", Ancho = 20, Alineacion = "right" },
                new() { Nombre = "% Ingresos", TipoDato = "percentage", Ancho = 15, Alineacion = "center" }
            });

            // Agregar datos
            foreach (var item in ReporteModel.IngresosPorMedioContacto)
            {
                var fila = new Dictionary<string, object>
                {
                    ["medio_de_contacto"] = item.MedioContacto,
                    ["cantidad_de_reservas"] = item.CantidadReservas,
                    ["total_ingresos"] = item.TotalIngresos,
                    ["promedio_por_reserva"] = item.PromedioIngreso,
                    ["%_ingresos"] = item.PorcentajeIngresos / 100
                };
                datosExportacion.Filas.Add(fila);
            }

            // Agregar estadísticas
            datosExportacion.Estadisticas.Add("Total de Reservas", ReporteModel.TotalReservas);
            datosExportacion.Estadisticas.Add("Total de Ingresos", ReporteModel.TotalIngresos);
            datosExportacion.Estadisticas.Add("Promedio General", ReporteModel.TotalReservas > 0 ? ReporteModel.TotalIngresos / ReporteModel.TotalReservas : 0);

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

        public async Task<IActionResult> OnGetExportarPdfAsync(DateTime? fechaDesde, DateTime? fechaHasta, int? cabanaId)
        {
            // Configurar los filtros para exportación
            var hoy = DateTime.Today;
            var primerDiaDelMes = new DateTime(hoy.Year, hoy.Month, 1);
            var ultimoDiaDelMes = primerDiaDelMes.AddMonths(1).AddDays(-1);

            ReporteModel.FechaDesde = fechaDesde ?? primerDiaDelMes;
            ReporteModel.FechaHasta = fechaHasta ?? ultimoDiaDelMes;
            ReporteModel.CabanaId = cabanaId;

            // Obtener todos los datos
            await CargarReporte();

            // Preparar datos para exportación
            var datosExportacion = new DatosExportacion
            {
                TituloReporte = "Reporte de Ingresos por Medio de Contacto",
                Periodo = $"{ReporteModel.FechaDesde:dd/MM/yyyy} - {ReporteModel.FechaHasta:dd/MM/yyyy}",
                NombreEmpresa = "Aldea Auriel"
            };

            // Definir columnas
            datosExportacion.Columnas.AddRange(new List<ColumnaExportacion>
            {
                new() { Nombre = "Medio de Contacto", TipoDato = "string", Ancho = 25, Alineacion = "left" },
                new() { Nombre = "Cantidad de Reservas", TipoDato = "number", Ancho = 18, Alineacion = "center" },
                new() { Nombre = "Total Ingresos", TipoDato = "currency", Ancho = 20, Alineacion = "right" },
                new() { Nombre = "Promedio", TipoDato = "currency", Ancho = 20, Alineacion = "right" },
                new() { Nombre = "% Ingresos", TipoDato = "percentage", Ancho = 17, Alineacion = "center" }
            });

            // Agregar datos
            foreach (var item in ReporteModel.IngresosPorMedioContacto)
            {
                var fila = new Dictionary<string, object>
                {
                    ["medio_de_contacto"] = item.MedioContacto,
                    ["cantidad_de_reservas"] = item.CantidadReservas,
                    ["total_ingresos"] = item.TotalIngresos,
                    ["promedio_por_reserva"] = item.PromedioIngreso,
                    ["%_ingresos"] = item.PorcentajeIngresos / 100
                };
                datosExportacion.Filas.Add(fila);
            }

            // Agregar estadísticas
            datosExportacion.Estadisticas.Add("Total de Reservas", ReporteModel.TotalReservas);
            datosExportacion.Estadisticas.Add("Total de Ingresos", ReporteModel.TotalIngresos);
            datosExportacion.Estadisticas.Add("Promedio General", ReporteModel.TotalReservas > 0 ? ReporteModel.TotalIngresos / ReporteModel.TotalReservas : 0);

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
