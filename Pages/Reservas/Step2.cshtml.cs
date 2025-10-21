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
                System.Diagnostics.Debug.WriteLine("=== No hay datos de wizard, redirigiendo a Step1 ===");
                return RedirectToPage("Step1");
            }
            try
            {
                var pasoAnterior = JsonSerializer.Deserialize<ReservaWizardViewModel>(wizardDataJson);
                // Copiar todos los campos del paso 1
                WizardModel.CabanaId = pasoAnterior.CabanaId;
                WizardModel.FechaDesde = pasoAnterior.FechaDesde;
                WizardModel.FechaHasta = pasoAnterior.FechaHasta;
                WizardModel.Temporada = pasoAnterior.Temporada;
                WizardModel.TemporadaId = pasoAnterior.TemporadaId;
                WizardModel.PrecioPorPersona = pasoAnterior.PrecioPorPersona;
                WizardModel.CantidadPersonas = pasoAnterior.CantidadPersonas;
                WizardModel.MedioContacto = pasoAnterior.MedioContacto;
                WizardModel.MontoTotal = pasoAnterior.MontoTotal;
                WizardModel.PasoActual = 2;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== Error deserializando: {ex.Message} ===");
                return RedirectToPage("Step1");
            }
            
            // Validar solo los campos del paso 2
            var isValid = true;
            // Si ya existe el cliente, solo asociar y avanzar
            var clienteExistente = await _context.Clientes
                .Include(c => c.Vehiculos)
                .FirstOrDefaultAsync(c => c.Dni == WizardModel.Dni && c.Activo);
            if (clienteExistente != null)
            {
                // Actualizar o crear vehículo si se proporcionaron datos
                if (!string.IsNullOrWhiteSpace(WizardModel.Patente) || 
                    !string.IsNullOrWhiteSpace(WizardModel.Marca) || 
                    !string.IsNullOrWhiteSpace(WizardModel.Modelo) || 
                    !string.IsNullOrWhiteSpace(WizardModel.Color))
                {
                    var vehiculoExistente = clienteExistente.Vehiculos.FirstOrDefault();
                    if (vehiculoExistente != null)
                    {
                        // Actualizar vehículo existente
                        vehiculoExistente.Patente = WizardModel.Patente ?? vehiculoExistente.Patente;
                        vehiculoExistente.Marca = WizardModel.Marca ?? vehiculoExistente.Marca;
                        vehiculoExistente.Modelo = WizardModel.Modelo ?? vehiculoExistente.Modelo;
                        vehiculoExistente.Color = WizardModel.Color ?? vehiculoExistente.Color;
                    }
                    else
                    {
                        // Crear nuevo vehículo
                        var nuevoVehiculo = new Vehiculo
                        {
                            Patente = WizardModel.Patente ?? "",
                            Marca = WizardModel.Marca ?? "",
                            Modelo = WizardModel.Modelo ?? "",
                            Color = WizardModel.Color ?? "",
                            ClienteId = clienteExistente.Id
                        };
                        _context.Vehiculos.Add(nuevoVehiculo);
                    }
                    await _context.SaveChangesAsync();
                }
                
                WizardModel.ClienteId = clienteExistente.Id;
                WizardModel.Cliente = clienteExistente;
                // Guardar datos para el siguiente paso
                var newWizardData = JsonSerializer.Serialize(WizardModel);
                TempData["WizardData"] = newWizardData;
                WizardDataJson = newWizardData;
                return RedirectToPage("Step3");
            }
            // Si es cliente nuevo, validar campos del paso 2
            if (string.IsNullOrWhiteSpace(WizardModel.Dni))
            {
                ModelState.AddModelError("WizardModel.Dni", "El DNI es requerido");
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(WizardModel.Nombre))
            {
                ModelState.AddModelError("WizardModel.Nombre", "El nombre es requerido");
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(WizardModel.Apellido))
            {
                ModelState.AddModelError("WizardModel.Apellido", "El apellido es requerido");
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(WizardModel.Telefono))
            {
                ModelState.AddModelError("WizardModel.Telefono", "El teléfono es requerido");
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(WizardModel.Email))
            {
                ModelState.AddModelError("WizardModel.Email", "El email es requerido");
                isValid = false;
            }
            else
            {
                // Validación más robusta de email
                try
                {
                    var mailAddress = new System.Net.Mail.MailAddress(WizardModel.Email);
                    if (mailAddress.Address != WizardModel.Email)
                    {
                        ModelState.AddModelError("WizardModel.Email", "El formato del email no es válido");
                        isValid = false;
                    }
                }
                catch
                {
                    ModelState.AddModelError("WizardModel.Email", "El formato del email no es válido");
                    isValid = false;
                }
            }
            // Validación personalizada para Dirección
            if (string.IsNullOrWhiteSpace(WizardModel.Direccion))
            {
                ModelState.AddModelError("WizardModel.Direccion", "La dirección es requerida");
                isValid = false;
            }
            else
            {
                var direccion = WizardModel.Direccion.Trim().ToUpper();
                // Permitir S/N o direcciones con número o direcciones con palabras clave válidas (Av., Avenida, Calle, etc.)
                bool tieneNumero = System.Text.RegularExpressions.Regex.IsMatch(WizardModel.Direccion, @"\d");
                bool esSN = direccion == "S/N" || direccion.Contains("S/N");
                bool tienePalabrasClave = System.Text.RegularExpressions.Regex.IsMatch(direccion, @"\b(AVENIDA|AVDA|AV\.?|CALLE|PASAJE|RUTA|KM|KILOMETRO)\b");

                if (!tieneNumero && !esSN && !tienePalabrasClave)
                {
                    ModelState.AddModelError("WizardModel.Direccion", "La dirección debe contener calle y número, o indicar S/N");
                    isValid = false;
                }
            }

            if (!isValid)
            {
                WizardDataJson = JsonSerializer.Serialize(WizardModel);
                return Page();
            }
            // Validación DNI solo números
            if (string.IsNullOrWhiteSpace(WizardModel.Dni) || !System.Text.RegularExpressions.Regex.IsMatch(WizardModel.Dni, @"^\d+$"))
            {
                ModelState.AddModelError("WizardModel.Dni", "El DNI debe contener solo números, sin letras ni caracteres especiales.");
                WizardDataJson = JsonSerializer.Serialize(WizardModel);
                return Page();
            }
            // Validación DNI único
            if (await _context.Clientes.AnyAsync(c => c.Dni == WizardModel.Dni))
            {
                ModelState.AddModelError("WizardModel.Dni", "Ya existe un cliente con ese DNI.");
                WizardDataJson = JsonSerializer.Serialize(WizardModel);
                return Page();
            }
            
            // Validación de edad (mayor de 18 años)
            if (WizardModel.FechaNacimiento.HasValue)
            {
                var edad = DateTime.Today.Year - WizardModel.FechaNacimiento.Value.Year;
                if (WizardModel.FechaNacimiento.Value.Date > DateTime.Today.AddYears(-edad))
                {
                    edad--;
                }
                
                if (edad < 18)
                {
                    ModelState.AddModelError("WizardModel.FechaNacimiento", "Debes ser mayor de 18 años para realizar una reserva.");
                    WizardDataJson = JsonSerializer.Serialize(WizardModel);
                    return Page();
                }
            }
            else
            {
                ModelState.AddModelError("WizardModel.FechaNacimiento", "La fecha de nacimiento es requerida.");
                WizardDataJson = JsonSerializer.Serialize(WizardModel);
                return Page();
            }
            if (!isValid)
            {
                WizardDataJson = JsonSerializer.Serialize(WizardModel);
                return Page();
            }
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
            
            // Crear vehículo si se proporcionaron datos
            if (!string.IsNullOrWhiteSpace(WizardModel.Patente) || 
                !string.IsNullOrWhiteSpace(WizardModel.Marca) || 
                !string.IsNullOrWhiteSpace(WizardModel.Modelo) || 
                !string.IsNullOrWhiteSpace(WizardModel.Color))
            {
                var nuevoVehiculo = new Vehiculo
                {
                    Patente = WizardModel.Patente ?? "",
                    Marca = WizardModel.Marca ?? "",
                    Modelo = WizardModel.Modelo ?? "",
                    Color = WizardModel.Color ?? "",
                    ClienteId = nuevoCliente.Id
                };
                _context.Vehiculos.Add(nuevoVehiculo);
                await _context.SaveChangesAsync();
            }
            
            WizardModel.ClienteId = nuevoCliente.Id;
            WizardModel.Cliente = nuevoCliente;
            // Guardar datos para el siguiente paso
            var newWizardData2 = JsonSerializer.Serialize(WizardModel);
            TempData["WizardData"] = newWizardData2;
            WizardDataJson = newWizardData2;
            return RedirectToPage("Step3");
        }

        public async Task<IActionResult> OnPostBuscarClienteAsync(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
            {
                return new JsonResult(new { success = false, message = "DNI requerido" });
            }

            var cliente = await _context.Clientes
                .Include(c => c.Vehiculos)
                .FirstOrDefaultAsync(c => c.Dni == dni && c.Activo);

            if (cliente == null)
            {
                return new JsonResult(new { success = false, message = "Cliente no encontrado" });
            }

            // Obtener el primer vehículo del cliente (si existe)
            var vehiculo = cliente.Vehiculos.FirstOrDefault();

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
                    fechaNacimiento = cliente.FechaNacimiento?.ToString("yyyy-MM-dd"),
                    vehiculo = vehiculo != null ? new {
                        patente = vehiculo.Patente,
                        marca = vehiculo.Marca,
                        modelo = vehiculo.Modelo,
                        color = vehiculo.Color
                    } : null
                }
            });
        }

        public async Task<IActionResult> OnPostBuscarClientesAvanzadoAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new JsonResult(new { success = false, clientes = new List<object>() });
            }
            var clientes = await _context.Clientes
                .Include(c => c.Vehiculos)
                .Where(c => c.Activo && (
                    c.Dni.Contains(query) ||
                    c.Nombre.Contains(query) ||
                    c.Apellido.Contains(query) ||
                    c.Id.ToString().Contains(query)
                ))
                .OrderBy(c => c.Nombre)
                .Take(10)
                .ToListAsync();
            return new JsonResult(new
            {
                success = true,
                clientes = clientes.Select(c => {
                    var vehiculo = c.Vehiculos.FirstOrDefault();
                    return new {
                        id = c.Id,
                        dni = c.Dni,
                        nombre = c.Nombre,
                        apellido = c.Apellido,
                        telefono = c.Telefono,
                        email = c.Email,
                        direccion = c.Direccion,
                        ciudad = c.Ciudad,
                        provincia = c.Provincia,
                        pais = c.Pais,
                        nacionalidad = c.Nacionalidad,
                        fechaNacimiento = c.FechaNacimiento?.ToString("yyyy-MM-dd"),
                        patente = vehiculo?.Patente,
                        marca = vehiculo?.Marca,
                        modelo = vehiculo?.Modelo,
                        color = vehiculo?.Color
                    };
                }).ToList()
            });
        }
    }
}
