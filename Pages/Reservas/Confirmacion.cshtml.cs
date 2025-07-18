using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace ReservaCabanasSite.Pages.Reservas
{
    public class ConfirmacionModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        public ConfirmacionModel(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public Reserva Reserva { get; set; }
        public Cliente Cliente { get; set; }
        public Cabana Cabana { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Recuperar IDs de la reserva y cliente creados
            if (TempData["ReservaId"] == null || TempData["ClienteId"] == null)
            {
                return RedirectToPage("Index");
            }

            var reservaId = (int)TempData["ReservaId"];
            var clienteId = (int)TempData["ClienteId"];

            // Cargar datos completos de la reserva
            Reserva = await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Cabana)
                .FirstOrDefaultAsync(r => r.Id == reservaId);

            if (Reserva == null)
            {
                return RedirectToPage("Index");
            }

            Cliente = Reserva.Cliente;
            Cabana = Reserva.Cabana;

            return Page();
        }

        public async Task<IActionResult> OnPostImprimirAsync(int reservaId)
        {
            // Lógica para imprimir comprobante
            // Por ahora solo redirigimos a una página de impresión
            return RedirectToPage("Imprimir", new { id = reservaId });
        }

        public async Task<IActionResult> OnPostEnviarEmailAsync(int reservaId)
        {
            // Cargar datos de la reserva
            var reserva = await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Cabana)
                .FirstOrDefaultAsync(r => r.Id == reservaId);
            if (reserva == null)
            {
                TempData["Mensaje"] = "No se encontró la reserva para enviar el email.";
                return RedirectToPage("Confirmacion");
            }
            var cliente = reserva.Cliente;
            var cabana = reserva.Cabana;
            // Configuración del email
            var to = cliente.Email;
            var subject = $"Confirmación de Reserva #{reserva.Id} - Aldea Uruel";
            var body = $@"Hola {cliente.Nombre},\n\nTu reserva ha sido confirmada.\n\n" +
                $"Cabaña: {cabana.Nombre}\nFechas: {reserva.FechaDesde:dd/MM/yyyy} - {reserva.FechaHasta:dd/MM/yyyy}\n" +
                $"Cantidad de personas: {reserva.CantidadPersonas}\nMonto total: ${reserva.MontoTotal:N2}\n\n" +
                $"Método de pago: {reserva.MetodoPago}\nEstado del pago: {reserva.EstadoPago}\n\n" +
                $"¡Gracias por elegirnos!";
            try
            {
                // Configura aquí tu servidor SMTP real
                var smtp = new SmtpClient(_config["Email:SmtpHost"])
                {
                    Port = int.Parse(_config["Email:SmtpPort"]),
                    Credentials = new NetworkCredential(_config["Email:SmtpUser"], _config["Email:SmtpPass"]),
                    EnableSsl = true
                };
                var mail = new MailMessage(_config["Email:SmtpUser"], to, subject, body);
                await smtp.SendMailAsync(mail);
                TempData["Mensaje"] = "Email de confirmación enviado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al enviar el email: {ex.Message}";
            }
            return RedirectToPage("Confirmacion");
        }
    }
} 