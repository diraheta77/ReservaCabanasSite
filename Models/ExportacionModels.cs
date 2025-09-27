namespace ReservaCabanasSite.Models
{
    public class DatosExportacion
    {
        public string TituloReporte { get; set; } = "";
        public string Periodo { get; set; } = "";
        public string? FiltroAdicional { get; set; }
        public List<ColumnaExportacion> Columnas { get; set; } = new();
        public List<Dictionary<string, object>> Filas { get; set; } = new();
        public Dictionary<string, object> Estadisticas { get; set; } = new();
        public byte[]? LogoEmpresa { get; set; }
        public string NombreEmpresa { get; set; } = "Aldea Auriel";
    }

    public class ColumnaExportacion
    {
        public string Nombre { get; set; } = "";
        public string TipoDato { get; set; } = "string"; // string, number, date, currency
        public int Ancho { get; set; } = 15; // Para Excel
        public string Alineacion { get; set; } = "left"; // left, center, right
    }

    public class ResultadoExportacion
    {
        public byte[] Archivo { get; set; } = Array.Empty<byte>();
        public string NombreArchivo { get; set; } = "";
        public string TipoContenido { get; set; } = "";
        public bool Exito { get; set; }
        public string? Error { get; set; }
    }
}