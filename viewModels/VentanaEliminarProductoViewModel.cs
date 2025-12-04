using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SistemaDeInventarioASOEM.clases;
using System;
using System.Windows;

namespace SistemaDeInventarioASOEM.viewModels
{
    public partial class VentanaEliminarProductoViewModel : ObservableObject
    {
        private readonly BaseDeDatos _dbService;
        public event Action? SolicitudCerrar;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EjecutarEliminacionCommand))]
        private string _terminoInput = string.Empty;

        public VentanaEliminarProductoViewModel(BaseDeDatos dbService)
        {
            _dbService = dbService;
        }

        private bool CanEliminar()
        {
            return !string.IsNullOrWhiteSpace(TerminoInput);
        }

        [RelayCommand(CanExecute = nameof(CanEliminar))]
        private void EjecutarEliminacion()
        {
            if (MessageBox.Show($"¿Estás seguro de eliminar: '{TerminoInput}'?",
                                "Confirmar Eliminación",
                                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                bool exito = _dbService.BorrarProducto(TerminoInput!);

                if (exito)
                {
                    MessageBox.Show("Producto eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    SolicitudCerrar?.Invoke();
                }
                else
                {
                    MessageBox.Show("No se encontró ningún producto con ese Nombre o ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void Cancelar()
        {
            SolicitudCerrar?.Invoke();
        }
    }
}