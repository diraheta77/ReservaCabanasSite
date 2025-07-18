using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ReservaCabanasSite.Pages.Reservas
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Reserva> Reservas { get; set; } = new List<Reserva>();
        public int MesActual { get; set; }
        public int AñoActual { get; set; }
        public string NombreMes { get; set; }
        public List<DateTime> DiasDelMes { get; set; } = new List<DateTime>();

        public async Task OnGetAsync(int? mes = null, int? año = null)
        {
            // Obtener mes y año actual o especificado
            var fechaActual = DateTime.Now;
            MesActual = mes ?? fechaActual.Month;
            AñoActual = año ?? fechaActual.Year;
            
            // Obtener nombre del mes
            NombreMes = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(MesActual).ToUpper();
            
            // Generar días del mes
            GenerarDiasDelMes();
            
            // Obtener reservas del mes
            var fechaInicio = new DateTime(AñoActual, MesActual, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            
            Reservas = await _context.Reservas
                .Include(r => r.Cabana)
                .Include(r => r.Cliente)
                .Where(r => (r.FechaDesde <= fechaFin && r.FechaHasta >= fechaInicio))
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
            if (reserva.EstadoReserva != null)
            {
                switch (reserva.EstadoReserva.ToLower())
                {
                    case "pendiente":
                        return "#f39c12"; // Amarillo
                    case "confirmada":
                        return "#27ae60"; // Verde
                    case "cancelada":
                        return "#e74c3c"; // Rojo
                }
            }
            return "#6c757d"; // Gris por defecto
        }
    }
}
