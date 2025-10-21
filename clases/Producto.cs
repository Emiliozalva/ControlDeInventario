using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeInventarioASOEM.clases
{
    public class Producto
    {
        // La Columna se llama "IDproducto" y es la Clave Primaria
        public int IDproducto { get; set; }

        // La Columna se llama "producto"
        public string producto { get; set; }

        // La Columna se llama "cantidadStock"
        public int cantidadStock { get; set; }

        // La Columna se llama "marca"
        public string marca { get; set; }

        // La Columna se llama "modelo"
        public string modelo { get; set; }

        // La Columna se llama "cantidadPrestada"
        public int cantidadPrestada { get; set; }
    }
}

