using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClosedXML.Excel;
using SistemaDeInventarioASOEM.clases;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace SistemaDeInventarioASOEM.viewModels
{
    public partial class VentanaGenerarOrdenViewModel : ObservableObject
    {
        private readonly BaseDeDatos _dbService;
        public Action? SolicitudCerrar;

     
        [ObservableProperty] private ObservableCollection<Producto> _productosInventario;
        [ObservableProperty] private ObservableCollection<ItemOrden> _detalleOrden;

     
        [ObservableProperty] private Producto? _productoSeleccionado;
        [ObservableProperty] private string _nombreProductoManual = string.Empty;
        [ObservableProperty] private int _cantidadInput = 1;

        [ObservableProperty] private ItemOrden? _itemSeleccionadoParaBorrar;

        public VentanaGenerarOrdenViewModel(BaseDeDatos dbService)
        {
            _dbService = dbService;
            _productosInventario = new ObservableCollection<Producto>(_dbService.ObtenerTodosLosProductos());
            _detalleOrden = new ObservableCollection<ItemOrden>();
        }

        [RelayCommand]
        private void AgregarDesdeInventario()
        {
            if (_detalleOrden.Count >= 10)
            {
                MessageBox.Show("La planilla solo permite un máximo de 10 ítems.");
                return;
            }
            if (ProductoSeleccionado == null)
            {
                MessageBox.Show("Selecciona un producto de la lista.");
                return;
            }
            if (CantidadInput <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a 0.");
                return;
            }

            _detalleOrden.Add(new ItemOrden
            {
                Nombre = ProductoSeleccionado.DescripcionCompleta,
                Cantidad = CantidadInput
            });
            CantidadInput = 1;
            ProductoSeleccionado = null;
        }

        [RelayCommand]
        private void AgregarManual()
        {
            if (_detalleOrden.Count >= 10)
            {
                MessageBox.Show("La planilla solo permite un máximo de 10 ítems.");
                return;
            }
            if (string.IsNullOrWhiteSpace(NombreProductoManual))
            {
                MessageBox.Show("Escribe el nombre del producto.");
                return;
            }
            if (CantidadInput <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a 0.");
                return;
            }

            _detalleOrden.Add(new ItemOrden
            {
                Nombre = NombreProductoManual,
                Cantidad = CantidadInput
            });

            NombreProductoManual = string.Empty;
            CantidadInput = 1;
        }

        [RelayCommand]
        private void EliminarItem()
        {
            if (ItemSeleccionadoParaBorrar != null)
            {
                _detalleOrden.Remove(ItemSeleccionadoParaBorrar);
            }
        }

        [RelayCommand]
        private void GenerarExcel()
        {
            if (_detalleOrden.Count == 0)
            {
                MessageBox.Show("La lista está vacía. Agrega productos primero.");
                return;
            }

            try
            {
                string carpetaBase = AppDomain.CurrentDomain.BaseDirectory;
                string rutaPlantilla = Path.Combine(carpetaBase, "ncs", "PlanillaDeElementos.xlsx");

                if (!File.Exists(rutaPlantilla))
                {
                    MessageBox.Show("No se encontró la plantilla en: " + rutaPlantilla);
                    return;
                }

                using (var workbook = new XLWorkbook(rutaPlantilla))
                {
                    var hoja = workbook.Worksheet(1);

                    hoja.Range("B9:B18").Clear(XLClearOptions.Contents);
                    hoja.Range("F9:F18").Clear(XLClearOptions.Contents);

                    int filaInicial = 9;

                    for (int i = 0; i < _detalleOrden.Count; i++)
                    {
                        var item = _detalleOrden[i];
                        int filaActual = filaInicial + i;

                        hoja.Cell(filaActual, 2).Value = item.Nombre;   
                        hoja.Cell(filaActual, 6).Value = item.Cantidad; 
                    }

                    hoja.Cell("D2").Value = DateTime.Now.ToString("dd/MM/yyyy");

                    string nombreArchivo = $"Orden_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    string rutaEscritorio = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string rutaFinal = Path.Combine(rutaEscritorio, nombreArchivo);

                    workbook.SaveAs(rutaFinal);

                    try
                    {
                        var p = new System.Diagnostics.Process();
                        p.StartInfo = new System.Diagnostics.ProcessStartInfo(rutaFinal)
                        {
                            UseShellExecute = true 
                        };
                        p.Start();
                    }
                    catch
                    {
                        MessageBox.Show("El archivo se generó pero no se pudo abrir automáticamente.", "Aviso");
                    }
                    SolicitudCerrar?.Invoke();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        [RelayCommand]
        private void Cancelar()
        {
            SolicitudCerrar?.Invoke();
        }
    }
}