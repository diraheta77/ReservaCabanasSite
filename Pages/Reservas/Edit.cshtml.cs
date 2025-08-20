using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.Reservas
{
    [ServiceFilter(typeof(AuthFilter))]
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Reserva Reserva { get; set; } = default!;
        
        public string CabanaNombre { get; set; } = "";
        public string ClienteNombre { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Cabana)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reserva == null)
            {
                return NotFound();
            }

            Reserva = reserva;
            CabanaNombre = reserva.Cabana.Nombre;
            ClienteNombre = $"{reserva.Cliente.Nombre} {reserva.Cliente.Apellido}";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var reservaToUpdate = await _context.Reservas.FindAsync(Reserva.Id);
            if (reservaToUpdate == null)
            {
                return NotFound();
            }

            // Actualizar solo los campos editables
            reservaToUpdate.FechaDesde = Reserva.FechaDesde;
            reservaToUpdate.FechaHasta = Reserva.FechaHasta;
            reservaToUpdate.CantidadPersonas = Reserva.CantidadPersonas;
            reservaToUpdate.EstadoReserva = Reserva.EstadoReserva;
            reservaToUpdate.MetodoPago = Reserva.MetodoPago;
            reservaToUpdate.EstadoPago = Reserva.EstadoPago;
            reservaToUpdate.MontoTotal = Reserva.MontoTotal;
            reservaToUpdate.Observaciones = Reserva.Observaciones;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Reserva actualizada exitosamente.";
                return RedirectToPage("Details", new { id = Reserva.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservaExists(Reserva.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ReservaExists(int id)
        {
            return _context.Reservas.Any(e => e.Id == id);
        }
    }
}
