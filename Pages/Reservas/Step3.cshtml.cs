using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.Reservas
{
    [ServiceFilter(typeof(AuthFilter))]
    public class Step3Model : PageModel
    {
        private readonly AppDbContext _context;
        public Step3Model(AppDbContext context)
        {
            _context = context;
        }

        public ReservaWizardViewModel WizardModel { get; set; } = new ReservaWizardViewModel();
        [BindProperty]
        public string WizardDataJson { get; set; }
        [BindProperty]
        public string MetodoPagoInput { get; set; }
        [BindProperty]
        public string EstadoPagoInput { get; set; }
        [BindProperty]
        public string? ObservacionesInput { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            string wizardDataJson = null;
            if (TempData.Peek("WizardData") != null)
            {
                wizardDataJson = TempData.Peek("WizardData").ToString();
            }
            else if (!string.IsNullOrEmpty(WizardDataJson))
            {
                wizardDataJson = WizardDataJson;
            }
            if (wizardDataJson == null)
            {
                return RedirectToPage("Step1");
            }
            try
            {
                WizardModel = JsonSerializer.Deserialize<ReservaWizardViewModel>(wizardDataJson);
                WizardModel.PasoActual = 3;
                WizardDataJson = wizardDataJson;
                // Cargar datos de la cabaña para mostrar información
                if (WizardModel.CabanaId.HasValue)
                {
                    var cabana = await _context.Cabanas.FindAsync(WizardModel.CabanaId.Value);
                    if (cabana != null)
                    {
                        ViewData["Cabana"] = cabana;
                    }
                }
            }
            catch
            {
                return RedirectToPage("Step1");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            string wizardDataJson = null;
            if (TempData.Peek("WizardData") != null)
            {
                wizardDataJson = TempData.Peek("WizardData").ToString();
            }
            else if (!string.IsNullOrEmpty(WizardDataJson))
            {
                wizardDataJson = WizardDataJson;
            }
            if (wizardDataJson == null)
            {
                return RedirectToPage("Step1");
            }
            // Reconstruir el modelo completo desde el JSON oculto
            ReservaWizardViewModel wizardFromJson;
            try
            {
                wizardFromJson = JsonSerializer.Deserialize<ReservaWizardViewModel>(wizardDataJson);
            }
            catch
            {
                return RedirectToPage("Step1");
            }
            // Copiar los campos de pago y observaciones del POST al modelo reconstruido
            wizardFromJson.MetodoPago = MetodoPagoInput;
            wizardFromJson.EstadoPago = EstadoPagoInput;
            wizardFromJson.Observaciones = ObservacionesInput;
            
            // NO recalcular el precio - usar el que ya viene calculado desde Step1
            // El precio ya fue calculado correctamente en Step1 y no debe cambiar
            
            // Asignar el modelo reconstruido al property para la vista
            WizardModel = wizardFromJson;
            // Validar manualmente el modelo completo
            if (string.IsNullOrWhiteSpace(WizardModel.Dni))
                ModelState.AddModelError("WizardModel.Dni", "El DNI es requerido");
            if (string.IsNullOrWhiteSpace(WizardModel.Nombre))
                ModelState.AddModelError("WizardModel.Nombre", "El nombre es requerido");
            if (string.IsNullOrWhiteSpace(WizardModel.Apellido))
                ModelState.AddModelError("WizardModel.Apellido", "El apellido es requerido");
            if (string.IsNullOrWhiteSpace(WizardModel.Telefono))
                ModelState.AddModelError("WizardModel.Telefono", "El teléfono es requerido");
            if (string.IsNullOrWhiteSpace(WizardModel.Email))
                ModelState.AddModelError("WizardModel.Email", "El email es requerido");
            if (string.IsNullOrWhiteSpace(WizardModel.Temporada))
                ModelState.AddModelError("WizardModel.Temporada", "La temporada es requerida");
            if (string.IsNullOrWhiteSpace(WizardModel.MedioContacto))
                ModelState.AddModelError("WizardModel.MedioContacto", "El medio de contacto es requerido");
            if (string.IsNullOrWhiteSpace(WizardModel.MetodoPago))
                ModelState.AddModelError("WizardModel.MetodoPago", "El método de pago es requerido");
            if (string.IsNullOrWhiteSpace(WizardModel.EstadoPago))
                ModelState.AddModelError("WizardModel.EstadoPago", "El estado del pago es requerido");
            if (WizardModel.MontoTotal <= 0)
                ModelState.AddModelError("WizardModel.MontoTotal", "El monto total debe ser mayor a cero");
            if (!ModelState.IsValid)
            {
                WizardDataJson = JsonSerializer.Serialize(WizardModel);
                return Page();
            }

            // Crear o actualizar el cliente
            Cliente cliente;
            bool clienteNuevo = false;
            if (WizardModel.ClienteId.HasValue)
            {
                // Usar cliente existente
                cliente = await _context.Clientes.FindAsync(WizardModel.ClienteId.Value);
                if (cliente == null)
                {
                    ModelState.AddModelError("", "Cliente no encontrado");
                    return Page();
                }
            }
            else
            {
                // Verificar si ya existe un cliente con ese DNI
                var clienteExistente = await _context.Clientes.FirstOrDefaultAsync(c => c.Dni == WizardModel.Dni && c.Activo);
                if (clienteExistente != null)
                {
                    ModelState.AddModelError("WizardModel.Dni", "Ya existe un cliente con ese DNI.");
                    WizardDataJson = JsonSerializer.Serialize(WizardModel);
                    return Page();
                }

                // Crear nuevo cliente
                cliente = new Cliente
                {
                    Dni = WizardModel.Dni,
                    Nombre = WizardModel.Nombre,
                    Apellido = WizardModel.Apellido,
                    FechaNacimiento = WizardModel.FechaNacimiento,
                    Nacionalidad = WizardModel.Nacionalidad,
                    Pais = WizardModel.Pais,
                    Provincia = WizardModel.Provincia,
                    Ciudad = WizardModel.Ciudad,
                    Direccion = WizardModel.Direccion,
                    Telefono = WizardModel.Telefono,
                    Email = WizardModel.Email,
                    Activo = true
                };
                _context.Clientes.Add(cliente);
                clienteNuevo = true;
            }
            // Si es cliente nuevo, guardar primero para obtener el ID
            if (clienteNuevo)
            {
                await _context.SaveChangesAsync();
            }

            // Re-validar que la cabaña sigue disponible en las fechas seleccionadas
            var conflicto = await _context.Reservas.AnyAsync(r =>
                r.CabanaId == WizardModel.CabanaId.Value &&
                r.Activa &&
                r.FechaDesde < WizardModel.FechaHasta.Value &&
                r.FechaHasta > WizardModel.FechaDesde.Value);

            if (conflicto)
            {
                ModelState.AddModelError("", "La cabaña ya no está disponible en las fechas seleccionadas. Por favor, selecciona otras fechas.");
                WizardDataJson = JsonSerializer.Serialize(WizardModel);
                return Page();
            }

            // Crear la reserva
            var reserva = new Reserva
            {
                CabanaId = WizardModel.CabanaId.Value,
                ClienteId = cliente.Id,
                FechaDesde = WizardModel.FechaDesde.Value,
                FechaHasta = WizardModel.FechaHasta.Value,
                CantidadPersonas = WizardModel.CantidadPersonas,
                Temporada = WizardModel.Temporada,
                TemporadaId = WizardModel.TemporadaId,
                PrecioPorPersona = WizardModel.PrecioPorPersona,
                MedioContacto = WizardModel.MedioContacto,
                Observaciones = WizardModel.Observaciones,
                MontoTotal = WizardModel.MontoTotal, // Usar el monto calculado en Step1
                MetodoPago = WizardModel.MetodoPago,
                EstadoPago = WizardModel.EstadoPago,
                EstadoReserva = "Confirmada",
                FechaCreacion = DateTime.Now,
                Activa = true
            };
            try
            {
                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar la reserva: {ex.Message}");
                WizardDataJson = JsonSerializer.Serialize(WizardModel);
                return Page();
            }
            // Guardar el ID de la reserva creada para mostrar en la confirmación
            TempData["ReservaId"] = reserva.Id;
            TempData["ClienteId"] = cliente.Id;
            return RedirectToPage("Confirmacion");
        }
    }
}
