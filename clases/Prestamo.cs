using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeInventarioASOEM.clases
{
    public class Prestamo
    {
        // Esta es la Clave Primaria que añadimos a la tabla
        public int Id { get; set; }

        // CAMBIO: Se agregó '?'
        public string? area { get; set; }

        // CAMBIO: Se agregó '?'
        public string? persona { get; set; }

        public long fecha1 { get; set; }
        public long fecha2 { get; set; }

        // CAMBIO: Se agregó '?'
        public string? description { get; set; }

        public int estado { get; set; }

        // Esta es la "Clave Foránea"
        public int idProducto { get; set; }
    }
}
