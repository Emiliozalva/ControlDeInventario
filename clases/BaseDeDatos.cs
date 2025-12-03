using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SistemaDeInventarioASOEM.clases
{
    public class BaseDeDatos
    {
        private readonly string _connectionString;

        public BaseDeDatos()
        {
            string nombreArchivoDB = "BaseProductos.db";

            // Rutas
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolderPath = Path.Combine(appDataPath, "SistemaDeInventarioASOEM");
            Directory.CreateDirectory(appFolderPath);
            string rutaDestinoDB = Path.Combine(appFolderPath, nombreArchivoDB);
            string rutaOrigenDB = Path.Combine(AppContext.BaseDirectory, nombreArchivoDB);

            // Copiar si no existe
            if (!File.Exists(rutaDestinoDB))
            {
                if (File.Exists(rutaOrigenDB))
                {
                    File.Copy(rutaOrigenDB, rutaDestinoDB);
                }
            }

            _connectionString = $"Data Source={rutaDestinoDB}";
            InicializarEstructuraDB();
        }

        private void InicializarEstructuraDB()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // Tabla Stock
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

                // Tabla Prestamos
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

        // --- MÉTODOS DE PRODUCTOS ---

        public List<Producto> ObtenerTodosLosProductos()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                return connection.Query<Producto>("SELECT * FROM stock").ToList();
            }
        }
        public Producto? ObtenerProductoPorId(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = "SELECT * FROM stock WHERE IDproducto = @Id;";
                return connection.QueryFirstOrDefault<Producto>(sql, new { Id = id });
            }
        }
        public void AgregarProducto(Producto producto)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO stock (producto, cantidadStock, marca, modelo, cantidadPrestada) 
                    VALUES (@producto, @cantidadStock, @marca, @modelo, @cantidadPrestada);";

                connection.Execute(sql, producto);
            }
        }

        public void BorrarProducto(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Execute("DELETE FROM stock WHERE IDproducto = @Id;", new { Id = id });
            }
        }

        // --- AQUÍ TERMINA LA CLASE ---
        // ¡Asegúrate de no tener código pegado después de esta llave!
    }
}