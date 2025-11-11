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
    public class EdadesModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IExportacionService _exportacionService;

        public EdadesModel(AppDbContext context, IExportacionService exportacionService)
        {
            _context = context;
            _exportacionService = exportacionService;
        }

        [BindProperty]
        public ReporteEdadesViewModel ReporteModel { get; set; } = new();

        public List<Models.Cabana> Cabanas { get; set; } = new();
        public List<Temporada> Temporadas { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? dniCliente, int? cabanaId, int? temporadaId)
        {
            // Establecer fechas por defecto si no se proporcionan (julio 2025)
            ReporteModel.FechaDesde = fechaDesde ?? new DateTime(2025, 7, 1);
            ReporteModel.FechaHasta = fechaHasta ?? new DateTime(2025, 7, 31);
            ReporteModel.DniCliente = dniCliente;
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
            // Buscar reservas por fecha de estadía y opcionalmente por DNI de cliente
            var query = _context.Reservas
                .Include(r => r.Cliente)
                .Where(r => r.EstadoReserva != "Cancelada" &&
                           r.FechaDesde.Date >= ReporteModel.FechaDesde.Date &&
                           r.FechaDesde.Date <= ReporteModel.FechaHasta.Date);

            // Filtrar por DNI si se proporciona
            if (!string.IsNullOrWhiteSpace(ReporteModel.DniCliente))
            {
                query = query.Where(r => r.Cliente != null && r.Cliente.Dni.Contains(ReporteModel.DniCliente));
            }

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

            // Obtener clientes únicos que hicieron reservas en el período
            var clientesUnicos = reservas
                .Where(r => r.Cliente != null && r.Cliente.FechaNacimiento.HasValue)
                .Select(r => r.Cliente)
                .Distinct()
                .ToList();

            // Calcular edades y agrupar por rangos
            var hoy = DateTime.Today;
            var rangosEdades = new Dictionary<string, (int Clientes, int Reservas, decimal Ingresos)>
            {
                ["18-25"] = (0, 0, 0),
                ["26-30"] = (0, 0, 0),
                ["31-35"] = (0, 0, 0),
                ["36-40"] = (0, 0, 0),
                ["41-50"] = (0, 0, 0),
                ["51-60"] = (0, 0, 0),
                ["61-70"] = (0, 0, 0),
                ["71-80"] = (0, 0, 0)
            };

            foreach (var cliente in clientesUnicos)
            {
                if (cliente.FechaNacimiento.HasValue)
                {
                    var edad = hoy.Year - cliente.FechaNacimiento.Value.Year;
                    if (cliente.FechaNacimiento.Value.Date > hoy.AddYears(-edad))
                        edad--;

                    // Calcular reservas e ingresos del cliente
                    var reservasCliente = reservas.Where(r => r.ClienteId == cliente.Id).ToList();
                    var cantidadReservas = reservasCliente.Count;
                    var totalIngresos = reservasCliente.Sum(r => r.MontoTotal);

                    // Asignar a rango correspondiente
                    if (edad >= 18 && edad <= 25)
                        rangosEdades["18-25"] = (rangosEdades["18-25"].Clientes + 1,
                            rangosEdades["18-25"].Reservas + cantidadReservas,
                            rangosEdades["18-25"].Ingresos + totalIngresos);
                    else if (edad >= 26 && edad <= 30)
                        rangosEdades["26-30"] = (rangosEdades["26-30"].Clientes + 1,
                            rangosEdades["26-30"].Reservas + cantidadReservas,
                            rangosEdades["26-30"].Ingresos + totalIngresos);
                    else if (edad >= 31 && edad <= 35)
                        rangosEdades["31-35"] = (rangosEdades["31-35"].Clientes + 1,
                            rangosEdades["31-35"].Reservas + cantidadReservas,
                            rangosEdades["31-35"].Ingresos + totalIngresos);
                    else if (edad >= 36 && edad <= 40)
                        rangosEdades["36-40"] = (rangosEdades["36-40"].Clientes + 1,
                            rangosEdades["36-40"].Reservas + cantidadReservas,
                            rangosEdades["36-40"].Ingresos + totalIngresos);
                    else if (edad >= 41 && edad <= 50)
                        rangosEdades["41-50"] = (rangosEdades["41-50"].Clientes + 1,
                            rangosEdades["41-50"].Reservas + cantidadReservas,
                            rangosEdades["41-50"].Ingresos + totalIngresos);
                    else if (edad >= 51 && edad <= 60)
                        rangosEdades["51-60"] = (rangosEdades["51-60"].Clientes + 1,
                            rangosEdades["51-60"].Reservas + cantidadReservas,
                            rangosEdades["51-60"].Ingresos + totalIngresos);
                    else if (edad >= 61 && edad <= 70)
                        rangosEdades["61-70"] = (rangosEdades["61-70"].Clientes + 1,
                            rangosEdades["61-70"].Reservas + cantidadReservas,
                            rangosEdades["61-70"].Ingresos + totalIngresos);
                    else if (edad >= 71 && edad <= 80)
                        rangosEdades["71-80"] = (rangosEdades["71-80"].Clientes + 1,
                            rangosEdades["71-80"].Reservas + cantidadReservas,
                            rangosEdades["71-80"].Ingresos + totalIngresos);
                }
            }

            // Crear datos para la gráfica
            ReporteModel.DatosGrafica = rangosEdades
                .Where(r => r.Value.Clientes > 0)
                .Select(r => new DatosEdadRango
                {
                    Rango = r.Key,
                    Cantidad = r.Value.Clientes,
                    CantidadReservas = r.Value.Reservas,
                    TotalIngresos = r.Value.Ingresos
                })
                .OrderBy(r => r.Rango)
                .ToList();

            // Calcular estadísticas
            ReporteModel.TotalClientes = clientesUnicos.Count;
            ReporteModel.TotalReservas = reservas.Count;
            ReporteModel.TotalIngresos = reservas.Sum(r => r.MontoTotal);
            ReporteModel.PromedioEdad = clientesUnicos
                .Where(c => c.FechaNacimiento.HasValue)
                .Select(c => hoy.Year - c.FechaNacimiento.Value.Year - 
                    (c.FechaNacimiento.Value.Date > hoy.AddYears(-(hoy.Year - c.FechaNacimiento.Value.Year)) ? 1 : 0))
                .DefaultIfEmpty(0)
                .Average();
        }

        public async Task<IActionResult> OnGetExportarExcelAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? dniCliente)
        {
            // Configurar los filtros para exportación
            ReporteModel.FechaDesde = fechaDesde ?? new DateTime(2025, 7, 1);
            ReporteModel.FechaHasta = fechaHasta ?? new DateTime(2025, 7, 31);
            ReporteModel.DniCliente = dniCliente;

            // Obtener todos los datos
            await CargarReporte();

            // Preparar datos para exportación
            var datosExportacion = new DatosExportacion
            {
                TituloReporte = "Reporte de Edades de Clientes",
                Periodo = $"{ReporteModel.FechaDesde:dd/MM/yyyy} - {ReporteModel.FechaHasta:dd/MM/yyyy}",
                FiltroAdicional = !string.IsNullOrWhiteSpace(ReporteModel.DniCliente)
                    ? $"Filtrado por DNI: {ReporteModel.DniCliente}"
                    : null,
                NombreEmpresa = "Aldea Auriel"
            };

            // Definir columnas
            datosExportacion.Columnas.AddRange(new List<ColumnaExportacion>
            {
                new() { Nombre = "Rango de Edad", TipoDato = "string", Ancho = 20, Alineacion = "center" },
                new() { Nombre = "Cantidad Clientes", TipoDato = "number", Ancho = 15, Alineacion = "center" },
                new() { Nombre = "Cantidad Reservas", TipoDato = "number", Ancho = 15, Alineacion = "center" },
                new() { Nombre = "Total Ingresos", TipoDato = "currency", Ancho = 20, Alineacion = "right" }
            });

            // Agregar datos
            foreach (var item in ReporteModel.DatosGrafica)
            {
                var fila = new Dictionary<string, object>
                {
                    ["rango_de_edad"] = item.Rango,
                    ["cantidad_clientes"] = item.Cantidad,
                    ["cantidad_reservas"] = item.CantidadReservas,
                    ["total_ingresos"] = item.TotalIngresos
                };
                datosExportacion.Filas.Add(fila);
            }

            // Agregar estadísticas
            datosExportacion.Estadisticas.Add("Total de Clientes", ReporteModel.TotalClientes);
            datosExportacion.Estadisticas.Add("Total de Reservas", ReporteModel.TotalReservas);
            datosExportacion.Estadisticas.Add("Ingresos Totales", ReporteModel.TotalIngresos);
            datosExportacion.Estadisticas.Add("Promedio de Edad", Math.Round(ReporteModel.PromedioEdad, 1));

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

        public async Task<IActionResult> OnGetExportarPdfAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? dniCliente)
        {
            // Configurar los filtros para exportación
            ReporteModel.FechaDesde = fechaDesde ?? new DateTime(2025, 7, 1);
            ReporteModel.FechaHasta = fechaHasta ?? new DateTime(2025, 7, 31);
            ReporteModel.DniCliente = dniCliente;

            // Obtener todos los datos
            await CargarReporte();

            // Preparar datos para exportación
            var datosExportacion = new DatosExportacion
            {
                TituloReporte = "Reporte de Edades de Clientes",
                Periodo = $"{ReporteModel.FechaDesde:dd/MM/yyyy} - {ReporteModel.FechaHasta:dd/MM/yyyy}",
                FiltroAdicional = !string.IsNullOrWhiteSpace(ReporteModel.DniCliente)
                    ? $"Filtrado por DNI: {ReporteModel.DniCliente}"
                    : null,
                NombreEmpresa = "Aldea Auriel"
            };

            // Definir columnas
            datosExportacion.Columnas.AddRange(new List<ColumnaExportacion>
            {
                new() { Nombre = "Rango de Edad", TipoDato = "string", Ancho = 25, Alineacion = "center" },
                new() { Nombre = "Cantidad Clientes", TipoDato = "number", Ancho = 20, Alineacion = "center" },
                new() { Nombre = "Cantidad Reservas", TipoDato = "number", Ancho = 20, Alineacion = "center" },
                new() { Nombre = "Total Ingresos", TipoDato = "currency", Ancho = 25, Alineacion = "right" }
            });

            // Agregar datos
            foreach (var item in ReporteModel.DatosGrafica)
            {
                var fila = new Dictionary<string, object>
                {
                    ["rango_de_edad"] = item.Rango,
                    ["cantidad_clientes"] = item.Cantidad,
                    ["cantidad_reservas"] = item.CantidadReservas,
                    ["total_ingresos"] = item.TotalIngresos
                };
                datosExportacion.Filas.Add(fila);
            }

            // Agregar estadísticas
            datosExportacion.Estadisticas.Add("Total de Clientes", ReporteModel.TotalClientes);
            datosExportacion.Estadisticas.Add("Total de Reservas", ReporteModel.TotalReservas);
            datosExportacion.Estadisticas.Add("Ingresos Totales", ReporteModel.TotalIngresos);
            datosExportacion.Estadisticas.Add("Promedio de Edad", Math.Round(ReporteModel.PromedioEdad, 1));

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
