using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;
using ReservaCabanasSite.Services;
using System.Net.Mail;
using System.Net;

namespace ReservaCabanasSite.Pages.Reservas
{
    [ServiceFilter(typeof(AuthFilter))]
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IExportacionService _exportacionService;

        public DetailsModel(AppDbContext context, IConfiguration config, IExportacionService exportacionService)
        {
            _context = context;
            _config = config;
            _exportacionService = exportacionService;
        }

        public Reserva Reserva { get; set; } = default!;
        public Cliente Cliente { get; set; } = default!;
        public Cabana Cabana { get; set; } = default!;
        public Vehiculo? Vehiculo { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Cabana)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reserva == null)
            {
                return NotFound();
            }

            // Buscar el vehículo del cliente
            var vehiculo = await _context.Vehiculos
                .FirstOrDefaultAsync(v => v.ClienteId == reserva.ClienteId);

            Reserva = reserva;
            Cliente = reserva.Cliente;
            Cabana = reserva.Cabana;
            Vehiculo = vehiculo;

            return Page();
        }

        public async Task<IActionResult> OnPostSendEmailAsync(string emailTo, string emailSubject, string emailMessage)
        {
            try
            {
                // Obtener el ID de la reserva desde la URL
                var routeData = HttpContext.GetRouteData();
                var id = routeData?.Values["id"]?.ToString();
                
                if (string.IsNullOrEmpty(id) || !int.TryParse(id, out int reservaId))
                {
                    return new JsonResult(new { success = false, message = "ID de reserva inválido" });
                }

                // Cargar los datos de la reserva
                var reserva = await _context.Reservas
                    .Include(r => r.Cliente)
                    .Include(r => r.Cabana)
                    .FirstOrDefaultAsync(m => m.Id == reservaId);

                if (reserva == null)
                {
                    return new JsonResult(new { success = false, message = "Reserva no encontrada" });
                }

                // Construir el cuerpo del email usando la misma plantilla que en Confirmacion
                var body = await BuildEmailBody(reserva, emailMessage);

                // Enviar el email
                await SendEmailAsync(emailTo, emailSubject, body);

                return new JsonResult(new { success = true, message = "Email enviado exitosamente" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        private async Task<string> BuildEmailBody(Reserva reserva, string additionalMessage)
        {
            var body = $@"<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; border:1px solid #eee; border-radius:10px; padding:24px;'>
        <div style='text-align:center; margin-bottom:24px;'>
            <img src='https://i.postimg.cc/j2zvrjYK/temp-Imagee-Bh-Ze-J.avif' alt='Aldea Uruel' style='max-height:75px;'>
        </div>
        <h2 style='color:#5c4a45;'>¡Reserva Confirmada!</h2>
        <p>Hola <b>{reserva.Cliente.Nombre}</b>,<br>
        Tu reserva ha sido confirmada. Aquí tienes los detalles:</p>
        <table style='width:100%; margin:16px 0;'>
            <tr><td><b>Cabaña:</b></td><td>{reserva.Cabana.Nombre}</td></tr>
            <tr><td><b>Fechas:</b></td><td>{reserva.FechaDesde:dd/MM/yyyy} - {reserva.FechaHasta:dd/MM/yyyy}</td></tr>
            <tr><td><b>Cantidad de personas:</b></td><td>{reserva.CantidadPersonas}</td></tr>
            <tr><td><b>Método de pago:</b></td><td>{reserva.MetodoPago}</td></tr>
            <tr><td><b>Estado del pago:</b></td><td>{reserva.EstadoPago}</td></tr>
            <tr><td><b>Monto total:</b></td><td><b style='color:#a67c52;'>${reserva.MontoTotal:N2}</b></td></tr>
        </table>";

            // Agregar mensaje adicional si existe
            if (!string.IsNullOrEmpty(additionalMessage))
            {
                body += $@"
        <div style='background: #e3f2fd; border-left: 4px solid #2196f3; padding: 16px; margin: 20px 0; border-radius: 4px;'>
            <h3 style='color: #1976d2; margin-top: 0;'>💬 Mensaje adicional:</h3>
            <p style='color: #1976d2; margin: 8px 0 0 0;'>{additionalMessage}</p>
        </div>";
            }

            body += $@"
        <div style='background: #fff3cd; border-left: 4px solid #ffc107; padding: 16px; margin: 20px 0; border-radius: 4px;'>
            <h3 style='color: #856404; margin-top: 0;'>¡Importante!</h3>
            <ul style='color: #856404; margin: 8px 0; padding-left: 20px;'>
                <li>Guarda el número de reserva (#{reserva.Id}) para futuras consultas</li>
                <li>Revisa tu email para la confirmación detallada</li>
                <li>Para modificaciones o cancelaciones, contacta con nosotros</li>
            </ul>
        </div>
        
        <p style='margin-top:24px;'>¡Gracias por elegirnos!<br>Aldea Uruel</p>
    </div>
</body>
</html>";

            return body;
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // Usar la misma configuración que ya funciona en Confirmacion
                var smtp = new SmtpClient(_config["Email:SmtpHost"])
                {
                    Port = int.Parse(_config["Email:SmtpPort"]),
                    Credentials = new NetworkCredential(_config["Email:SmtpUser"], _config["Email:SmtpPass"]),
                    EnableSsl = true
                };

                var message = new MailMessage(_config["Email:SmtpUser"], to, subject, body)
                {
                    IsBodyHtml = true
                };

                // Agregar múltiples destinatarios si están separados por comas
                var emails = to.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var email in emails)
                {
                    message.To.Add(email.Trim());
                }

                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al enviar el email: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostDescargarRegistroAsync(int reservaId)
        {
            // Generar PDF del registro de reserva
            var resultado = await _exportacionService.GenerarRegistroReservaPdf(reservaId);

            if (resultado.Exito)
            {
                return File(resultado.Archivo, resultado.TipoContenido, resultado.NombreArchivo);
            }
            else
            {
                TempData["Mensaje"] = $"Error al generar el registro: {resultado.Error}";
                return RedirectToPage("Details", new { id = reservaId });
            }
        }

        public async Task<IActionResult> OnPostDescargarTerminosAsync(int reservaId)
        {
            // Generar PDF de términos y condiciones
            var resultado = await _exportacionService.GenerarTerminosCondicionesPdf(reservaId);

            if (resultado.Exito)
            {
                return File(resultado.Archivo, resultado.TipoContenido, resultado.NombreArchivo);
            }
            else
            {
                TempData["Mensaje"] = $"Error al generar términos y condiciones: {resultado.Error}";
                return RedirectToPage("Details", new { id = reservaId });
            }
        }
    }
}
