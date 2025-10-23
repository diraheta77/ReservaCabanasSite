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
    }
}