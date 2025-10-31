using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.Reservas
{
    [ServiceFilter(typeof(AuthFilter))]
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        public Reserva Reserva { get; set; } = default!;
        public Cliente Cliente { get; set; } = default!;
        public Cabana Cabana { get; set; } = default!;

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
            Cliente = reserva.Cliente;
            Cabana = reserva.Cabana;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
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

            try
            {
                // Cambiar el estado a "Cancelada" en lugar de eliminar
                reserva.EstadoReserva = "Cancelada";
                reserva.Activa = false;

                _context.Reservas.Update(reserva);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Reserva #{reserva.Id} cancelada exitosamente.";
                return RedirectToPage("Listado");
            }
            catch (DbUpdateException ex)
            {
                // Si hay algún error al actualizar
                TempData["ErrorMessage"] = "No se pudo cancelar la reserva. Por favor, inténtelo nuevamente.";
                return RedirectToPage("Details", new { id = id });
            }
        }
    }
}
