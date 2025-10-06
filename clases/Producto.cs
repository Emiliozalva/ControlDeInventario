using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeInventarioASOEM.clases
{
    class Producto
    {
        private int _id;
        private string _nombre;
        private string _modelo;
        private string _marca;
        private int _cantStock;
        Producto(int id, int cant, string nombre, string modelo, string marca)
        {
            if (cant < 0) { cant = cant * (-1); }
            _id = id;
            _nombre = nombre;
            _modelo = modelo;
            _marca = marca;
            _cantStock = cant;
        }
        Producto(int id, string nombre)
        {
            _id = id;
            _nombre = nombre;
            _modelo = "";
            _marca = "";
            _cantStock = 1;
        }
        Producto(int id, string nombre,int cant)
        {   
            if(cant < 0) { cant = cant * (-1); }
            _id = id;
            _nombre = nombre;
            _modelo = "";
            _marca = "";
            _cantStock = cant;
        }
        void modificarNombre(string nombre) { _nombre = nombre; }
        void modificarMarca(string marca) { _marca = marca; }
        void modificarModelo(string modelo) {  _modelo = modelo; }
        void modificarStock(int n) { 
            _cantStock = _cantStock + n;
            if (_cantStock < 1) { _cantStock = 0; }
        }
        int getId() {  return _id; }
        string getMarca() { return _marca; }
        string getModel() { return _modelo; }
        string getNombre() { return _nombre; }
        int getCantStock() { return _cantStock; }
        
    }
}
