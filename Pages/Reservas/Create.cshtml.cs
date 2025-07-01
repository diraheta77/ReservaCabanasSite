using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReservaCabanasSite.Pages.Reservas
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Reserva Reserva { get; set; }

        public List<SelectListItem> CabanasSelectList { get; set; }

        public async Task OnGetAsync()
        {
            CabanasSelectList = await _context.Cabanas
                //.Where(c => c.Activa)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre
                }).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            _context.Reservas.Add(Reserva);
            await _context.SaveChangesAsync();

            // Redirige a SeleccionarCliente pasando el id de la reserva recién creada
            return RedirectToPage("SeleccionarCliente", new { reservaId = Reserva.Id });
        }
    }
}