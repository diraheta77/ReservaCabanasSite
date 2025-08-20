using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Filters;
using System.Net.Mail;
using System.Net;

namespace ReservaCabanasSite.Pages.Reservas
{
    [ServiceFilter(typeof(AuthFilter))]
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public DetailsModel(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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
            // Usar la misma plantilla que en Confirmacion.cshtml.cs
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: #a67c52; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                        .content {{ background: #f9f9f9; padding: 20px; border-radius: 0 0 8px 8px; }}
                        .reservation-details {{ background: white; padding: 15px; margin: 15px 0; border-radius: 8px; border-left: 4px solid #a67c52; }}
                        .detail-row {{ display: flex; justify-content: space-between; margin: 8px 0; }}
                        .detail-label {{ font-weight: bold; color: #5c4a45; }}
                        .detail-value {{ color: #6c757d; }}
                        .total {{ background: #e3f2fd; padding: 15px; border-radius: 8px; margin: 15px 0; }}
                        .total-amount {{ font-size: 1.2em; font-weight: bold; color: #a67c52; }}
                        .note {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 16px; margin: 20px 0; border-radius: 4px; }}
                        .footer {{ text-align: center; margin-top: 20px; color: #6c757d; font-size: 0.9em; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🏠 AldeaUruel</h1>
                            <h2>Detalles de Reserva #{reserva.Id}</h2>
                        </div>
                        
                        <div class='content'>
                            <div class='reservation-details'>
                                <h3>📅 Información de la Reserva</h3>
                                <div class='detail-row'>
                                    <span class='detail-label'>Cabaña:</span>
                                    <span class='detail-value'>{reserva.Cabana.Nombre}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='detail-label'>Fecha de llegada:</span>
                                    <span class='detail-value'>{reserva.FechaDesde:dd/MM/yyyy}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='detail-label'>Fecha de salida:</span>
                                    <span class='detail-value'>{reserva.FechaHasta:dd/MM/yyyy}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='detail-label'>Cantidad de personas:</span>
                                    <span class='detail-value'>{reserva.CantidadPersonas}</span>
                                </div>
                            </div>

                            <div class='reservation-details'>
                                <h3>👤 Información del Cliente</h3>
                                <div class='detail-row'>
                                    <span class='detail-label'>Nombre:</span>
                                    <span class='detail-value'>{reserva.Cliente.Nombre} {reserva.Cliente.Apellido}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='detail-label'>DNI:</span>
                                    <span class='detail-value'>{reserva.Cliente.Dni}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='detail-label'>Teléfono:</span>
                                    <span class='detail-value'>{reserva.Cliente.Telefono}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='detail-label'>Email:</span>
                                    <span class='detail-value'>{reserva.Cliente.Email}</span>
                                </div>
                            </div>

                            <div class='reservation-details'>
                                <h3>💳 Información de Pago</h3>
                                <div class='detail-row'>
                                    <span class='detail-label'>Método de pago:</span>
                                    <span class='detail-value'>{reserva.MetodoPago}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='detail-label'>Estado del pago:</span>
                                    <span class='detail-value'>{reserva.EstadoPago}</span>
                                </div>
                            </div>

                            <div class='total'>
                                <div class='detail-row'>
                                    <span class='detail-label'>Monto total:</span>
                                    <span class='detail-value total-amount'>${reserva.MontoTotal:N2}</span>
                                </div>
                            </div>";

            // Agregar mensaje adicional si existe
            if (!string.IsNullOrEmpty(additionalMessage))
            {
                body += $@"
                            <div class='note'>
                                <h4>💬 Mensaje adicional:</h4>
                                <p>{additionalMessage}</p>
                            </div>";
            }

            body += $@"
                            <div class='note'>
                                <h3 style='color: #e65100; margin-top: 0;'>¡Importante!</h3>
                                <p style='color: #e65100; margin: 8px 0 0 0; font-size: 0.9rem;'>
                                    Por favor, guarda este número de reserva para futuras consultas: <strong>{reserva.Id}</strong>.<br>
                                    Para cualquier modificación o cancelación, por favor contáctanos directamente.
                                </p>
                            </div>
                        </div>
                        
                        <div class='footer'>
                            <p>© 2025 AldeaUruel - Sistema de Administración de Cabañas</p>
                        </div>
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
    }
}
