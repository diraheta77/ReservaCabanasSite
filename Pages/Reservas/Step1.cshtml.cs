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
        public List<Temporada> TemporadasDisponibles { get; set; } = new List<Temporada>();

        public async Task OnGetAsync()
        {
            // Cargar cabañas activas
            CabanasDisponibles = await _context.Cabanas
                .Where(c => c.Activa)
                .ToListAsync();
            // Cargar temporadas activas
            TemporadasDisponibles = await _context.Temporadas
                .Where(t => t.Activa)
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
            if (!WizardModel.TemporadaId.HasValue)
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
                CabanasDisponibles = await _context.Cabanas.Where(c => c.Activa).ToListAsync();
                TemporadasDisponibles = await _context.Temporadas.Where(t => t.Activa).ToListAsync();
                return Page();
            }
            // Validar que las fechas sean válidas
            if (WizardModel.FechaDesde >= WizardModel.FechaHasta)
            {
                ModelState.AddModelError("WizardModel.FechaHasta", "La fecha de fin debe ser posterior a la fecha de inicio");
                CabanasDisponibles = await _context.Cabanas.Where(c => c.Activa).ToListAsync();
                TemporadasDisponibles = await _context.Temporadas.Where(t => t.Activa).ToListAsync();
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
                CabanasDisponibles = await _context.Cabanas.Where(c => c.Activa).ToListAsync();
                TemporadasDisponibles = await _context.Temporadas.Where(t => t.Activa).ToListAsync();
                return Page();
            }
            // Asignar precio por persona según temporada seleccionada
            var temporada = await _context.Temporadas.FirstOrDefaultAsync(t => t.Id == WizardModel.TemporadaId);
            if (temporada == null)
            {
                ModelState.AddModelError("WizardModel.TemporadaId", "La temporada seleccionada no es válida");
                CabanasDisponibles = await _context.Cabanas.Where(c => c.Activa).ToListAsync();
                TemporadasDisponibles = await _context.Temporadas.Where(t => t.Activa).ToListAsync();
                return Page();
            }
            WizardModel.PrecioPorPersona = temporada.PrecioPorPersona;
            WizardModel.Temporada = temporada.Nombre;
            // Calcular monto total
            var dias = (WizardModel.FechaHasta - WizardModel.FechaDesde)?.Days ?? 0;
            WizardModel.MontoTotal = WizardModel.PrecioPorPersona * WizardModel.CantidadPersonas * dias;
            try
            {
                // Guardar datos en TempData para el siguiente paso
                var jsonData = System.Text.Json.JsonSerializer.Serialize(WizardModel);
                TempData["WizardData"] = jsonData;
                System.Diagnostics.Debug.WriteLine($"Step1 - Datos guardados en TempData: {jsonData}");
                System.Diagnostics.Debug.WriteLine($"Step1 - CabanaId: {WizardModel.CabanaId}");
                System.Diagnostics.Debug.WriteLine($"Step1 - FechaDesde: {WizardModel.FechaDesde}");
                System.Diagnostics.Debug.WriteLine($"Step1 - FechaHasta: {WizardModel.FechaHasta}");
                TempData["DebugMessage"] = $"Datos guardados: CabanaId={WizardModel.CabanaId}, FechaDesde={WizardModel.FechaDesde:yyyy-MM-dd}";
                System.Diagnostics.Debug.WriteLine("Step1 - Redirigiendo a Step2");
                return RedirectToPage("Step2");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al procesar los datos: {ex.Message}");
                CabanasDisponibles = await _context.Cabanas.Where(c => c.Activa).ToListAsync();
                TemporadasDisponibles = await _context.Temporadas.Where(t => t.Activa).ToListAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostVerificarDisponibilidadAsync(int cabanaId, DateTime fechaDesde, DateTime fechaHasta)
        {
            var existe = await _context.Reservas.AnyAsync(r => r.CabanaId == cabanaId && r.FechaDesde <= fechaHasta && r.FechaHasta >= fechaDesde);
            if (existe)
            {
                return new JsonResult(new { disponible = false, mensaje = "La cabaña no está disponible en las fechas seleccionadas." });
            }
            return new JsonResult(new { disponible = true });
        }
    }
}
