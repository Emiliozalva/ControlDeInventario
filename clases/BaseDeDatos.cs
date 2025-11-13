using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper; 
using System.IO;

namespace SistemaDeInventarioASOEM.clases
{
    public class BaseDeDatos
    {
        private readonly string _connectionString;

        public BaseDeDatos()
        {
            string nombreArchivoDB = "BaseProductos.db";

            

            // Ruta de Destino (AppData): C:\Users\<tu_usuario>\AppData\Local\SistemaDeInventarioASOEM
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolderPath = Path.Combine(appDataPath, "SistemaDeInventarioASOEM");
            Directory.CreateDirectory(appFolderPath); // Asegura que la carpeta exista
            string rutaDestinoDB = Path.Combine(appFolderPath, nombreArchivoDB);

            // Ruta de Origen (junto al .exe, en bin\Debug o en la carpeta de instalación)
            string rutaOrigenDB = Path.Combine(AppContext.BaseDirectory, nombreArchivoDB);

            // --- 2. Lógica de Copia ---
            // Copia la base de datos "semilla" (la que tiene tus datos) 
            // a AppData SÓLO si no existe una ya allí.
            if (!File.Exists(rutaDestinoDB))
            {
                if (File.Exists(rutaOrigenDB))
                {
                    File.Copy(rutaOrigenDB, rutaDestinoDB);
                }
                else
                {
                    // Esto pasaría si olvidaste poner el .db con "Copiar si es posterior"
                    throw new FileNotFoundException("¡Error Crítico! No se encontró la base de datos de origen.", rutaOrigenDB);
                }
            }

            // --- 3. Conexión ---
            
            _connectionString = $"Data Source={rutaDestinoDB}"; // La conexión SIEMPRE apunta a la copia en AppData (la que es editable)

            // --- 4. Inicializar/Arreglar Tablas ---
            // Esto asegura que tus tablas tengan Claves Primarias.
            // No borra datos existentes.
            InicializarEstructuraDB();
        }
        /// <summary>
        /// Asegura que las tablas existan y tengan la estructura correcta
        /// (especialmente Claves Primarias).
        /// </summary>
        private void InicializarEstructuraDB()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // 1. Crea/Modifica la tabla "stock"
                // Hacemos IDproducto la Clave Primaria.
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS stock (
                        IDproducto INTEGER PRIMARY KEY AUTOINCREMENT,
                        producto TEXT NOT NULL,
                        cantidadStock INTEGER NOT NULL,
                        marca TEXT,
                        modelo TEXT,
                        cantidadPrestada INTEGER
                    );
                ");

                // 2. Crea/Modifica la tabla "prestamos"
                // Añadimos una Clave Primaria 'Id' y la Clave Foránea.
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS prestamos (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        area TEXT NOT NULL,
                        persona TEXT NOT NULL,
                        fecha1 INTEGER NOT NULL,
                        fecha2 INTEGER,
                        description TEXT,
                        estado INTEGER NOT NULL,
                        idProducto INTEGER,
                        FOREIGN KEY(idProducto) REFERENCES stock(IDproducto)
                    );
                ");
            }
        }

        // =======================================================
        // --- MÉTODOS PARA PRODUCTOS (TABLA "stock") ---
        // =======================================================

        public List<Producto> ObtenerTodosLosProductos()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                // Dapper lee de "stock" y lo convierte en List<Producto>
                return connection.Query<Producto>("SELECT * FROM stock").ToList();
            }
        }

        public void AgregarProducto(Producto producto)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                // Dapper mapea las propiedades de la clase a los @...
                string sql = @"
                    INSERT INTO stock (producto, cantidadStock, marca, modelo, cantidadPrestada) 
                    VALUES (@producto, @cantidadStock, @marca, @modelo, @cantidadPrestada);";

                connection.Execute(sql, producto);
            }
        }

        public void ActualizarProducto(Producto producto)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = @"
                    UPDATE stock 
                    SET producto = @producto, 
                        cantidadStock = @cantidadStock, 
                        marca = @marca, 
                        modelo = @modelo,
                        cantidadPrestada = @cantidadPrestada
                    WHERE IDproducto = @IDproducto;"; 
                connection.Execute(sql, producto);
            }
        }

        public void BorrarProducto(int idProducto)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = "DELETE FROM stock WHERE IDproducto = @Id;";
                connection.Execute(sql, new { Id = idProducto });
            }
        }

        // =========================================================
        // --- MÉTODOS PARA PRESTAMOS (TABLA "prestamos") ---
        // =========================================================

        /// <summary>
        /// Obtiene todos los préstamos de un producto específico.
        /// </summary>
        public List<Prestamo> ObtenerPrestamosDeProducto(int idProducto)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = "SELECT * FROM prestamos WHERE idProducto = @IdProd;";
                return connection.Query<Prestamo>(sql, new { IdProd = idProducto }).ToList();
            }
        }

        /// <summary>
        /// Obtiene todos los préstamos de la base de datos.
        /// </summary>
        public List<Prestamo> ObtenerTodosLosPrestamos()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                return connection.Query<Prestamo>("SELECT * FROM prestamos").ToList();
            }
        }

        public void AgregarPrestamo(Prestamo prestamo)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO prestamos (area, persona, fecha1, fecha2, description, estado, idProducto) 
                    VALUES (@area, @persona, @fecha1, @fecha2, @description, @estado, @idProducto);";

                connection.Execute(sql, prestamo);
            }
        }

        public void BorrarPrestamo(int idPrestamo)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = "DELETE FROM prestamos WHERE Id = @Id;";
                connection.Execute(sql, new { Id = idPrestamo });
            }
        }
    }
}
