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


namespace SistemaDeInventarioASOEM.viewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // 1. Servicio de Base de Datos
        private readonly BaseDeDatos _dbService;

        // 2. Lista de Productos (Enlazada al DataGrid)
        [ObservableProperty]
        private ObservableCollection<Producto> _productos;

        // 3. Producto Seleccionado (Para borrar o editar)
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(BorrarProductoCommand))]
        private Producto? _productoSeleccionado;

        public MainViewModel()
        {
            _dbService = new BaseDeDatos();
            Productos = new ObservableCollection<Producto>();

            // Cargar datos al iniciar
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

        // --- COMANDOS ---

        [RelayCommand]
        private void AbrirVentanaAgregar()
        {
            // Lógica para abrir la ventana que ya tienes en la carpeta 'windows'

            // 1. Instanciamos el VM de la ventana hija
            var vmHijo = new VentanaAgregarProductoViewModel(_dbService);

            // 2. Instanciamos la Vista
            var ventana = new VentanaAgregarProducto();

            // 3. Conectamos
            ventana.DataContext = vmHijo;

            // 4. Mostramos
            ventana.ShowDialog();

            // 5. Al volver, recargamos la lista para ver el cambio
            CargarProductos();
        }

        [RelayCommand]
        private void BorrarProducto()
        {
            if (ProductoSeleccionado != null)
            {
                _dbService.BorrarProducto(ProductoSeleccionado.IDproducto);
                CargarProductos();
            }
        }
    }
}
