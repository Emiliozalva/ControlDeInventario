using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeInventarioASOEM.clases
{
    public class Prestamo
    {
        public int Id { get; set; }
        public string area { get; set; }
        public string persona { get; set; }
        public long fecha1 { get; set; }
        public long? fecha2 { get; set; }
        public string? description { get; set; }
        public int estado { get; set; }
        public int idProducto { get; set; }
        public string producto { get; set; } 
        public string marca { get; set; }
        public string modelo { get; set; }
        public string DescripcionCompleta => $"{producto} {modelo} {marca}";
        public string EstadoTexto => estado == 1 ? "Prestado" : "Devuelto";
        public string FechaInicioTexto => new DateTime(fecha1).ToString("dd/MM/yyyy HH:mm");
        public string FechaDevolucionTexto => (fecha2 == null || fecha2 == 0) ? "-" : new DateTime(fecha2.Value).ToString("dd/MM/yyyy HH:mm");
    }
}
