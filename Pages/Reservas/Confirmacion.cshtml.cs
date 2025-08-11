using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.Reservas
{
    [ServiceFilter(typeof(AuthFilter))]
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

        public async Task<IActionResult> OnGetAsync(int? id = null)
        {
            int reservaId;
            if (id.HasValue)
            {
                reservaId = id.Value;
            }
            else if (TempData["ReservaId"] != null)
            {
                reservaId = (int)TempData["ReservaId"];
            }
            else
            {
                return RedirectToPage("Index");
            }

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
            var body = $@"<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; border:1px solid #eee; border-radius:10px; padding:24px;'>
        <div style='text-align:center; margin-bottom:24px;'>
            <img src='https://i.postimg.cc/j2zvrjYK/temp-Imagee-Bh-Ze-J.avif' alt='Aldea Uruel' style='max-height:75px;'>
        </div>
        <h2 style='color:#5c4a45;'>¡Reserva Confirmada!</h2>
        <p>Hola <b>{cliente.Nombre}</b>,<br>
        Tu reserva ha sido confirmada. Aquí tienes los detalles:</p>
        <table style='width:100%; margin:16px 0;'>
            <tr><td><b>Cabaña:</b></td><td>{cabana.Nombre}</td></tr>
            <tr><td><b>Fechas:</b></td><td>{reserva.FechaDesde:dd/MM/yyyy} - {reserva.FechaHasta:dd/MM/yyyy}</td></tr>
            <tr><td><b>Cantidad de personas:</b></td><td>{reserva.CantidadPersonas}</td></tr>
            <tr><td><b>Método de pago:</b></td><td>{reserva.MetodoPago}</td></tr>
            <tr><td><b>Estado del pago:</b></td><td>{reserva.EstadoPago}</td></tr>
            <tr><td><b>Monto total:</b></td><td><b style='color:#a67c52;'>${reserva.MontoTotal:N2}</b></td></tr>
        </table>
        
        <div style='background: #e3f2fd; border-left: 4px solid #2196f3; padding: 16px; margin: 20px 0; border-radius: 4px;'>
            <h3 style='color: #1976d2; margin-top: 0;'>📅 Horarios de Check-in y Check-out</h3>
            <table style='width:100%; margin:8px 0;'>
                <tr><td><b>Check-out:</b></td><td><strong>Hasta las 10:00 AM</strong></td></tr>
                <tr><td><b>Check-in:</b></td><td><strong>A partir de las 12:00 PM</strong></td></tr>
            </table>
            <p style='color: #1976d2; font-style: italic; margin: 8px 0 0 0; font-size: 0.9rem;'>
                <strong>Nota importante:</strong> El check-out de una reserva y el check-in de otra pueden realizarse en la misma fecha, 
                permitiendo una transición fluida entre huéspedes.
            </p>
        </div>
        
        <p style='margin-top:24px;'>¡Gracias por elegirnos!<br>Aldea Uruel</p>
    </div>
</body>
</html>";
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
                mail.IsBodyHtml = true;
                await smtp.SendMailAsync(mail);
                TempData["Mensaje"] = "Email de confirmación enviado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al enviar el email: {ex.Message}";
            }
            return RedirectToPage("Confirmacion", new { id = reservaId });
        }
    }
} 