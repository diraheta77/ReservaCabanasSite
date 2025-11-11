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

        public List<Models.Cabana> Cabanas { get; set; } = new();
        public List<Temporada> Temporadas { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? ano, int? cabanaId, int? temporadaId, List<int>? meses)
        {
            // Establecer año por defecto (año actual)
            ReporteModel.Ano = ano ?? DateTime.Now.Year;
            ReporteModel.CabanaId = cabanaId;
            ReporteModel.TemporadaId = temporadaId;

            // Si no se proporcionan meses, seleccionar el mes actual
            if (meses == null || !meses.Any())
            {
                ReporteModel.MesesSeleccionados = new List<int> { DateTime.Now.Month };
            }
            else
            {
                ReporteModel.MesesSeleccionados = meses;
            }

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

            // Validar que se haya seleccionado al menos un mes
            if (ReporteModel.MesesSeleccionados == null || !ReporteModel.MesesSeleccionados.Any())
            {
                ModelState.AddModelError("ReporteModel.MesesSeleccionados", "Debe seleccionar al menos un mes.");
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
            // Si no hay meses seleccionados, no cargar datos
            if (ReporteModel.MesesSeleccionados == null || !ReporteModel.MesesSeleccionados.Any())
            {
                ReporteModel.ReservasPorMes = new List<ReporteMesItem>();
                ReporteModel.TotalReservas = 0;
                ReporteModel.TotalIngresos = 0;
                return;
            }

            // Buscar reservas por año y meses seleccionados - incluir Confirmadas y Finalizadas, excluir Canceladas
            var query = _context.Reservas
                .Where(r => r.EstadoReserva != "Cancelada" &&
                           r.FechaDesde.Year == ReporteModel.Ano &&
                           ReporteModel.MesesSeleccionados.Contains(r.FechaDesde.Month));

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

            // Agrupar por mes y calcular estadísticas
            var reservasPorMes = reservas
                .GroupBy(r => new { r.FechaDesde.Year, r.FechaDesde.Month })
                .Select(g => new ReporteMesItem
                {
                    Ano = g.Key.Year,
                    Mes = g.Key.Month,
                    NombreMes = GetNombreMes(g.Key.Month) + " " + g.Key.Year,
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

        public async Task<IActionResult> OnGetExportarExcelAsync(int? ano, List<int>? meses)
        {
            // Configurar los filtros para exportación
            ReporteModel.Ano = ano ?? DateTime.Now.Year;

            if (meses == null || !meses.Any())
            {
                ReporteModel.MesesSeleccionados = new List<int> { DateTime.Now.Month };
            }
            else
            {
                ReporteModel.MesesSeleccionados = meses;
            }

            // Obtener todos los datos
            await CargarReporte();

            // Preparar datos para exportación
            var mesesTexto = string.Join(", ", ReporteModel.MesesSeleccionados.Select(m => GetNombreMes(m)));
            var datosExportacion = new DatosExportacion
            {
                TituloReporte = "Reporte de Reservas por Meses",
                Periodo = $"{mesesTexto} {ReporteModel.Ano}",
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
                    ["cantidad_de_reservas"] = item.CantidadReservas,
                    ["total_ingresos"] = item.TotalIngresos,
                    ["%_reservas"] = $"{item.PorcentajeReservas}%",
                    ["%_ingresos"] = $"{item.PorcentajeIngresos}%"
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

        public async Task<IActionResult> OnGetExportarPdfAsync(int? ano, List<int>? meses)
        {
            // Configurar los filtros para exportación
            ReporteModel.Ano = ano ?? DateTime.Now.Year;

            if (meses == null || !meses.Any())
            {
                ReporteModel.MesesSeleccionados = new List<int> { DateTime.Now.Month };
            }
            else
            {
                ReporteModel.MesesSeleccionados = meses;
            }

            // Obtener todos los datos
            await CargarReporte();

            // Preparar datos para exportación
            var mesesTexto = string.Join(", ", ReporteModel.MesesSeleccionados.Select(m => GetNombreMes(m)));
            var datosExportacion = new DatosExportacion
            {
                TituloReporte = "Reporte de Reservas por Meses",
                Periodo = $"{mesesTexto} {ReporteModel.Ano}",
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
                    ["cantidad_de_reservas"] = item.CantidadReservas,
                    ["total_ingresos"] = item.TotalIngresos,
                    ["%_reservas"] = $"{item.PorcentajeReservas}%",
                    ["%_ingresos"] = $"{item.PorcentajeIngresos}%"
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