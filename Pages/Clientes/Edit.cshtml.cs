using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;

namespace ReservaCabanasSite.Pages.Clientes
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;
        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Cliente Cliente { get; set; }
        [BindProperty(SupportsGet = true)]
        public Vehiculo Vehiculo { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.Activo);
            if (Cliente == null)
            {
                return NotFound();
            }
            // Cargar vehículo existente
            Vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(v => v.ClienteId == Cliente.Id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Vehiculo = await _context.Vehiculos.FirstOrDefaultAsync(v => v.ClienteId == Cliente.Id);
                return Page();
            }
            var clienteDb = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == Cliente.Id && c.Activo);
            if (clienteDb == null)
            {
                return NotFound();
            }
            // Validación personalizada para Dirección
            if (string.IsNullOrWhiteSpace(Cliente.Direccion) ||
                (!System.Text.RegularExpressions.Regex.IsMatch(Cliente.Direccion, @"\\d") && Cliente.Direccion.Trim().ToUpper() != "S/N"))
            {
                ModelState.AddModelError("Cliente.Direccion", "La dirección debe contener calle y número o decir S/N (sin numeración)");
                return Page();
            }
            // Validación DNI solo números
            if (string.IsNullOrWhiteSpace(Cliente.Dni) || !System.Text.RegularExpressions.Regex.IsMatch(Cliente.Dni, "^\\d+$"))
            {
                ModelState.AddModelError("Cliente.Dni", "El DNI debe contener solo números, sin letras ni caracteres especiales.");
                return Page();
            }
            // Validación DNI único (excepto el propio)
            if (_context.Clientes.Any(c => c.Dni == Cliente.Dni && c.Id != Cliente.Id))
            {
                ModelState.AddModelError("Cliente.Dni", "Ya existe un cliente con ese DNI.");
                return Page();
            }
            // Actualizar campos
            clienteDb.Dni = Cliente.Dni;
            clienteDb.Nombre = Cliente.Nombre;
            clienteDb.Apellido = Cliente.Apellido;
            clienteDb.FechaNacimiento = Cliente.FechaNacimiento;
            clienteDb.Nacionalidad = Cliente.Nacionalidad;
            clienteDb.Direccion = Cliente.Direccion;
            clienteDb.Ciudad = Cliente.Ciudad;
            clienteDb.Provincia = Cliente.Provincia;
            clienteDb.Pais = Cliente.Pais;
            clienteDb.Telefono = Cliente.Telefono;
            clienteDb.Email = Cliente.Email;
            clienteDb.Observaciones = Cliente.Observaciones;
            await _context.SaveChangesAsync();

            // Guardar/actualizar/eliminar vehículo
            var tieneVehiculo = Request.Form["tieneVehiculo"] == "on";
            var vehiculoDb = await _context.Vehiculos.FirstOrDefaultAsync(v => v.ClienteId == Cliente.Id);
            if (tieneVehiculo)
            {
                var patente = Request.Form["Patente"].ToString();
                var marca = Request.Form["Marca"].ToString();
                var modelo = Request.Form["Modelo"].ToString();
                var color = Request.Form["Color"].ToString();
                if (string.IsNullOrWhiteSpace(patente) || string.IsNullOrWhiteSpace(marca) || string.IsNullOrWhiteSpace(modelo) || string.IsNullOrWhiteSpace(color))
                {
                    ModelState.AddModelError("", "Todos los campos del vehículo son obligatorios si el cliente tiene vehículo.");
                    Vehiculo = vehiculoDb ?? new Vehiculo();
                    Vehiculo.Patente = patente;
                    Vehiculo.Marca = marca;
                    Vehiculo.Modelo = modelo;
                    Vehiculo.Color = color;
                    return Page();
                }
                if (vehiculoDb == null)
                {
                    vehiculoDb = new Vehiculo
                    {
                        ClienteId = Cliente.Id
                    };
                    _context.Vehiculos.Add(vehiculoDb);
                }
                vehiculoDb.Patente = patente;
                vehiculoDb.Marca = marca;
                vehiculoDb.Modelo = modelo;
                vehiculoDb.Color = color;
            }
            else if (vehiculoDb != null)
            {
                _context.Vehiculos.Remove(vehiculoDb);
            }
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
