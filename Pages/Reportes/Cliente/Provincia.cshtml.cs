using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;
using ReservaCabanasSite.Services;

namespace ReservaCabanasSite.Pages.Reportes.Cliente
{
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class ProvinciaModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IExportacionService _exportacionService;

        public ProvinciaModel(AppDbContext context, IExportacionService exportacionService)
        {
            _context = context;
            _exportacionService = exportacionService;
        }

        [BindProperty]
        public ReporteReservacionesViewModel ReporteModel { get; set; } = new();

        public List<Models.Cabana> Cabanas { get; set; } = new();
        public List<Temporada> Temporadas { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(DateTime? fechaDesde, DateTime? fechaHasta, int? cabanaId, int? temporadaId)
        {
            // Establecer fechas por defecto si no se proporcionan (julio 2025)
            ReporteModel.FechaDesde = fechaDesde ?? new DateTime(2025, 7, 1);
            ReporteModel.FechaHasta = fechaHasta ?? new DateTime(2025, 7, 31);
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

            Temporadas = await _context.Temporadas
                .Where(t => t.Activa)
                .OrderBy(t => t.Nombre)
                .ToListAsync();
        }

        private async Task CargarReporte()
        {
            // Buscar reservas por fecha de estadía y agrupar por provincia
            var query = _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Cabana)
                .Where(r => r.Activa &&
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

            // Agrupar por provincia y calcular estadísticas
            var reservasPorProvincia = reservas
                .Where(r => r.Cliente != null && !string.IsNullOrEmpty(r.Cliente.Provincia))
                .GroupBy(r => r.Cliente.Provincia)
                .Select(g => new ReporteReservacionItem
                {
                    ClienteId = 0, // No aplica para agrupación por provincia
                    ClienteNombre = g.Key, // Provincia
                    ClienteApellido = "", // No aplica
                    ClienteDni = "N/A",
                    ClienteEmail = "N/A",
                    ClienteTelefono = "N/A",
                    TotalReservaciones = g.Count(),
                    MontoTotalAcumulado = g.Sum(r => r.MontoTotal),
                    TotalPersonasAcumulado = g.Sum(r => r.CantidadPersonas),
                    UltimaReservaFecha = g.Max(r => r.FechaCreacion),
                    PrimeraReservaFecha = g.Min(r => r.FechaCreacion),
                    UltimaCabana = g.OrderByDescending(r => r.FechaCreacion).First().Cabana?.Nombre ?? "N/A",
                    UltimoEstado = g.OrderByDescending(r => r.FechaCreacion).First().EstadoReserva ?? "N/A"
                })
                .OrderByDescending(r => r.TotalReservaciones)
                .ThenByDescending(r => r.MontoTotalAcumulado)
                .ThenBy(r => r.ClienteNombre) // Provincia
                .ToList();

            ReporteModel.Reservaciones = reservasPorProvincia;
        }

        public async Task<IActionResult> OnGetExportarExcelAsync(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            // Configurar los filtros para exportación
            ReporteModel.FechaDesde = fechaDesde ?? new DateTime(2025, 7, 1);
            ReporteModel.FechaHasta = fechaHasta ?? new DateTime(2025, 7, 31);

            // Obtener todos los datos (sin paginación para exportación completa)
            await CargarReporte();

            // Preparar datos para exportación
            var datosExportacion = new DatosExportacion
            {
                TituloReporte = "Reporte de Reservaciones por Provincia",
                Periodo = $"{ReporteModel.FechaDesde:dd/MM/yyyy} - {ReporteModel.FechaHasta:dd/MM/yyyy}",
                NombreEmpresa = "Aldea Uruel"
            };

            // Definir columnas
            datosExportacion.Columnas.AddRange(new List<ColumnaExportacion>
            {
                new() { Nombre = "Provincia", TipoDato = "string", Ancho = 30, Alineacion = "left" },
                new() { Nombre = "Total Reservas", TipoDato = "number", Ancho = 20, Alineacion = "center" },
                new() { Nombre = "Total Gastado", TipoDato = "currency", Ancho = 25, Alineacion = "right" }
            });

            // Agregar datos
            foreach (var item in ReporteModel.Reservaciones)
            {
                var fila = new Dictionary<string, object>
                {
                    ["provincia"] = item.ClienteNombre,
                    ["total_reservas"] = item.TotalReservaciones,
                    ["total_gastado"] = item.MontoTotalAcumulado
                };
                datosExportacion.Filas.Add(fila);
            }

            // Agregar estadísticas
            datosExportacion.Estadisticas.Add("Total de Provincias", ReporteModel.TotalClientes);
            datosExportacion.Estadisticas.Add("Total de Reservaciones", ReporteModel.TotalReservaciones);
            datosExportacion.Estadisticas.Add("Ingresos Totales", ReporteModel.TotalIngresos);
            datosExportacion.Estadisticas.Add("Total de Personas", ReporteModel.TotalPersonas);

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
            ReporteModel.FechaDesde = fechaDesde ?? new DateTime(2025, 7, 1);
            ReporteModel.FechaHasta = fechaHasta ?? new DateTime(2025, 7, 31);

            // Obtener todos los datos (sin paginación para exportación completa)
            await CargarReporte();

            // Preparar datos para exportación
            var datosExportacion = new DatosExportacion
            {
                TituloReporte = "Reporte de Reservaciones por Provincia",
                Periodo = $"{ReporteModel.FechaDesde:dd/MM/yyyy} - {ReporteModel.FechaHasta:dd/MM/yyyy}",
                NombreEmpresa = "Aldea Uruel"
            };

            // Definir columnas
            datosExportacion.Columnas.AddRange(new List<ColumnaExportacion>
            {
                new() { Nombre = "Provincia", TipoDato = "string", Ancho = 30, Alineacion = "left" },
                new() { Nombre = "Total Reservas", TipoDato = "number", Ancho = 20, Alineacion = "center" },
                new() { Nombre = "Total Gastado", TipoDato = "currency", Ancho = 25, Alineacion = "right" }
            });

            // Agregar datos
            foreach (var item in ReporteModel.Reservaciones)
            {
                var fila = new Dictionary<string, object>
                {
                    ["provincia"] = item.ClienteNombre,
                    ["total_reservas"] = item.TotalReservaciones,
                    ["total_gastado"] = item.MontoTotalAcumulado
                };
                datosExportacion.Filas.Add(fila);
            }

            // Agregar estadísticas
            datosExportacion.Estadisticas.Add("Total de Provincias", ReporteModel.TotalClientes);
            datosExportacion.Estadisticas.Add("Total de Reservaciones", ReporteModel.TotalReservaciones);
            datosExportacion.Estadisticas.Add("Ingresos Totales", ReporteModel.TotalIngresos);
            datosExportacion.Estadisticas.Add("Total de Personas", ReporteModel.TotalPersonas);

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