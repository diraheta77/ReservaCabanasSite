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
    public class ClientesModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IExportacionService _exportacionService;

        public ClientesModel(AppDbContext context, IExportacionService exportacionService)
        {
            _context = context;
            _exportacionService = exportacionService;
        }

        [BindProperty]
        public ReporteReservacionesViewModel ReporteModel { get; set; } = new();

        public List<Models.Cabana> Cabanas { get; set; } = new();
        public List<Temporada> Temporadas { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? dniCliente, int? cabanaId, int? temporadaId, int? pagina)
        {
            // Establecer fechas por defecto si no se proporcionan (mes actual)
            var hoy = DateTime.Today;
            var primerDiaDelMes = new DateTime(hoy.Year, hoy.Month, 1);
            var ultimoDiaDelMes = primerDiaDelMes.AddMonths(1).AddDays(-1);

            ReporteModel.FechaDesde = fechaDesde ?? primerDiaDelMes;
            ReporteModel.FechaHasta = fechaHasta ?? ultimoDiaDelMes;
            ReporteModel.DniCliente = dniCliente;
            ReporteModel.CabanaId = cabanaId;
            ReporteModel.TemporadaId = temporadaId;
            ReporteModel.PaginaActual = pagina ?? 1;

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
                .Include(r => r.Cabana)
                .Where(r => r.Activa &&
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

            // Agrupar por cliente y calcular estadísticas (SIN paginación para estadísticas completas)
            var reservasPorClienteCompleto = reservas
                .Where(r => r.Cliente != null)
                .GroupBy(r => r.ClienteId)
                .Select(g => new ReporteReservacionItem
                {
                    ClienteId = g.Key ?? 0,
                    ClienteNombre = g.First().Cliente?.Nombre ?? "Sin cliente",
                    ClienteApellido = g.First().Cliente?.Apellido ?? "",
                    ClienteDni = g.First().Cliente?.Dni ?? "N/A",
                    ClienteEmail = g.First().Cliente?.Email ?? "N/A",
                    ClienteTelefono = g.First().Cliente?.Telefono ?? "N/A",
                    TotalReservaciones = g.Count(),
                    MontoTotalAcumulado = g.Sum(r => r.MontoTotal),
                    TotalPersonasAcumulado = g.Sum(r => r.CantidadPersonas),
                    UltimaReservaFecha = g.Max(r => r.FechaDesde),
                    PrimeraReservaFecha = g.Min(r => r.FechaDesde),
                    UltimaCabana = g.OrderByDescending(r => r.FechaCreacion).First().Cabana?.Nombre ?? "N/A",
                    UltimoEstado = g.OrderByDescending(r => r.FechaCreacion).First().EstadoReserva ?? "N/A"
                })
                .OrderByDescending(r => r.TotalReservaciones)
                .ThenByDescending(r => r.UltimaReservaFecha)
                .ThenBy(r => r.ClienteApellido)
                .ThenBy(r => r.ClienteNombre)
                .ToList();

            // Calcular estadísticas completas
            ReporteModel.TotalRegistros = reservasPorClienteCompleto.Count;
            ReporteModel.TotalClientesCompleto = reservasPorClienteCompleto.Count;
            ReporteModel.TotalReservacionesCompleto = reservasPorClienteCompleto.Sum(r => r.TotalReservaciones);
            ReporteModel.TotalIngresosCompleto = reservasPorClienteCompleto.Sum(r => r.MontoTotalAcumulado);
            ReporteModel.TotalPersonasCompleto = reservasPorClienteCompleto.Sum(r => r.TotalPersonasAcumulado);

            // Aplicar paginación
            var reservasPaginadas = reservasPorClienteCompleto
                .Skip((ReporteModel.PaginaActual - 1) * ReporteModel.TamanoPagina)
                .Take(ReporteModel.TamanoPagina)
                .ToList();

            ReporteModel.Reservaciones = reservasPaginadas;
        }

        public async Task<IActionResult> OnGetExportarExcelAsync(DateTime? fechaDesde, DateTime? fechaHasta, string? dniCliente)
        {
            // Configurar los filtros para exportación
            var hoy = DateTime.Today;
            var primerDiaDelMes = new DateTime(hoy.Year, hoy.Month, 1);
            var ultimoDiaDelMes = primerDiaDelMes.AddMonths(1).AddDays(-1);
            
            ReporteModel.FechaDesde = fechaDesde ?? primerDiaDelMes;
            ReporteModel.FechaHasta = fechaHasta ?? ultimoDiaDelMes;
            ReporteModel.DniCliente = dniCliente;

            // Obtener todos los datos (sin paginación para exportación completa)
            await CargarDatosCompletos();

            // Preparar datos para exportación
            var datosExportacion = new DatosExportacion
            {
                TituloReporte = "Reporte de Reservas por Cliente",
                Periodo = $"{ReporteModel.FechaDesde:dd/MM/yyyy} - {ReporteModel.FechaHasta:dd/MM/yyyy}",
                FiltroAdicional = !string.IsNullOrWhiteSpace(ReporteModel.DniCliente)
                    ? $"Filtrado por DNI: {ReporteModel.DniCliente}"
                    : null,
                NombreEmpresa = "Aldea Auriel"
            };

            // Definir columnas
            datosExportacion.Columnas.AddRange(new List<ColumnaExportacion>
            {
                new() { Nombre = "Cliente", TipoDato = "string", Ancho = 25, Alineacion = "left" },
                new() { Nombre = "Cantidad Reservas", TipoDato = "number", Ancho = 15, Alineacion = "center" },
                new() { Nombre = "Última Reserva", TipoDato = "date", Ancho = 20, Alineacion = "center" }
            });

            // Agregar datos
            foreach (var item in ReporteModel.Reservaciones)
            {
                var fila = new Dictionary<string, object>
                {
                    ["cliente"] = $"{item.ClienteNombre} {item.ClienteApellido}",
                    ["cantidad_reservas"] = item.TotalReservaciones,
                    ["última_reserva"] = item.UltimaReservaFecha.ToString("dd/MM/yyyy")
                };
                datosExportacion.Filas.Add(fila);
            }

            // Agregar estadísticas
            datosExportacion.Estadisticas.Add("Total de Clientes", ReporteModel.TotalClientesCompleto);
            datosExportacion.Estadisticas.Add("Total de Reservaciones", ReporteModel.TotalReservacionesCompleto);
            datosExportacion.Estadisticas.Add("Ingresos Totales", ReporteModel.TotalIngresosCompleto);
            datosExportacion.Estadisticas.Add("Total de Personas", ReporteModel.TotalPersonasCompleto);

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
            var hoy = DateTime.Today;
            var primerDiaDelMes = new DateTime(hoy.Year, hoy.Month, 1);
            var ultimoDiaDelMes = primerDiaDelMes.AddMonths(1).AddDays(-1);
            
            ReporteModel.FechaDesde = fechaDesde ?? primerDiaDelMes;
            ReporteModel.FechaHasta = fechaHasta ?? ultimoDiaDelMes;
            ReporteModel.DniCliente = dniCliente;

            // Obtener todos los datos (sin paginación para exportación completa)
            await CargarDatosCompletos();

            // Preparar datos para exportación
            var datosExportacion = new DatosExportacion
            {
                TituloReporte = "Reporte de Reservas por Cliente",
                Periodo = $"{ReporteModel.FechaDesde:dd/MM/yyyy} - {ReporteModel.FechaHasta:dd/MM/yyyy}",
                FiltroAdicional = !string.IsNullOrWhiteSpace(ReporteModel.DniCliente)
                    ? $"Filtrado por DNI: {ReporteModel.DniCliente}"
                    : null,
                NombreEmpresa = "Aldea Auriel"
            };

            // Definir columnas
            datosExportacion.Columnas.AddRange(new List<ColumnaExportacion>
            {
                new() { Nombre = "Cliente", TipoDato = "string", Ancho = 40, Alineacion = "left" },
                new() { Nombre = "Cantidad Reservas", TipoDato = "number", Ancho = 20, Alineacion = "center" },
                new() { Nombre = "Última Reserva", TipoDato = "date", Ancho = 25, Alineacion = "center" }
            });

            // Agregar datos
            foreach (var item in ReporteModel.Reservaciones)
            {
                var fila = new Dictionary<string, object>
                {
                    ["cliente"] = $"{item.ClienteNombre} {item.ClienteApellido}",
                    ["cantidad_reservas"] = item.TotalReservaciones,
                    ["última_reserva"] = item.UltimaReservaFecha.ToString("dd/MM/yyyy")
                };
                datosExportacion.Filas.Add(fila);
            }

            // Agregar estadísticas
            datosExportacion.Estadisticas.Add("Total de Clientes", ReporteModel.TotalClientesCompleto);
            datosExportacion.Estadisticas.Add("Total de Reservaciones", ReporteModel.TotalReservacionesCompleto);
            datosExportacion.Estadisticas.Add("Ingresos Totales", ReporteModel.TotalIngresosCompleto);
            datosExportacion.Estadisticas.Add("Total de Personas", ReporteModel.TotalPersonasCompleto);

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

        private async Task CargarDatosCompletos()
        {
            // Buscar reservas por fecha de estadía y opcionalmente por DNI de cliente
            var query = _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Cabana)
                .Where(r => r.Activa &&
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

            // Agrupar por cliente y calcular estadísticas (TODOS los datos para exportación)
            var reservasPorCliente = reservas
                .Where(r => r.Cliente != null)
                .GroupBy(r => r.ClienteId)
                .Select(g => new ReporteReservacionItem
                {
                    ClienteId = g.Key ?? 0,
                    ClienteNombre = g.First().Cliente?.Nombre ?? "Sin cliente",
                    ClienteApellido = g.First().Cliente?.Apellido ?? "",
                    ClienteDni = g.First().Cliente?.Dni ?? "N/A",
                    ClienteEmail = g.First().Cliente?.Email ?? "N/A",
                    ClienteTelefono = g.First().Cliente?.Telefono ?? "N/A",
                    TotalReservaciones = g.Count(),
                    MontoTotalAcumulado = g.Sum(r => r.MontoTotal),
                    TotalPersonasAcumulado = g.Sum(r => r.CantidadPersonas),
                    UltimaReservaFecha = g.Max(r => r.FechaDesde),
                    PrimeraReservaFecha = g.Min(r => r.FechaDesde),
                    UltimaCabana = g.OrderByDescending(r => r.FechaCreacion).First().Cabana?.Nombre ?? "N/A",
                    UltimoEstado = g.OrderByDescending(r => r.FechaCreacion).First().EstadoReserva ?? "N/A"
                })
                .OrderByDescending(r => r.TotalReservaciones)
                .ThenByDescending(r => r.UltimaReservaFecha)
                .ThenBy(r => r.ClienteApellido)
                .ThenBy(r => r.ClienteNombre)
                .ToList();

            // Calcular estadísticas completas
            ReporteModel.TotalClientesCompleto = reservasPorCliente.Count;
            ReporteModel.TotalReservacionesCompleto = reservasPorCliente.Sum(r => r.TotalReservaciones);
            ReporteModel.TotalIngresosCompleto = reservasPorCliente.Sum(r => r.MontoTotalAcumulado);
            ReporteModel.TotalPersonasCompleto = reservasPorCliente.Sum(r => r.TotalPersonasAcumulado);

            // Para exportación, usar todos los datos
            ReporteModel.Reservaciones = reservasPorCliente;
        }
    }
}