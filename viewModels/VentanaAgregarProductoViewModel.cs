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
        public event Action? SolicitudCerrar;

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

        public VentanaAgregarProductoViewModel(BaseDeDatos dbService)
        {
            _dbService = dbService;
        }

        private bool CanGuardar()
        {
            return !string.IsNullOrWhiteSpace(ProductoNombre) && CantidadStock >= 0;
        }


        [RelayCommand(CanExecute = nameof(CanGuardar))]
        private void Guardar()
        {
            try
            {
                var nuevoProducto = new Producto
                {
                    producto = ProductoNombre,
                    marca = Marca,
                    modelo = Modelo,
                    cantidadStock = CantidadStock,
                    cantidadPrestada = 0
                };

                _dbService.AgregarProducto(nuevoProducto);
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
            SolicitudCerrar?.Invoke();
        }
    }
}