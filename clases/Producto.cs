using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeInventarioASOEM.clases
{
    public class Producto
    {
        public int IDproducto { get; set; }
        public string? producto { get; set; }
        public int cantidadStock { get; set; }
        public string? marca { get; set; }
        public string? modelo { get; set; }
        public int cantidadPrestada { get; set; }
        public string DescripcionCompleta => $"{producto} {modelo} {marca}";
    }
}
