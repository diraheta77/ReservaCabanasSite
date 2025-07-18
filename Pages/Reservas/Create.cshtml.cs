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
        public CreateModel(AppDbContext context) { _context = context; }

        [BindProperty]
        public ReservaWizardViewModel Wizard { get; set; }

        [BindProperty]
        public int? Paso { get; set; }

        public List<SelectListItem> CabanasSelectList { get; set; }

        public async Task OnGetAsync()
        {
            Paso = 1;
            await CargarCabanas();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var accion = Request.Form["accion"].ToString();
            await CargarCabanas();

            if (Wizard == null)
                Wizard = new ReservaWizardViewModel();

            if (!Paso.HasValue || Paso.Value == 0) Paso = 1;

            if (accion == "siguiente")
            {
                if (Paso < 4)
                    Paso++;
            }
            else if (accion == "anterior")
            {
                if (Paso > 1)
                    Paso--;
            }
            else if (accion == "confirmar")
            {
                // Buscar o crear cliente
                int clienteId = 0;
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Dni == Wizard.ClienteDni);
                if (cliente == null)
                {
                    cliente = new Cliente
                    {
                        Dni = Wizard.ClienteDni,
                        Nombre = Wizard.ClienteNombre,
                        Apellido = Wizard.ClienteApellido,
                        Email = Wizard.ClienteEmail
                    };
                    _context.Clientes.Add(cliente);
                    await _context.SaveChangesAsync();
                }
                clienteId = cliente.Id;

                // Guardar la reserva en la base de datos
                var reserva = new Reserva
                {
                    CabanaId = Wizard.CabanaId.Value,
                    FechaDesde = Wizard.FechaDesde.Value,
                    FechaHasta = Wizard.FechaHasta.Value,
                    Temporada = Wizard.Temporada,
                    CantidadPersonas = Wizard.CantidadPersonas,
                    MedioContacto = Wizard.MedioContacto,
                    Observaciones = Wizard.Observaciones,
                    ImporteTotal = Wizard.ImporteTotal,
                    Sena = Wizard.Sena,
                    Saldo = Wizard.Saldo,
                    MedioPago = Wizard.MedioPago,
                    ClienteId = clienteId
                };
                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();
                TempData["ToastrMessage"] = "¡Reserva creada exitosamente!";
                TempData["ToastrType"] = "success";
                return RedirectToPage("Index");
            }

            return Page();
        }

        private async Task CargarCabanas()
        {
            CabanasSelectList = await _context.Cabanas
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nombre
                }).ToListAsync();
        }
    }
}