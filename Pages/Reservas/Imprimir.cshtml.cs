using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;

namespace ReservaCabanasSite.Pages.Reservas
{
    public class ImprimirModel : PageModel
    {
        private readonly AppDbContext _context;
        public ImprimirModel(AppDbContext context)
        {
            _context = context;
        }

        public Reserva Reserva { get; set; }
        public Cliente Cliente { get; set; }
        public Cabana Cabana { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Reserva = await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Cabana)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (Reserva == null)
            {
                return RedirectToPage("Index");
            }
            Cliente = Reserva.Cliente;
            Cabana = Reserva.Cabana;
            return Page();
        }
    }
} 