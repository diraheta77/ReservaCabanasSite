using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ReservaCabanasSite.Pages.Reservas
{
    public class SeleccionarClienteModel : PageModel
    {
        private readonly AppDbContext _context;

        public SeleccionarClienteModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string DniBusqueda { get; set; }
        public Cliente ClienteEncontrado { get; set; }
        public bool BusquedaRealizada { get; set; }

        [BindProperty(SupportsGet = true)]
        public int ReservaId { get; set; }

        public async Task OnGetAsync()
        {
            // Nada por defecto
        }

        public async Task<IActionResult> OnPostAsync(string action, string DniBusqueda, int? ClienteId, string Nombre, string Apellido, string Email)
        {
            if (action == "buscar")
            {
                BusquedaRealizada = true;
                ClienteEncontrado = _context.Clientes.FirstOrDefault(c => c.Dni == DniBusqueda);
                this.DniBusqueda = DniBusqueda;
                return Page();
            }
            if (action == "seleccionar" && ClienteId.HasValue)
            {
                // Redirige a ConfirmarReserva con ambos IDs
                return RedirectToPage("ConfirmarReserva", new { reservaId = ReservaId, clienteId = ClienteId.Value });
            }
            if (action == "crear")
            {
                var nuevo = new Cliente
                {
                    Dni = DniBusqueda,
                    Nombre = Nombre,
                    Apellido = Apellido,
                    Email = Email
                };
                _context.Clientes.Add(nuevo);
                await _context.SaveChangesAsync();
                // Redirige a ConfirmarReserva con ambos IDs
                return RedirectToPage("ConfirmarReserva", new { reservaId = ReservaId, clienteId = nuevo.Id });
            }
            return Page();
        }
    }
}