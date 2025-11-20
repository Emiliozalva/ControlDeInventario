using Dapper; 
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
        public void RegistrarNuevoPrestamo(Prestamo nuevoPrestamo)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // Iniciamos una TRANSACCIÓN.
                // Esto asegura que si algo falla a la mitad, no se descuadre el inventario.
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Validar si hay Stock disponible
                        // (Es mejor verificarlo en la BD por si alguien más lo prestó hace un milisegundo)
                        var stockActual = connection.QueryFirstOrDefault<int>(
                            "SELECT cantidadStock FROM stock WHERE IDproducto = @Id",
                            new { Id = nuevoPrestamo.idProducto },
                            transaction);

                        if (stockActual <= 0)
                        {
                            throw new Exception("No hay stock disponible para realizar el préstamo.");
                        }

                        // 2. Insertar el registro del préstamo
                        string sqlPrestamo = @"
                    INSERT INTO prestamos (area, persona, fecha1, fecha2, description, estado, idProducto) 
                    VALUES (@area, @persona, @fecha1, @fecha2, @description, @estado, @idProducto);";

                        connection.Execute(sqlPrestamo, nuevoPrestamo, transaction);

                        // 3. Actualizar el Stock del Producto
                        // Restamos 1 al Stock disponible y Sumamos 1 a Prestada
                        string sqlUpdateStock = @"
                    UPDATE stock 
                    SET cantidadStock = cantidadStock - 1,
                        cantidadPrestada = cantidadPrestada + 1
                    WHERE IDproducto = @Id;";

                        connection.Execute(sqlUpdateStock, new { Id = nuevoPrestamo.idProducto }, transaction);

                        // Si todo salió bien, guardamos los cambios.
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        // Si hubo error, deshacemos todo.
                        transaction.Rollback();
                        throw; // Re-lanzamos el error para que la UI lo muestre
                    }
                }
            }
        }

        // MÉTODO PARA DEVOLVER (Entrada de inventario)
        public void RegistrarDevolucion(int idPrestamo, int idProducto, long fechaDevolucion)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Actualizar el Préstamo (Poner fecha fin y estado Devuelto)
                        string sqlPrestamo = @"
                    UPDATE prestamos 
                    SET fecha2 = @FechaFin, 
                        estado = @NuevoEstado 
                    WHERE Id = @IdPrestamo;";

                        connection.Execute(sqlPrestamo, new
                        {
                            FechaFin = fechaDevolucion,
                            NuevoEstado = (int)EstadoPrestamo.Devuelto, // 2
                            IdPrestamo = idPrestamo
                        }, transaction);

                        // 2. Actualizar el Stock
                        // Sumamos 1 al Stock disponible y Restamos 1 a Prestada
                        string sqlStock = @"
                    UPDATE stock 
                    SET cantidadStock = cantidadStock + 1,
                        cantidadPrestada = cantidadPrestada - 1
                    WHERE IDproducto = @IdProd;";

                        connection.Execute(sqlStock, new { IdProd = idProducto }, transaction);

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


private void Prestar()
        {
            try
            {
                var prestamo = new Prestamo
                {
                    area = Area,
                    persona = Persona,
                    description = Descripcion,
                    idProducto = ProductoSeleccionado.IDproducto,

                    // Fechas como Ticks (Enteros)
                    fecha1 = DateTime.Now.Ticks,
                    fecha2 = 0, // 0 significa "aún no devuelto"

                    estado = (int)EstadoPrestamo.Activo // 1
                };

                // Llamamos al método con transacción
                _dbService.RegistrarNuevoPrestamo(prestamo);

                MessageBox.Show("Préstamo registrado exitosamente.");
                CerrarVentana();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
