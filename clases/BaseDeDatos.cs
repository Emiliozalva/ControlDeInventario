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
                connection.Open();
                string sqlFind = "SELECT IDproducto FROM stock WHERE IDproducto = @val OR producto = @val COLLATE NOCASE LIMIT 1";

                int.TryParse(terminoBusqueda, out int idPosible);
                int? idEncontrado = connection.QueryFirstOrDefault<int?>(sqlFind, new { val = (idPosible > 0 ? idPosible.ToString() : terminoBusqueda) });

                if (idEncontrado != null)
                {
                    BorrarProducto(idEncontrado.Value);
                    return true; 
                }

                return false; 
            }
        }
        public void BorrarProducto(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string sqlCheck = "SELECT COUNT(*) FROM prestamos WHERE idProducto = @id";
                int cantidadPrestamos = connection.ExecuteScalar<int>(sqlCheck, new { id });

                if (cantidadPrestamos > 0)
                {
                    throw new Exception($"No se puede eliminar el producto.\n\nEste producto aparece en {cantidadPrestamos} registro(s) de préstamos (activos o devueltos).\nPara mantener el historial, no se permite su eliminación.");
                }
                connection.Execute("DELETE FROM stock WHERE IDproducto = @Id;", new { Id = id });
            }
        }
        

        public List<Prestamo> ObtenerTodosLosPrestamos()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
               
                string sql = @"
            SELECT 
                p.*, 
                s.producto, 
                s.marca, 
                s.modelo
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
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string sqlCheck = "SELECT cantidadStock FROM stock WHERE IDproducto = @id";
                        int stockActual = connection.ExecuteScalar<int>(sqlCheck, new { id = prestamo.idProducto }, transaction);

                        if (stockActual <= 0)
                        {
                            throw new Exception("No hay stock suficiente para realizar este préstamo.");
                        }

                        string sqlInsert = @"
                            INSERT INTO prestamos (area, persona, fecha1, description, estado, idProducto) 
                            VALUES (@area, @persona, @fecha1, @description, 1, @idProducto);";

                        connection.Execute(sqlInsert, prestamo, transaction);

                        string sqlUpdateStock = @"
                            UPDATE stock 
                            SET cantidadStock = cantidadStock - 1, 
                                cantidadPrestada = cantidadPrestada + 1 
                            WHERE IDproducto = @idProducto";

                        connection.Execute(sqlUpdateStock, new { idProducto = prestamo.idProducto }, transaction);

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw; 
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
                        var prestamoDB = connection.QueryFirstOrDefault<Prestamo>(
                            "SELECT * FROM prestamos WHERE Id = @id", new { id = idPrestamo }, transaction);

                        if (prestamoDB == null) throw new Exception("El préstamo no existe.");
                        if (prestamoDB.estado == 0) throw new Exception("Este préstamo ya fue devuelto.");

                        long fechaDevolucion = DateTime.Now.Ticks;
                        string sqlUpdatePrestamo = @"
                            UPDATE prestamos 
                            SET estado = 0, 
                                fecha2 = @fechaFin 
                            WHERE Id = @id";

                        connection.Execute(sqlUpdatePrestamo, new { fechaFin = fechaDevolucion, id = idPrestamo }, transaction);

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