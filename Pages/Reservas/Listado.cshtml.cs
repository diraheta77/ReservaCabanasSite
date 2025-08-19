using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.Reservas
{
    [ServiceFilter(typeof(AuthFilter))]
    public class ListadoModel : PageModel
    {
        private readonly AppDbContext _context;
        public ListadoModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Reserva> Reservas { get; set; } = new List<Reserva>();
        public List<Cabana> Cabanas { get; set; } = new List<Cabana>();

        // Propiedades para filtros
        [BindProperty(SupportsGet = true)]
        public int? CabanaIdFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ClienteBusqueda { get; set; }

        [BindProperty(SupportsGet = true)]
        public string EstadoFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public string FechaDesdeFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public string FechaHastaFiltro { get; set; }

        public async Task OnGetAsync()
        {
            // Obtener todas las cabañas para los filtros
            Cabanas = await _context.Cabanas
                .Where(c => c.Activa)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            // Construir query base
            var query = _context.Reservas
                .Include(r => r.Cabana)
                .Include(r => r.Cliente)
                .AsQueryable();

            // Aplicar filtros
            if (CabanaIdFiltro.HasValue && CabanaIdFiltro.Value > 0)
            {
                query = query.Where(r => r.CabanaId == CabanaIdFiltro.Value);
            }

            if (!string.IsNullOrWhiteSpace(ClienteBusqueda))
            {
                var busqueda = ClienteBusqueda.ToLower();
                query = query.Where(r => 
                    r.Cliente.Nombre.ToLower().Contains(busqueda) ||
                    r.Cliente.Apellido.ToLower().Contains(busqueda) ||
                    r.Cliente.Dni.Contains(busqueda));
            }

            if (!string.IsNullOrWhiteSpace(EstadoFiltro))
            {
                query = query.Where(r => r.EstadoReserva == EstadoFiltro);
            }

            if (!string.IsNullOrWhiteSpace(FechaDesdeFiltro))
            {
                if (DateTime.TryParse(FechaDesdeFiltro, out var fechaDesde))
                {
                    query = query.Where(r => r.FechaDesde >= fechaDesde);
                }
            }

            if (!string.IsNullOrWhiteSpace(FechaHastaFiltro))
            {
                if (DateTime.TryParse(FechaHastaFiltro, out var fechaHasta))
                {
                    query = query.Where(r => r.FechaHasta <= fechaHasta);
                }
            }

            // Ordenar por fecha de creación (más recientes primero)
            Reservas = await query
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();
        }
    }
}
