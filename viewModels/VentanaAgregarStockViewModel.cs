using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SistemaDeInventarioASOEM.clases;
using System;
using System.Windows;

namespace SistemaDeInventarioASOEM.viewModels
{
    public partial class VentanaAgregarStockViewModel : ObservableObject
    {
        private readonly BaseDeDatos _dbService;
        public event Action? SolicitudCerrar;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ActualizarStockCommand))]
        private int? _idProductoInput;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ActualizarStockCommand))]
        private int? _cantidadInput; 

        public VentanaAgregarStockViewModel(BaseDeDatos dbService)
        {
            _dbService = dbService;
        }

        private bool CanActualizar()
        {
            return IdProductoInput > 0 && CantidadInput != 0 && CantidadInput != null;
        }

        [RelayCommand(CanExecute = nameof(CanActualizar))]
        private void ActualizarStock()
        {
            try
            {
                var producto = _dbService.ObtenerProductoPorId(IdProductoInput ?? 0);

                if (producto == null)
                {
                    MessageBox.Show("No existe un producto con ese ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                int stockActual = producto.cantidadStock;
                int cambio = CantidadInput ?? 0;
                int nuevoStockTotal = stockActual + cambio;
                if (nuevoStockTotal < 0)
                {
                    nuevoStockTotal = 0;
                    MessageBox.Show($"El stock no puede ser negativo. Se ajustó a 0 (Faltaron {Math.Abs(stockActual + cambio)} unidades).",
                                    "Aviso de Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                _dbService.ActualizarStock(producto.IDproducto, nuevoStockTotal);

                string accion = cambio > 0 ? "agregado" : "descontado";
                MessageBox.Show($"Stock actualizado correctamente.\nAnterior: {stockActual}\nNuevo: {nuevoStockTotal}",
                                "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                SolicitudCerrar?.Invoke();
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