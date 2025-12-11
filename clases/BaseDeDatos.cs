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

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolderPath = Path.Combine(appDataPath, "SistemaDeInventarioASOEM");
            Directory.CreateDirectory(appFolderPath);
            string rutaDestinoDB = Path.Combine(appFolderPath, nombreArchivoDB);
            string rutaOrigenDB = Path.Combine(AppContext.BaseDirectory, nombreArchivoDB);

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
        public void ActualizarStock(Producto producto)
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
        public void ActualizarStock(int id, int nuevaCantidad)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = "UPDATE stock SET cantidadStock = @cantidadStock WHERE IDproducto = @Id;";
                connection.Execute(sql, new { cantidadStock = nuevaCantidad, Id = id });
            }
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

                connection.Execute(@"
        CREATE TABLE IF NOT EXISTS prestamos (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,  -- <--- ESTO ES LO QUE FALTABA
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
        public bool BorrarProducto(string terminoBusqueda)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                // Intentamos ver si el término es un número (ID) o texto (Nombre)
                bool esId = int.TryParse(terminoBusqueda, out int idPosible);

                string sql;
                int filasAfectadas = 0;

                if (esId)
                {
                    // Si es número, intentamos borrar por ID
                    sql = "DELETE FROM stock WHERE IDproducto = @id";
                    filasAfectadas = connection.Execute(sql, new { id = idPosible });
                }

                // Si no se borró nada por ID (o no era número), intentamos borrar por Nombre
                if (filasAfectadas == 0)
                {
                    // COLLATE NOCASE hace que no importe mayúsculas/minúsculas
                    sql = "DELETE FROM stock WHERE producto = @nombre COLLATE NOCASE";
                    filasAfectadas = connection.Execute(sql, new { nombre = terminoBusqueda });
                }

                return filasAfectadas > 0; // Retorna true si borró algo
            }
        }
        public void BorrarProducto(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Execute("DELETE FROM stock WHERE IDproducto = @Id;", new { Id = id });
            }
        }
        // --- MÉTODOS DE PRÉSTAMOS (Nuevos) ---

        public List<Prestamo> ObtenerTodosLosPrestamos()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = @"
            SELECT 
                p.*, 
                s.producto AS NombreProducto 
            FROM prestamos p
            INNER JOIN stock s ON p.idProducto = s.IDproducto
            ORDER BY p.Id DESC";

                return connection.Query<Prestamo>(sql).ToList();
            }
        }

        public void RegistrarPrestamo(Prestamo prestamo)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // Iniciamos una TRANSACCIÓN: O se hace todo (insertar + descontar) o no se hace nada.
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Verificar si el producto existe y tiene stock
                        string sqlCheck = "SELECT cantidadStock FROM stock WHERE IDproducto = @id";
                        int stockActual = connection.ExecuteScalar<int>(sqlCheck, new { id = prestamo.idProducto }, transaction);

                        if (stockActual <= 0)
                        {
                            throw new Exception("No hay stock suficiente para realizar este préstamo.");
                        }

                        // 2. Insertar el préstamo
                        // Nota: Guardamos fecha1 como Ticks (long) porque tu DB lo pide INTEGER
                        string sqlInsert = @"
                            INSERT INTO prestamos (area, persona, fecha1, description, estado, idProducto) 
                            VALUES (@area, @persona, @fecha1, @description, 1, @idProducto);";

                        // Estado 1 = Activo
                        connection.Execute(sqlInsert, prestamo, transaction);

                        // 3. Actualizar el Stock (Resta 1 al stock disponible, Suma 1 a prestados)
                        string sqlUpdateStock = @"
                            UPDATE stock 
                            SET cantidadStock = cantidadStock - 1, 
                                cantidadPrestada = cantidadPrestada + 1 
                            WHERE IDproducto = @idProducto";

                        connection.Execute(sqlUpdateStock, new { idProducto = prestamo.idProducto }, transaction);

                        // Si todo salió bien, confirmamos los cambios
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        // Si algo falló, deshacemos todo
                        transaction.Rollback();
                        throw; // Re-lanzamos el error para que lo vea la ventana
                    }
                }
            }
        }

        public void DevolverPrestamo(int idPrestamo)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Obtener el préstamo para saber qué producto es y si ya se devolvió
                        var prestamoDB = connection.QueryFirstOrDefault<Prestamo>(
                            "SELECT * FROM prestamos WHERE Id = @id", new { id = idPrestamo }, transaction);

                        if (prestamoDB == null) throw new Exception("El préstamo no existe.");
                        if (prestamoDB.estado == 0) throw new Exception("Este préstamo ya fue devuelto.");

                        // 2. Marcar préstamo como devuelto (Estado 0) y poner fecha de fin
                        long fechaDevolucion = DateTime.Now.Ticks;
                        string sqlUpdatePrestamo = @"
                            UPDATE prestamos 
                            SET estado = 0, 
                                fecha2 = @fechaFin 
                            WHERE Id = @id";

                        connection.Execute(sqlUpdatePrestamo, new { fechaFin = fechaDevolucion, id = idPrestamo }, transaction);

                        // 3. Devolver el stock (Suma 1 al stock disponible, Resta 1 a prestados)
                        // Usamos MAX(0, ...) por seguridad para que 'cantidadPrestada' nunca sea negativo
                        string sqlUpdateStock = @"
                            UPDATE stock 
                            SET cantidadStock = cantidadStock + 1, 
                                cantidadPrestada = MAX(0, cantidadPrestada - 1)
                            WHERE IDproducto = @idProd";

                        connection.Execute(sqlUpdateStock, new { idProd = prestamoDB.idProducto }, transaction);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

    }
}