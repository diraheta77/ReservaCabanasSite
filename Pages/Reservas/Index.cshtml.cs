using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.Reservas
{
    [ServiceFilter(typeof(AuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Reserva> Reservas { get; set; } = new List<Reserva>();
        public List<Cabana> Cabanas { get; set; } = new List<Cabana>();
        public int MesActual { get; set; }
        public int AñoActual { get; set; }
        public string NombreMes { get; set; }
        public List<DateTime> DiasDelMes { get; set; } = new List<DateTime>();

        // Propiedades para filtros
        [BindProperty(SupportsGet = true)]
        public int? CabanaIdFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool MostrarTodasLasCabanas { get; set; } = true;

        // Colores para las cabañas
        private readonly string[] ColoresCabanas = {
            "#00353e", // Rojo
            "#007186", // Azul
            "#2ecc71", // Verde
            "#0097b2", // Naranja
            "#9b59b6", // Púrpura
            "#1abc9c", // Turquesa
            "#e67e22", // Naranja oscuro
            "#34495e", // Gris oscuro
            "#f1c40f", // Amarillo
            "#e91e63"  // Rosa
        };

        public async Task OnGetAsync(int? mes = null, int? año = null, int? cabanaId = null, bool? mostrarTodas = null)
        {
            // Obtener mes y año actual o especificado
            var fechaActual = DateTime.Now;
            MesActual = mes ?? fechaActual.Month;
            AñoActual = año ?? fechaActual.Year;
            
            // Obtener nombre del mes
            NombreMes = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(MesActual).ToUpper();
            
            // Aplicar filtros
            CabanaIdFiltro = cabanaId;
            MostrarTodasLasCabanas = mostrarTodas ?? true;
            
            // Generar días del mes
            GenerarDiasDelMes();
            
            // Obtener reservas del mes con filtros
            var fechaInicio = new DateTime(AñoActual, MesActual, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            
            var query = _context.Reservas
                .Include(r => r.Cabana)
                .Include(r => r.Cliente)
                .Where(r => (r.FechaDesde <= fechaFin && r.FechaHasta >= fechaInicio));

            // Aplicar filtro por cabaña
            if (CabanaIdFiltro.HasValue && CabanaIdFiltro.Value > 0)
            {
                query = query.Where(r => r.CabanaId == CabanaIdFiltro.Value);
            }

            Reservas = await query.ToListAsync();

            // Obtener todas las cabañas activas para la leyenda y filtros
            Cabanas = await _context.Cabanas
                .Where(c => c.Activa)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        private void GenerarDiasDelMes()
        {
            var primerDia = new DateTime(AñoActual, MesActual, 1);
            var ultimoDia = primerDia.AddMonths(1).AddDays(-1);
            
            // Agregar días del mes anterior para completar la primera semana
            var primerDiaSemana = primerDia.DayOfWeek;
            var diasAgregar = (int)primerDiaSemana;
            
            for (int i = diasAgregar - 1; i >= 0; i--)
            {
                DiasDelMes.Add(primerDia.AddDays(-i - 1));
            }
            
            // Agregar todos los días del mes
            for (int i = 0; i < ultimoDia.Day; i++)
            {
                DiasDelMes.Add(primerDia.AddDays(i));
            }
            
            // Completar la última semana si es necesario
            var ultimoDiaSemana = ultimoDia.DayOfWeek;
            var diasFaltantes = 6 - (int)ultimoDiaSemana;
            
            for (int i = 1; i <= diasFaltantes; i++)
            {
                DiasDelMes.Add(ultimoDia.AddDays(i));
            }
        }

        public bool EsDiaDelMesActual(DateTime fecha)
        {
            return fecha.Month == MesActual && fecha.Year == AñoActual;
        }

        public List<Reserva> ObtenerReservasDelDia(DateTime fecha)
        {
            return Reservas.Where(r => fecha >= r.FechaDesde && fecha <= r.FechaHasta).ToList();
        }

        public string ObtenerColorReserva(Reserva reserva)
        {
            if (reserva.Cabana != null)
            {
                // Asignar color basado en el ID de la cabaña
                var indiceColor = (reserva.Cabana.Id - 1) % ColoresCabanas.Length;
                return ColoresCabanas[indiceColor];
            }
            return "#6c757d"; // Gris por defecto
        }

        public string ObtenerColorCabana(Cabana cabana)
        {
            var indiceColor = (cabana.Id - 1) % ColoresCabanas.Length;
            return ColoresCabanas[indiceColor];
        }

        public string ConstruirUrlConFiltros(int? mes = null, int? año = null, int? cabanaId = null, bool? mostrarTodas = null)
        {
            var parametros = new List<string>();
            
            if (mes.HasValue) parametros.Add($"mes={mes}");
            if (año.HasValue) parametros.Add($"año={año}");
            if (cabanaId.HasValue) parametros.Add($"cabanaId={cabanaId}");
            if (mostrarTodas.HasValue) parametros.Add($"mostrarTodas={mostrarTodas}");
            
            return parametros.Any() ? "?" + string.Join("&", parametros) : "";
        }
    }
}
