using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using System.Threading.Tasks;

namespace ReservaCabanasSite.Pages.Reservas
{
    public class ConfirmarReservaModel : PageModel
    {
        private readonly AppDbContext _context;

        public ConfirmarReservaModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Reserva Reserva { get; set; }
        public Cliente Cliente { get; set; }
        public string CabanaNombre { get; set; }

        public async Task<IActionResult> OnGetAsync(int reservaId, int clienteId)
        {
            Reserva = await _context.Reservas.Include(r => r.Cabana).FirstOrDefaultAsync(r => r.Id == reservaId);
            Cliente = await _context.Clientes.FindAsync(clienteId);
            if (Reserva == null || Cliente == null)
                return RedirectToPage("Index");

            CabanaNombre = Reserva.Cabana?.Nombre ?? "";
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int reservaId, int clienteId)
        {
            var reservaDb = await _context.Reservas.FindAsync(reservaId);
            if (reservaDb == null)
                return RedirectToPage("Index");

            // Actualiza los datos de pago y observaciones
            reservaDb.ImporteTotal = Reserva.ImporteTotal;
            reservaDb.Sena = Reserva.Sena;
            reservaDb.Saldo = Reserva.Saldo;
            reservaDb.MedioPago = Reserva.MedioPago;
            reservaDb.Observaciones = Reserva.Observaciones;
            reservaDb.ClienteId = clienteId;

            await _context.SaveChangesAsync();

            return RedirectToPage("Index"); // O página de éxito
        }
    }
}