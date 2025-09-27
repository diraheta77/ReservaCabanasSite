using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.Reportes.Cliente
{
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class ProvinciaModel : PageModel
    {
        private readonly AppDbContext _context;

        public ProvinciaModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ReporteReservacionesViewModel ReporteModel { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            // Establecer fechas por defecto si no se proporcionan (julio 2025)
            ReporteModel.FechaDesde = fechaDesde ?? new DateTime(2025, 7, 1);
            ReporteModel.FechaHasta = fechaHasta ?? new DateTime(2025, 7, 31);

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
            // Buscar reservas por fecha de estadía y agrupar por provincia
            var reservas = await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Cabana)
                .Where(r => r.Activa &&
                           r.FechaDesde.Date >= ReporteModel.FechaDesde.Date &&
                           r.FechaDesde.Date <= ReporteModel.FechaHasta.Date)
                .ToListAsync();

            // Agrupar por provincia y calcular estadísticas
            var reservasPorProvincia = reservas
                .Where(r => r.Cliente != null && !string.IsNullOrEmpty(r.Cliente.Provincia))
                .GroupBy(r => r.Cliente.Provincia)
                .Select(g => new ReporteReservacionItem
                {
                    ClienteId = 0, // No aplica para agrupación por provincia
                    ClienteNombre = g.Key, // Usamos el nombre del cliente para mostrar la provincia
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
    }
}