using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using SistemaDeInventarioASOEM.clases;
using SistemaDeInventarioASOEM.windows;
using System.Windows; 

namespace SistemaDeInventarioASOEM.viewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly BaseDeDatos _dbService;

        [ObservableProperty]
        private ObservableCollection<Producto> _productos;

        [ObservableProperty]
        private Producto? _productoSeleccionado;

     
        public System.Action? SolicitudCerrarVentana;
   


        public MainViewModel()
        {
            _dbService = new BaseDeDatos();
            Productos = new ObservableCollection<Producto>();
            CargarProductos();
        }

        private void CargarProductos()
        {
            Productos.Clear();
            var lista = _dbService.ObtenerTodosLosProductos();

            foreach (var p in lista)
            {
                Productos.Add(p);
            }
        }
        [RelayCommand]
        private void AbrirAdministrarPrestamos()
        {
            var ventanaPrestamos = new VentanaAdministrarPrestamo();

            var vmPrestamos = new VentanaAdministrarPrestamoViewModel();
            ventanaPrestamos.DataContext = vmPrestamos; 

            ventanaPrestamos.Show();

            SolicitudCerrarVentana?.Invoke();
        }
        [RelayCommand]
        private void AbrirVentanaAgregar()
        {
            var vmHijo = new VentanaAgregarProductoViewModel(_dbService);
            var ventana = new VentanaAgregarProducto();
            ventana.DataContext = vmHijo;
            ventana.ShowDialog();
            CargarProductos();
        }

        [RelayCommand]
        private void AbrirVentanaAgregarStock()
        {
            var vmAgregarStock = new VentanaAgregarStockViewModel(_dbService);
            var ventana = new VentanaAgregarStock();
            ventana.DataContext = vmAgregarStock;
            ventana.ShowDialog();
            CargarProductos();
        }

        [RelayCommand]
        private void BorrarProducto()
        {
            try
            {
                var vmEliminar = new VentanaEliminarProductoViewModel(_dbService);
                var ventana = new VentanaEliminarProducto();
                ventana.DataContext = vmEliminar;
                ventana.ShowDialog();
                CargarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir la ventana de eliminación: {ex.Message}");
            }
        }
    }
}