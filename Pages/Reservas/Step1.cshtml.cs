using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;

namespace ReservaCabanasSite.Pages.Reservas
{
    public class Step1Model : PageModel
    {
        private readonly AppDbContext _context;
        public Step1Model(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ReservaWizardViewModel WizardModel { get; set; } = new ReservaWizardViewModel();

        public List<Cabana> CabanasDisponibles { get; set; } = new List<Cabana>();

        public async Task OnGetAsync()
        {
            // Cargar cabañas activas
            CabanasDisponibles = await _context.Cabanas
                .Where(c => c.Activa)
                .ToListAsync();

            // Inicializar fechas por defecto
            if (WizardModel.FechaDesde == default)
            {
                WizardModel.FechaDesde = DateTime.Today.AddDays(1);
            }
            if (WizardModel.FechaHasta == default)
            {
                WizardModel.FechaHasta = DateTime.Today.AddDays(2);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validar solo los campos del Paso 1
            var paso1Errors = new List<string>();
            
            if (WizardModel.CabanaId == 0)
                paso1Errors.Add("Debe seleccionar una cabaña");
            
            if (WizardModel.FechaDesde == default)
                paso1Errors.Add("Debe seleccionar fecha de inicio");
            
            if (WizardModel.FechaHasta == default)
                paso1Errors.Add("Debe seleccionar fecha de fin");
            
            if (string.IsNullOrWhiteSpace(WizardModel.Temporada))
                paso1Errors.Add("Debe seleccionar la temporada");
            
            if (WizardModel.CantidadPersonas <= 0)
                paso1Errors.Add("Debe especificar la cantidad de personas");
            
            if (string.IsNullOrWhiteSpace(WizardModel.MedioContacto))
                paso1Errors.Add("Debe seleccionar el medio de contacto");
            
            if (paso1Errors.Any())
            {
                foreach (var error in paso1Errors)
                {
                    ModelState.AddModelError("", error);
                }
                CabanasDisponibles = await _context.Cabanas
                    .Where(c => c.Activa)
                    .ToListAsync();
                return Page();
            }

            // Validar que las fechas sean válidas
            if (WizardModel.FechaDesde >= WizardModel.FechaHasta)
            {
                ModelState.AddModelError("WizardModel.FechaHasta", "La fecha de fin debe ser posterior a la fecha de inicio");
                CabanasDisponibles = await _context.Cabanas
                    .Where(c => c.Activa)
                    .ToListAsync();
                return Page();
            }

            // Verificar disponibilidad de la cabaña
            var reservasExistentes = await _context.Reservas
                .Where(r => r.CabanaId == WizardModel.CabanaId &&
                           ((r.FechaDesde <= WizardModel.FechaHasta && r.FechaHasta >= WizardModel.FechaDesde)))
                .AnyAsync();

            if (reservasExistentes)
            {
                ModelState.AddModelError("WizardModel.CabanaId", "La cabaña no está disponible para las fechas seleccionadas");
                CabanasDisponibles = await _context.Cabanas
                    .Where(c => c.Activa)
                    .ToListAsync();
                return Page();
            }

            try
            {
                // Guardar datos en TempData para el siguiente paso
                var jsonData = System.Text.Json.JsonSerializer.Serialize(WizardModel);
                TempData["WizardData"] = jsonData;
                
                System.Diagnostics.Debug.WriteLine($"Step1 - Datos guardados en TempData: {jsonData}");
                System.Diagnostics.Debug.WriteLine($"Step1 - CabanaId: {WizardModel.CabanaId}");
                System.Diagnostics.Debug.WriteLine($"Step1 - FechaDesde: {WizardModel.FechaDesde}");
                System.Diagnostics.Debug.WriteLine($"Step1 - FechaHasta: {WizardModel.FechaHasta}");
                
                // Agregar un mensaje de debug
                TempData["DebugMessage"] = $"Datos guardados: CabanaId={WizardModel.CabanaId}, FechaDesde={WizardModel.FechaDesde:yyyy-MM-dd}";
                
                System.Diagnostics.Debug.WriteLine("Step1 - Redirigiendo a Step2");
                return RedirectToPage("Step2");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al procesar los datos: {ex.Message}");
                CabanasDisponibles = await _context.Cabanas
                    .Where(c => c.Activa)
                    .ToListAsync();
                return Page();
            }
        }
    }
}
