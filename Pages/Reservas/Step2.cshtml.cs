using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ReservaCabanasSite.Pages.Reservas
{
    public class Step2Model : PageModel
    {
        private readonly AppDbContext _context;
        public Step2Model(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ReservaWizardViewModel WizardModel { get; set; } = new ReservaWizardViewModel();

        // 1. Agregar propiedad para el JSON oculto
        [BindProperty]
        public string WizardDataJson { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            System.Diagnostics.Debug.WriteLine("=== OnGetAsync iniciado ===");
            string wizardDataJson = null;
            if (TempData["WizardData"] != null)
            {
                wizardDataJson = TempData["WizardData"].ToString();
            }
            else if (!string.IsNullOrEmpty(WizardDataJson))
            {
                wizardDataJson = WizardDataJson;
            }
            if (wizardDataJson == null)
            {
                System.Diagnostics.Debug.WriteLine("=== No hay datos de wizard, redirigiendo a Step1 ===");
                TempData["ErrorMessage"] = "No se encontraron datos del paso anterior. Por favor, complete el Paso 1 primero.";
                return RedirectToPage("Step1");
            }
            try
            {
                WizardModel = JsonSerializer.Deserialize<ReservaWizardViewModel>(wizardDataJson);
                WizardModel.PasoActual = 2;
                TempData["DebugMessage"] = $"Datos cargados: CabanaId={WizardModel.CabanaId}, FechaDesde={WizardModel.FechaDesde:yyyy-MM-dd}, FechaHasta={WizardModel.FechaHasta:yyyy-MM-dd}";
                WizardDataJson = wizardDataJson;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar los datos: {ex.Message}";
                return RedirectToPage("Step1");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            System.Diagnostics.Debug.WriteLine("=== OnPostAsync iniciado ===");
            string wizardDataJson = null;
            if (TempData["WizardData"] != null)
            {
                wizardDataJson = TempData["WizardData"].ToString();
            }
            else if (!string.IsNullOrEmpty(WizardDataJson))
            {
                wizardDataJson = WizardDataJson;
            }
            if (wizardDataJson == null)
            {
                System.Diagnostics.Debug.WriteLine("=== No hay datos de wizard, redirigiendo a Step1 ===");
                return RedirectToPage("Step1");
            }
            try
            {
                var pasoAnterior = JsonSerializer.Deserialize<ReservaWizardViewModel>(wizardDataJson);
                WizardModel.CabanaId = pasoAnterior.CabanaId;
                WizardModel.FechaDesde = pasoAnterior.FechaDesde;
                WizardModel.FechaHasta = pasoAnterior.FechaHasta;
                WizardModel.Temporada = pasoAnterior.Temporada;
                WizardModel.CantidadPersonas = pasoAnterior.CantidadPersonas;
                WizardModel.MedioContacto = pasoAnterior.MedioContacto;
                WizardModel.PasoActual = 2;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== Error deserializando: {ex.Message} ===");
                return RedirectToPage("Step1");
            }
            
            // Validar solo los campos del paso 2
            var isValid = true;
            
            if (string.IsNullOrWhiteSpace(WizardModel.Dni))
            {
                ModelState.AddModelError("WizardModel.Dni", "El DNI es requerido");
                isValid = false;
                System.Diagnostics.Debug.WriteLine("Error: DNI vacío");
            }
            
            if (string.IsNullOrWhiteSpace(WizardModel.Nombre))
            {
                ModelState.AddModelError("WizardModel.Nombre", "El nombre es requerido");
                isValid = false;
                System.Diagnostics.Debug.WriteLine("Error: Nombre vacío");
            }
            
            if (string.IsNullOrWhiteSpace(WizardModel.Apellido))
            {
                ModelState.AddModelError("WizardModel.Apellido", "El apellido es requerido");
                isValid = false;
                System.Diagnostics.Debug.WriteLine("Error: Apellido vacío");
            }
            
            if (string.IsNullOrWhiteSpace(WizardModel.Telefono))
            {
                ModelState.AddModelError("WizardModel.Telefono", "El teléfono es requerido");
                isValid = false;
                System.Diagnostics.Debug.WriteLine("Error: Teléfono vacío");
            }
            
            if (string.IsNullOrWhiteSpace(WizardModel.Email))
            {
                ModelState.AddModelError("WizardModel.Email", "El email es requerido");
                isValid = false;
                System.Diagnostics.Debug.WriteLine("Error: Email vacío");
            }
            else if (!WizardModel.Email.Contains("@"))
            {
                ModelState.AddModelError("WizardModel.Email", "El formato del email no es válido");
                isValid = false;
                System.Diagnostics.Debug.WriteLine("Error: Email inválido");
            }

            if (!isValid)
            {
                System.Diagnostics.Debug.WriteLine("=== Validación falló, retornando Page ===");
                // Actualizar el JSON oculto para el siguiente request
                WizardDataJson = JsonSerializer.Serialize(WizardModel);
                return Page();
            }

            System.Diagnostics.Debug.WriteLine("=== Validación exitosa ===");
            System.Diagnostics.Debug.WriteLine($"DNI: {WizardModel.Dni}");
            System.Diagnostics.Debug.WriteLine($"Nombre: {WizardModel.Nombre}");
            System.Diagnostics.Debug.WriteLine($"Email: {WizardModel.Email}");

            // Verificar si el cliente ya existe
            var clienteExistente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Dni == WizardModel.Dni && c.Activo);

            if (clienteExistente != null)
            {
                // Usar cliente existente
                WizardModel.ClienteId = clienteExistente.Id;
                WizardModel.Cliente = clienteExistente;
                System.Diagnostics.Debug.WriteLine("=== Cliente existente encontrado y asignado ===");
            }
            else
            {
                // Crear y guardar cliente nuevo
                var nuevoCliente = new Cliente
                {
                    Dni = WizardModel.Dni,
                    Nombre = WizardModel.Nombre,
                    Apellido = WizardModel.Apellido,
                    FechaNacimiento = WizardModel.FechaNacimiento,
                    Nacionalidad = WizardModel.Nacionalidad,
                    Direccion = WizardModel.Direccion,
                    Ciudad = WizardModel.Ciudad,
                    Provincia = WizardModel.Provincia,
                    Pais = WizardModel.Pais,
                    Telefono = WizardModel.Telefono,
                    Email = WizardModel.Email,
                    Observaciones = WizardModel.Observaciones,
                    Activo = true
                };
                _context.Clientes.Add(nuevoCliente);
                await _context.SaveChangesAsync();
                WizardModel.ClienteId = nuevoCliente.Id;
                WizardModel.Cliente = nuevoCliente;
                System.Diagnostics.Debug.WriteLine("=== Cliente nuevo creado y guardado ===");
            }

            // Guardar datos para el siguiente paso
            var newWizardData = JsonSerializer.Serialize(WizardModel);
            TempData["WizardData"] = newWizardData;
            WizardDataJson = newWizardData;
            
            System.Diagnostics.Debug.WriteLine($"=== Datos guardados en TempData: {newWizardData} ===");
            System.Diagnostics.Debug.WriteLine("=== Redirigiendo a Step3 ===");
            
            return RedirectToPage("Step3");
        }

        public async Task<IActionResult> OnPostBuscarClienteAsync(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
            {
                return new JsonResult(new { success = false, message = "DNI requerido" });
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Dni == dni && c.Activo);

            if (cliente == null)
            {
                return new JsonResult(new { success = false, message = "Cliente no encontrado" });
            }

            return new JsonResult(new { 
                success = true, 
                cliente = new {
                    dni = cliente.Dni,
                    nombre = cliente.Nombre,
                    apellido = cliente.Apellido,
                    telefono = cliente.Telefono,
                    email = cliente.Email,
                    direccion = cliente.Direccion,
                    ciudad = cliente.Ciudad,
                    provincia = cliente.Provincia,
                    pais = cliente.Pais,
                    nacionalidad = cliente.Nacionalidad,
                    fechaNacimiento = cliente.FechaNacimiento?.ToString("yyyy-MM-dd")
                }
            });
        }
    }
}
