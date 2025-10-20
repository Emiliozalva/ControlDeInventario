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
        public long fecha2 { get; set; }

        public string description { get; set; }
        public int estado { get; set; }
        public int idProducto { get; set; }
    }
}
