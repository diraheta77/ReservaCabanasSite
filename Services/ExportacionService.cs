using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using Color = System.Drawing.Color;

namespace ReservaCabanasSite.Services
{
    public interface IExportacionService
    {
        Task<ResultadoExportacion> ExportarAExcel(DatosExportacion datos);
        Task<ResultadoExportacion> ExportarAPdf(DatosExportacion datos);
        Task<ResultadoExportacion> GenerarRegistroReservaPdf(int reservaId);
    }

    public class ExportacionService : IExportacionService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ExportacionService(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private async Task<DatosEmpresa?> ObtenerDatosEmpresa()
        {
            return await _context.DatosEmpresa
                .Where(d => d.MostrarEnReportes)
                .FirstOrDefaultAsync();
        }

        public async Task<ResultadoExportacion> ExportarAExcel(DatosExportacion datos)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Reporte");

                int filaActual = 1;
                var datosEmpresa = await ObtenerDatosEmpresa();

                // Header: Logo e Información de la Empresa (lado a lado)
                int columnaLogo = 1;
                int columnaInfo = 3; // La información empieza en la columna 3
                int filaInicioHeader = filaActual;

                // Insertar logo si existe (más grande)
                if (datosEmpresa != null && !string.IsNullOrEmpty(datosEmpresa.RutaLogo))
                {
                    try
                    {
                        var rutaCompleta = Path.Combine(_environment.WebRootPath, datosEmpresa.RutaLogo.TrimStart('/'));
                        if (File.Exists(rutaCompleta))
                        {
                            var imagen = worksheet.AddPicture(rutaCompleta)
                                .MoveTo(worksheet.Cell(filaActual, columnaLogo))
                                .Scale(0.3); // Aumentado de 0.15 a 0.3 (el doble)

                            // El logo ahora ocupa más espacio
                        }
                    }
                    catch
                    {
                        // Si falla la carga del logo, continuar sin él
                    }
                }

                // Información de la empresa (al lado del logo)
                if (datosEmpresa != null)
                {
                    // Nombre de la empresa
                    worksheet.Cell(filaActual, columnaInfo).Value = datosEmpresa.NombreEmpresa;
                    worksheet.Cell(filaActual, columnaInfo).Style.Font.FontSize = 14;
                    worksheet.Cell(filaActual, columnaInfo).Style.Font.Bold = true;
                    worksheet.Cell(filaActual, columnaInfo).Style.Font.FontColor = XLColor.FromHtml("#5c4a45");
                    filaActual++;

                    // Dirección completa
                    if (!string.IsNullOrEmpty(datosEmpresa.Direccion))
                    {
                        var direccionCompleta = datosEmpresa.Direccion;
                        if (!string.IsNullOrEmpty(datosEmpresa.Ciudad))
                            direccionCompleta += ", " + datosEmpresa.Ciudad;
                        if (!string.IsNullOrEmpty(datosEmpresa.Provincia))
                            direccionCompleta += ", " + datosEmpresa.Provincia;
                        if (!string.IsNullOrEmpty(datosEmpresa.Pais))
                            direccionCompleta += ", " + datosEmpresa.Pais;

                        worksheet.Cell(filaActual, columnaInfo).Value = direccionCompleta;
                        worksheet.Cell(filaActual, columnaInfo).Style.Font.FontSize = 10;
                        filaActual++;
                    }

                    // Teléfono
                    if (!string.IsNullOrEmpty(datosEmpresa.Telefono))
                    {
                        worksheet.Cell(filaActual, columnaInfo).Value = "Tel: " + datosEmpresa.Telefono;
                        worksheet.Cell(filaActual, columnaInfo).Style.Font.FontSize = 10;
                        filaActual++;
                    }

                    // Email
                    if (!string.IsNullOrEmpty(datosEmpresa.Email))
                    {
                        worksheet.Cell(filaActual, columnaInfo).Value = "Email: " + datosEmpresa.Email;
                        worksheet.Cell(filaActual, columnaInfo).Style.Font.FontSize = 10;
                        filaActual++;
                    }

                    // Sitio Web
                    if (!string.IsNullOrEmpty(datosEmpresa.SitioWeb))
                    {
                        worksheet.Cell(filaActual, columnaInfo).Value = "Web: " + datosEmpresa.SitioWeb;
                        worksheet.Cell(filaActual, columnaInfo).Style.Font.FontSize = 10;
                        filaActual++;
                    }
                }

                // Ajustar fila actual para dejar espacio para el logo (si es más alto que la info)
                filaActual = Math.Max(filaActual, filaInicioHeader + 6);
                filaActual += 2; // Espacio adicional después del header

                // Título del reporte
                worksheet.Cell(filaActual, 1).Value = datos.TituloReporte;
                worksheet.Cell(filaActual, 1).Style.Font.FontSize = 16;
                worksheet.Cell(filaActual, 1).Style.Font.Bold = true;
                worksheet.Cell(filaActual, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(filaActual, 1, filaActual, datos.Columnas.Count).Merge();
                filaActual += 2;

                // Información del período
                worksheet.Cell(filaActual, 1).Value = $"Período: {datos.Periodo}";
                worksheet.Cell(filaActual, 1).Style.Font.Bold = true;
                filaActual++;

                // Filtro adicional si existe
                if (!string.IsNullOrEmpty(datos.FiltroAdicional))
                {
                    worksheet.Cell(filaActual, 1).Value = datos.FiltroAdicional;
                    worksheet.Cell(filaActual, 1).Style.Font.Italic = true;
                    filaActual++;
                }

                filaActual++; // Línea en blanco

                // Estadísticas
                if (datos.Estadisticas.Any())
                {
                    worksheet.Cell(filaActual, 1).Value = "RESUMEN ESTADÍSTICO";
                    worksheet.Cell(filaActual, 1).Style.Font.Bold = true;
                    worksheet.Cell(filaActual, 1).Style.Font.FontSize = 12;
                    filaActual++;

                    foreach (var estadistica in datos.Estadisticas)
                    {
                        worksheet.Cell(filaActual, 1).Value = estadistica.Key;
                        worksheet.Cell(filaActual, 1).Style.Font.Bold = true;
                        worksheet.Cell(filaActual, 2).Value = estadistica.Value?.ToString() ?? "";

                        if (estadistica.Key.Contains("Ingresos") || estadistica.Key.Contains("Promedio"))
                        {
                            worksheet.Cell(filaActual, 2).Style.NumberFormat.Format = "$#,##0";
                        }

                        filaActual++;
                    }
                    filaActual++; // Línea en blanco
                }

                // Encabezados de tabla
                worksheet.Cell(filaActual, 1).Value = "DETALLE DEL REPORTE";
                worksheet.Cell(filaActual, 1).Style.Font.Bold = true;
                worksheet.Cell(filaActual, 1).Style.Font.FontSize = 12;
                filaActual++;

                for (int i = 0; i < datos.Columnas.Count; i++)
                {
                    var columna = datos.Columnas[i];
                    var cell = worksheet.Cell(filaActual, i + 1);
                    cell.Value = columna.Nombre;
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#a67c52");
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Column(i + 1).Width = columna.Ancho;
                }
                filaActual++;

                // Datos
                foreach (var fila in datos.Filas)
                {
                    for (int i = 0; i < datos.Columnas.Count; i++)
                    {
                        var columna = datos.Columnas[i];
                        var clave = columna.Nombre.Replace(" ", "_").ToLower();

                        if (fila.ContainsKey(clave))
                        {
                            var valor = fila[clave];
                            var cell = worksheet.Cell(filaActual, i + 1);
                            cell.Value = valor?.ToString() ?? "";

                            // Formateo según tipo de dato
                            switch (columna.TipoDato.ToLower())
                            {
                                case "currency":
                                    cell.Style.NumberFormat.Format = "$#,##0";
                                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    break;
                                case "number":
                                    cell.Style.NumberFormat.Format = "#,##0";
                                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    break;
                                case "date":
                                    cell.Style.NumberFormat.Format = "dd/mm/yyyy";
                                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    break;
                                default:
                                    if (columna.Alineacion == "center")
                                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    else if (columna.Alineacion == "right")
                                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    break;
                            }
                        }
                    }
                    filaActual++;
                }

                // Bordes para la tabla de datos
                var rango = worksheet.Range(filaActual - datos.Filas.Count - 1, 1, filaActual - 1, datos.Columnas.Count);
                rango.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                rango.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Pie de página con fecha de generación
                filaActual += 2;
                var nombreEmpresaPie = datosEmpresa?.NombreEmpresa ?? "Aldea Auriel";
                var pieCell = worksheet.Cell(filaActual, 1);
                pieCell.Value = $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm} por {nombreEmpresaPie}";
                pieCell.Style.Font.FontSize = 10;
                pieCell.Style.Font.Italic = true;
                pieCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(filaActual, 1, filaActual, datos.Columnas.Count).Merge();

                // Convertir a bytes
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var archivo = stream.ToArray();
                var nombreArchivo = $"{datos.TituloReporte.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return new ResultadoExportacion
                {
                    Archivo = archivo,
                    NombreArchivo = nombreArchivo,
                    TipoContenido = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    Exito = true
                };
            }
            catch (Exception ex)
            {
                return new ResultadoExportacion
                {
                    Exito = false,
                    Error = $"Error al generar Excel: {ex.Message}"
                };
            }
        }

        public async Task<ResultadoExportacion> ExportarAPdf(DatosExportacion datos)
        {
            try
            {
                using var stream = new MemoryStream();
                var document = new Document(PageSize.A4.Rotate(), 25, 25, 30, 30);
                var writer = PdfWriter.GetInstance(document, stream);

                document.Open();

                // Fuentes
                var fuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.BLACK);
                var fuenteSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                var fuenteNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
                var fuenteNormalPequeña = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                var fuenteEmpresa = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13, new BaseColor(92, 74, 69)); // #5c4a45
                var fuenteEncabezado = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);

                var datosEmpresa = await ObtenerDatosEmpresa();

                // Header: Tabla con Logo e Información de la Empresa lado a lado
                var tablaHeader = new PdfPTable(2);
                tablaHeader.WidthPercentage = 100;
                tablaHeader.SetWidths(new float[] { 1, 2 }); // Logo: 1 parte, Info: 2 partes
                tablaHeader.SpacingAfter = 15f;

                // Celda del Logo
                PdfPCell celdaLogo;
                if (datosEmpresa != null && !string.IsNullOrEmpty(datosEmpresa.RutaLogo))
                {
                    try
                    {
                        var rutaCompleta = Path.Combine(_environment.WebRootPath, datosEmpresa.RutaLogo.TrimStart('/'));
                        if (File.Exists(rutaCompleta))
                        {
                            var imagen = iTextSharp.text.Image.GetInstance(rutaCompleta);
                            imagen.ScaleToFit(180f, 90f); // Aumentado de 120x60 a 180x90 (50% más grande)
                            celdaLogo = new PdfPCell(imagen);
                            celdaLogo.HorizontalAlignment = Element.ALIGN_CENTER;
                            celdaLogo.VerticalAlignment = Element.ALIGN_MIDDLE;
                        }
                        else
                        {
                            celdaLogo = new PdfPCell(new Phrase(""));
                        }
                    }
                    catch
                    {
                        celdaLogo = new PdfPCell(new Phrase(""));
                    }
                }
                else
                {
                    celdaLogo = new PdfPCell(new Phrase(""));
                }
                celdaLogo.Border = iTextSharp.text.Rectangle.NO_BORDER;
                celdaLogo.PaddingRight = 10f;
                tablaHeader.AddCell(celdaLogo);

                // Celda de Información de la Empresa
                var infoEmpresa = new Paragraph();
                if (datosEmpresa != null)
                {
                    // Nombre de la empresa
                    infoEmpresa.Add(new Chunk(datosEmpresa.NombreEmpresa + "\n", fuenteEmpresa));

                    // Dirección completa
                    if (!string.IsNullOrEmpty(datosEmpresa.Direccion))
                    {
                        var direccionCompleta = datosEmpresa.Direccion;
                        if (!string.IsNullOrEmpty(datosEmpresa.Ciudad))
                            direccionCompleta += ", " + datosEmpresa.Ciudad;
                        if (!string.IsNullOrEmpty(datosEmpresa.Provincia))
                            direccionCompleta += ", " + datosEmpresa.Provincia;
                        if (!string.IsNullOrEmpty(datosEmpresa.Pais))
                            direccionCompleta += ", " + datosEmpresa.Pais;

                        infoEmpresa.Add(new Chunk(direccionCompleta + "\n", fuenteNormalPequeña));
                    }

                    // Teléfono
                    if (!string.IsNullOrEmpty(datosEmpresa.Telefono))
                    {
                        infoEmpresa.Add(new Chunk("Tel: " + datosEmpresa.Telefono + "\n", fuenteNormalPequeña));
                    }

                    // Email
                    if (!string.IsNullOrEmpty(datosEmpresa.Email))
                    {
                        infoEmpresa.Add(new Chunk("Email: " + datosEmpresa.Email + "\n", fuenteNormalPequeña));
                    }

                    // Sitio Web
                    if (!string.IsNullOrEmpty(datosEmpresa.SitioWeb))
                    {
                        infoEmpresa.Add(new Chunk("Web: " + datosEmpresa.SitioWeb, fuenteNormalPequeña));
                    }
                }
                else
                {
                    infoEmpresa.Add(new Chunk("Aldea Auriel", fuenteEmpresa));
                }

                var celdaInfo = new PdfPCell(infoEmpresa);
                celdaInfo.Border = iTextSharp.text.Rectangle.NO_BORDER;
                celdaInfo.VerticalAlignment = Element.ALIGN_MIDDLE;
                celdaInfo.PaddingLeft = 10f;
                tablaHeader.AddCell(celdaInfo);

                document.Add(tablaHeader);

                // Línea separadora
                var linea = new Paragraph("_________________________________________________________________");
                linea.Alignment = Element.ALIGN_CENTER;
                linea.SpacingAfter = 15f;
                document.Add(linea);

                // Título
                var titulo = new Paragraph(datos.TituloReporte, fuenteTitulo);
                titulo.Alignment = Element.ALIGN_CENTER;
                titulo.SpacingAfter = 10;
                document.Add(titulo);

                // Información del período
                var periodo = new Paragraph($"Período: {datos.Periodo}", fuenteNormal);
                periodo.SpacingAfter = 5;
                document.Add(periodo);

                // Filtro adicional
                if (!string.IsNullOrEmpty(datos.FiltroAdicional))
                {
                    var filtro = new Paragraph(datos.FiltroAdicional, fuenteNormal);
                    filtro.SpacingAfter = 5;
                    document.Add(filtro);
                }

                // Estadísticas
                if (datos.Estadisticas.Any())
                {
                    var resumenTitulo = new Paragraph("RESUMEN ESTADÍSTICO", fuenteSubtitulo);
                    resumenTitulo.SpacingBefore = 10;
                    resumenTitulo.SpacingAfter = 5;
                    document.Add(resumenTitulo);

                    var tablaEstadisticas = new PdfPTable(2);
                    tablaEstadisticas.WidthPercentage = 50;
                    tablaEstadisticas.HorizontalAlignment = Element.ALIGN_LEFT;

                    foreach (var estadistica in datos.Estadisticas)
                    {
                        tablaEstadisticas.AddCell(new PdfPCell(new Phrase(estadistica.Key, fuenteNormal)));

                        string valorTexto = estadistica.Value?.ToString() ?? "";
                        if (estadistica.Key.Contains("Ingresos") || estadistica.Key.Contains("Promedio"))
                        {
                            if (decimal.TryParse(valorTexto, out decimal valor))
                            {
                                valorTexto = $"${valor:N0}";
                            }
                        }

                        tablaEstadisticas.AddCell(new PdfPCell(new Phrase(valorTexto, fuenteNormal)));
                    }

                    document.Add(tablaEstadisticas);
                }

                // Tabla de datos
                var detalleTitulo = new Paragraph("DETALLE DEL REPORTE", fuenteSubtitulo);
                detalleTitulo.SpacingBefore = 15;
                detalleTitulo.SpacingAfter = 10;
                document.Add(detalleTitulo);

                if (datos.Filas.Any())
                {
                    var tabla = new PdfPTable(datos.Columnas.Count);
                    tabla.WidthPercentage = 100;

                    // Anchos relativos
                    var anchos = datos.Columnas.Select(c => (float)c.Ancho).ToArray();
                    tabla.SetWidths(anchos);

                    // Encabezados
                    foreach (var columna in datos.Columnas)
                    {
                        var celda = new PdfPCell(new Phrase(columna.Nombre, fuenteEncabezado));
                        celda.BackgroundColor = new BaseColor(166, 124, 82); // #a67c52
                        celda.HorizontalAlignment = Element.ALIGN_CENTER;
                        celda.Padding = 8;
                        tabla.AddCell(celda);
                    }

                    // Datos
                    foreach (var fila in datos.Filas)
                    {
                        foreach (var columna in datos.Columnas)
                        {
                            var clave = columna.Nombre.Replace(" ", "_").ToLower();
                            var valor = fila.ContainsKey(clave) ? fila[clave]?.ToString() ?? "" : "";

                            // Formateo según tipo
                            if (columna.TipoDato == "currency" && decimal.TryParse(valor, out decimal valorDecimal))
                            {
                                valor = $"${valorDecimal:N0}";
                            }

                            var celda = new PdfPCell(new Phrase(valor, fuenteNormal));
                            celda.Padding = 5;

                            switch (columna.Alineacion.ToLower())
                            {
                                case "center":
                                    celda.HorizontalAlignment = Element.ALIGN_CENTER;
                                    break;
                                case "right":
                                    celda.HorizontalAlignment = Element.ALIGN_RIGHT;
                                    break;
                                default:
                                    celda.HorizontalAlignment = Element.ALIGN_LEFT;
                                    break;
                            }

                            tabla.AddCell(celda);
                        }
                    }

                    document.Add(tabla);
                }

                // Pie de página
                var nombreEmpresaPdf = datosEmpresa?.NombreEmpresa ?? "Aldea Auriel";
                var piePagina = new Paragraph($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm} por {nombreEmpresaPdf}",
                    FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8, BaseColor.GRAY));
                piePagina.Alignment = Element.ALIGN_CENTER;
                piePagina.SpacingBefore = 20;
                document.Add(piePagina);

                document.Close();

                var archivo = stream.ToArray();
                var nombreArchivo = $"{datos.TituloReporte.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                return new ResultadoExportacion
                {
                    Archivo = archivo,
                    NombreArchivo = nombreArchivo,
                    TipoContenido = "application/pdf",
                    Exito = true
                };
            }
            catch (Exception ex)
            {
                return new ResultadoExportacion
                {
                    Exito = false,
                    Error = $"Error al generar PDF: {ex.Message}"
                };
            }
        }

        public async Task<ResultadoExportacion> GenerarRegistroReservaPdf(int reservaId)
        {
            try
            {
                // Obtener datos de la reserva con todas las relaciones
                var reserva = await _context.Reservas
                    .Include(r => r.Cliente)
                        .ThenInclude(c => c.Vehiculos)
                    .Include(r => r.Cabana)
                    .FirstOrDefaultAsync(r => r.Id == reservaId);

                if (reserva == null)
                {
                    return new ResultadoExportacion
                    {
                        Exito = false,
                        Error = "Reserva no encontrada"
                    };
                }

                var datosEmpresa = await ObtenerDatosEmpresa();

                using var stream = new MemoryStream();
                var document = new Document(PageSize.A4, 40, 40, 40, 40); // Márgenes
                var writer = PdfWriter.GetInstance(document, stream);

                document.Open();

                // Fuentes
                var fuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
                var fuenteSeccion = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                var fuenteLabel = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK);
                var fuenteNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
                var fuentePequeña = FontFactory.GetFont(FontFactory.HELVETICA, 9, new BaseColor(92, 74, 69));

                // ===== HEADER: Logo y Datos de Empresa =====
                var tablaHeader = new PdfPTable(2);
                tablaHeader.WidthPercentage = 100;
                tablaHeader.SetWidths(new float[] { 1, 2 });
                tablaHeader.SpacingAfter = 10f;

                // Logo
                PdfPCell celdaLogo;
                if (datosEmpresa != null && !string.IsNullOrEmpty(datosEmpresa.RutaLogo))
                {
                    try
                    {
                        var rutaCompleta = Path.Combine(_environment.WebRootPath, datosEmpresa.RutaLogo.TrimStart('/'));
                        if (File.Exists(rutaCompleta))
                        {
                            var imagen = iTextSharp.text.Image.GetInstance(rutaCompleta);
                            imagen.ScaleToFit(120f, 80f);
                            celdaLogo = new PdfPCell(imagen);
                            celdaLogo.HorizontalAlignment = Element.ALIGN_LEFT;
                            celdaLogo.VerticalAlignment = Element.ALIGN_TOP;
                        }
                        else
                        {
                            celdaLogo = new PdfPCell(new Phrase("[LOGO - EMPRESA]", fuenteLabel));
                        }
                    }
                    catch
                    {
                        celdaLogo = new PdfPCell(new Phrase("[LOGO - EMPRESA]", fuenteLabel));
                    }
                }
                else
                {
                    celdaLogo = new PdfPCell(new Phrase("[LOGO - EMPRESA]", fuenteLabel));
                }
                celdaLogo.Border = iTextSharp.text.Rectangle.NO_BORDER;
                celdaLogo.PaddingTop = 5f;
                tablaHeader.AddCell(celdaLogo);

                // Información empresa + Fecha
                var infoEmpresa = new Paragraph();
                if (datosEmpresa != null)
                {
                    infoEmpresa.Add(new Chunk(datosEmpresa.NombreEmpresa + "\n", fuentePequeña));
                    if (!string.IsNullOrEmpty(datosEmpresa.Direccion))
                        infoEmpresa.Add(new Chunk(datosEmpresa.Direccion + "\n", fuentePequeña));

                    var ubicacion = "";
                    if (!string.IsNullOrEmpty(datosEmpresa.Provincia))
                        ubicacion = datosEmpresa.Provincia;
                    if (!string.IsNullOrEmpty(datosEmpresa.Ciudad))
                        ubicacion += (string.IsNullOrEmpty(ubicacion) ? "" : " - ") + datosEmpresa.Ciudad;
                    if (!string.IsNullOrEmpty(ubicacion))
                        infoEmpresa.Add(new Chunk(ubicacion + "\n", fuentePequeña));

                    if (!string.IsNullOrEmpty(datosEmpresa.Telefono))
                        infoEmpresa.Add(new Chunk(datosEmpresa.Telefono + "\n", fuentePequeña));
                }

                var celdaInfo = new PdfPCell(infoEmpresa);
                celdaInfo.Border = iTextSharp.text.Rectangle.NO_BORDER;
                celdaInfo.HorizontalAlignment = Element.ALIGN_RIGHT;
                celdaInfo.VerticalAlignment = Element.ALIGN_TOP;
                celdaInfo.PaddingTop = 5f;
                tablaHeader.AddCell(celdaInfo);

                document.Add(tablaHeader);

                // Fecha actual (arriba a la derecha)
                var fechaActual = new Paragraph(DateTime.Now.ToString("dd/MM/yyyy"), fuenteNormal);
                fechaActual.Alignment = Element.ALIGN_RIGHT;
                fechaActual.SpacingAfter = 10f;
                document.Add(fechaActual);

                // ===== TÍTULO =====
                var titulo = new Paragraph("REGISTRO DE RESERVA", fuenteTitulo);
                titulo.Alignment = Element.ALIGN_CENTER;
                titulo.SpacingAfter = 5f;
                document.Add(titulo);

                var numeroReserva = new Paragraph($"N° {reserva.Id.ToString().PadLeft(10, '0')}", fuenteSeccion);
                numeroReserva.Alignment = Element.ALIGN_CENTER;
                numeroReserva.SpacingAfter = 15f;
                document.Add(numeroReserva);

                // Línea separadora
                var linea1 = new Paragraph(new string('_', 100));
                linea1.SpacingAfter = 15f;
                document.Add(linea1);

                // ===== DATOS DE LA CABAÑA =====
                var tablaCabana = new PdfPTable(2);
                tablaCabana.WidthPercentage = 100;
                tablaCabana.SetWidths(new float[] { 1, 1 });
                tablaCabana.SpacingAfter = 10f;

                var celdaCabana = new PdfPCell(new Phrase($"Cabaña: {reserva.Cabana?.Nombre ?? "N/A"}", fuenteLabel));
                celdaCabana.Border = iTextSharp.text.Rectangle.NO_BORDER;
                celdaCabana.Colspan = 2;
                celdaCabana.PaddingBottom = 10f;
                tablaCabana.AddCell(celdaCabana);

                var celdaFechaIngreso = new PdfPCell(new Phrase($"Fecha de Ingreso: {reserva.FechaDesde:dd/MM/yyyy}", fuenteNormal));
                celdaFechaIngreso.Border = iTextSharp.text.Rectangle.BOTTOM_BORDER;
                celdaFechaIngreso.PaddingBottom = 5f;
                tablaCabana.AddCell(celdaFechaIngreso);

                var celdaFechaEgreso = new PdfPCell(new Phrase($"Fecha de Egreso: {reserva.FechaHasta:dd/MM/yyyy}", fuenteNormal));
                celdaFechaEgreso.Border = iTextSharp.text.Rectangle.BOTTOM_BORDER;
                celdaFechaEgreso.PaddingBottom = 5f;
                tablaCabana.AddCell(celdaFechaEgreso);

                document.Add(tablaCabana);

                // ===== DATOS DEL TITULAR =====
                var tituloTitular = new Paragraph("Datos del Titular de la Reserva", fuenteSeccion);
                tituloTitular.SpacingBefore = 15f;
                tituloTitular.SpacingAfter = 10f;
                document.Add(tituloTitular);

                var lineaSeparadora = new Paragraph(new string('_', 100));
                lineaSeparadora.SpacingAfter = 10f;
                document.Add(lineaSeparadora);

                // Tabla para datos del titular (2 columnas)
                var tablaTitular = new PdfPTable(2);
                tablaTitular.WidthPercentage = 100;
                tablaTitular.SetWidths(new float[] { 1, 1 });
                tablaTitular.SpacingAfter = 10f;

                var cliente = reserva.Cliente;
                var vehiculo = cliente?.Vehiculos?.FirstOrDefault();

                // Apellido y Nombre (full width)
                var celdaNombre = new PdfPCell(new Phrase($"Apellido y Nombre: {cliente?.Apellido ?? ""} {cliente?.Nombre ?? ""}", fuenteNormal));
                celdaNombre.Border = iTextSharp.text.Rectangle.BOTTOM_BORDER;
                celdaNombre.Colspan = 2;
                celdaNombre.PaddingBottom = 5f;
                celdaNombre.PaddingTop = 5f;
                tablaTitular.AddCell(celdaNombre);

                // DNI y Fecha de Nacimiento
                var celdaDni = new PdfPCell(new Phrase($"DNI: {cliente?.Dni ?? ""}", fuenteNormal));
                celdaDni.Border = iTextSharp.text.Rectangle.BOTTOM_BORDER;
                celdaDni.PaddingBottom = 5f;
                celdaDni.PaddingTop = 5f;
                tablaTitular.AddCell(celdaDni);

                var celdaFechaNac = new PdfPCell(new Phrase($"Fecha de Nacimiento: {(cliente?.FechaNacimiento.HasValue == true ? cliente.FechaNacimiento.Value.ToString("dd/MM/yyyy") : "")}", fuenteNormal));
                celdaFechaNac.Border = iTextSharp.text.Rectangle.BOTTOM_BORDER;
                celdaFechaNac.PaddingBottom = 5f;
                celdaFechaNac.PaddingTop = 5f;
                tablaTitular.AddCell(celdaFechaNac);

                // Dirección (full width)
                var celdaDireccion = new PdfPCell(new Phrase($"Dirección: {cliente?.Direccion ?? ""}", fuenteNormal));
                celdaDireccion.Border = iTextSharp.text.Rectangle.BOTTOM_BORDER;
                celdaDireccion.Colspan = 2;
                celdaDireccion.PaddingBottom = 5f;
                celdaDireccion.PaddingTop = 5f;
                tablaTitular.AddCell(celdaDireccion);

                // Provincia y Localidad
                var celdaProvincia = new PdfPCell(new Phrase($"Provincia: {cliente?.Provincia ?? ""}", fuenteNormal));
                celdaProvincia.Border = iTextSharp.text.Rectangle.BOTTOM_BORDER;
                celdaProvincia.PaddingBottom = 5f;
                celdaProvincia.PaddingTop = 5f;
                tablaTitular.AddCell(celdaProvincia);

                var celdaLocalidad = new PdfPCell(new Phrase($"Localidad: {cliente?.Ciudad ?? ""}", fuenteNormal));
                celdaLocalidad.Border = iTextSharp.text.Rectangle.BOTTOM_BORDER;
                celdaLocalidad.PaddingBottom = 5f;
                celdaLocalidad.PaddingTop = 5f;
                tablaTitular.AddCell(celdaLocalidad);

                // Teléfono y Email
                var celdaTelefono = new PdfPCell(new Phrase($"Teléfono: {cliente?.Telefono ?? ""}", fuenteNormal));
                celdaTelefono.Border = iTextSharp.text.Rectangle.BOTTOM_BORDER;
                celdaTelefono.PaddingBottom = 5f;
                celdaTelefono.PaddingTop = 5f;
                tablaTitular.AddCell(celdaTelefono);

                var celdaEmail = new PdfPCell(new Phrase($"E-mail: {cliente?.Email ?? ""}", fuenteNormal));
                celdaEmail.Border = iTextSharp.text.Rectangle.BOTTOM_BORDER;
                celdaEmail.PaddingBottom = 5f;
                celdaEmail.PaddingTop = 5f;
                tablaTitular.AddCell(celdaEmail);

                // Vehículo y Patente
                var celdaVehiculo = new PdfPCell(new Phrase($"Vehículo: {vehiculo?.Modelo ?? ""}", fuenteNormal));
                celdaVehiculo.Border = iTextSharp.text.Rectangle.BOTTOM_BORDER;
                celdaVehiculo.PaddingBottom = 5f;
                celdaVehiculo.PaddingTop = 5f;
                tablaTitular.AddCell(celdaVehiculo);

                var celdaPatente = new PdfPCell(new Phrase($"Patente: {vehiculo?.Patente ?? ""}", fuenteNormal));
                celdaPatente.Border = iTextSharp.text.Rectangle.BOTTOM_BORDER;
                celdaPatente.PaddingBottom = 5f;
                celdaPatente.PaddingTop = 5f;
                tablaTitular.AddCell(celdaPatente);

                document.Add(tablaTitular);

                // ===== DATOS DE ACOMPAÑANTES (5 secciones vacías) =====
                for (int i = 1; i <= 5; i++)
                {
                    var tituloAcomp = new Paragraph($"Datos de Acompañante {i}", fuenteSeccion);
                    tituloAcomp.SpacingBefore = 15f;
                    tituloAcomp.SpacingAfter = 10f;
                    document.Add(tituloAcomp);

                    var lineaAcomp = new Paragraph(new string('_', 100));
                    lineaAcomp.SpacingAfter = 10f;
                    document.Add(lineaAcomp);

                    var tablaAcomp = new PdfPTable(2);
                    tablaAcomp.WidthPercentage = 100;
                    tablaAcomp.SetWidths(new float[] { 1, 1 });
                    tablaAcomp.SpacingAfter = 10f;

                    // Apellido y Nombre
                    var celdaNombreAcomp = new PdfPCell(new Phrase("Apellido y Nombre: _______________________________", fuenteNormal));
                    celdaNombreAcomp.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    celdaNombreAcomp.Colspan = 2;
                    celdaNombreAcomp.PaddingBottom = 5f;
                    celdaNombreAcomp.PaddingTop = 5f;
                    tablaAcomp.AddCell(celdaNombreAcomp);

                    // DNI
                    var celdaDniAcomp = new PdfPCell(new Phrase("DNI: _______________________________", fuenteNormal));
                    celdaDniAcomp.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    celdaDniAcomp.PaddingBottom = 5f;
                    celdaDniAcomp.PaddingTop = 5f;
                    tablaAcomp.AddCell(celdaDniAcomp);

                    // Teléfono
                    var celdaTelAcomp = new PdfPCell(new Phrase("Teléfono: _______________________________", fuenteNormal));
                    celdaTelAcomp.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    celdaTelAcomp.PaddingBottom = 5f;
                    celdaTelAcomp.PaddingTop = 5f;
                    tablaAcomp.AddCell(celdaTelAcomp);

                    document.Add(tablaAcomp);
                }

                document.Close();

                var archivo = stream.ToArray();
                var nombreArchivo = $"Registro_Reserva_{reserva.Id}_{DateTime.Now:yyyyMMdd}.pdf";

                return new ResultadoExportacion
                {
                    Archivo = archivo,
                    NombreArchivo = nombreArchivo,
                    TipoContenido = "application/pdf",
                    Exito = true
                };
            }
            catch (Exception ex)
            {
                return new ResultadoExportacion
                {
                    Exito = false,
                    Error = $"Error al generar registro de reserva: {ex.Message}"
                };
            }
        }
    }
}