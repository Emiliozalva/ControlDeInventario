using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeInventarioASOEM.clases
{
    public class Prestamo
    {
        // --- COLUMNAS DE LA BASE DE DATOS ---
        public int Id { get; set; }
        public string area { get; set; } = string.Empty;
        public string persona { get; set; } = string.Empty;
        public long fecha1 { get; set; } 
        public long? fecha2 { get; set; } 
        public string? description { get; set; }
        public int estado { get; set; } 
        public int idProducto { get; set; }
        public string NombreProducto { get; set; } = "Cargando...";

        // Convierte el 1 o 0 en texto
        public string EstadoTexto => estado == 1 ? "Prestado" : "Devuelto";

        // Convierte los Ticks de fecha1 a fecha legible
        public string FechaInicioTexto => new DateTime(fecha1).ToString("dd/MM/yyyy HH:mm");

        // Convierte los Ticks de fecha2 a texto (o vacío si no se devolvió aún)
        public string FechaDevolucionTexto
        {
            get
            {
                if (fecha2 == null || fecha2 == 0) return ""; // Si está vacío, no mostrar nada
                return new DateTime(fecha2.Value).ToString("dd/MM/yyyy HH:mm");
            }
        }
    }
}
