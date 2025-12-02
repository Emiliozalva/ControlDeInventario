using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SistemaDeInventarioASOEM.clases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SistemaDeInventarioASOEM.viewModels
{
    public partial class VentanaAgregarProductoViewModel : ObservableObject
    {
        private readonly BaseDeDatos _dbService;

        // Evento para avisar a la ventana que se cierre
        public event Action? SolicitudCerrar;

        // --- Propiedades enlazadas al XAML ---

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GuardarCommand))] 
        private string? _productoNombre;

        [ObservableProperty]
        private string? _marca;

        [ObservableProperty]
        private string? _modelo;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GuardarCommand))]
        private int _cantidadStock;

        // --- Constructor ---
        public VentanaAgregarProductoViewModel(BaseDeDatos dbService)
        {
            _dbService = dbService;
        }

        // --- Validaciones ---
        private bool CanGuardar()
        {
            // El botón solo se habilita si hay nombre y el stock no es negativo
            return !string.IsNullOrWhiteSpace(ProductoNombre) && CantidadStock >= 0;
        }

        // --- Comandos ---

        [RelayCommand(CanExecute = nameof(CanGuardar))]
        private void Guardar()
        {
            try
            {
                // 1. Crear el objeto Producto (Modelo)
                var nuevoProducto = new Producto
                {
                    // ID se genera solo en la BD
                    producto = ProductoNombre,
                    marca = Marca,
                    modelo = Modelo,
                    cantidadStock = CantidadStock,
                    cantidadPrestada = 0
                };

                // 2. Guardar en la BD
                _dbService.AgregarProducto(nuevoProducto);

                // 3. Mostrar éxito y cerrar
                MessageBox.Show("Producto agregado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                SolicitudCerrar?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar en la base de datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancelar()
        {
            // Simplemente cerramos la ventana
            SolicitudCerrar?.Invoke();
        }
    }
}