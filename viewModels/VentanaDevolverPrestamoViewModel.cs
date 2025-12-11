using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SistemaDeInventarioASOEM.clases;
using System;
using System.Windows;

namespace SistemaDeInventarioASOEM.viewModels
{
    public partial class VentanaDevolverPrestamoViewModel : ObservableObject
    {
        private readonly BaseDeDatos _dbService;
        public Action? SolicitudCerrar;

        [ObservableProperty]
        private int? _idPrestamoInput; // Aquí guardamos el ID que escribe el usuario

        public VentanaDevolverPrestamoViewModel(BaseDeDatos dbService)
        {
            _dbService = dbService;
        }

        [RelayCommand]
        private void ConfirmarDevolucion()
        {
            if (IdPrestamoInput == null || IdPrestamoInput <= 0)
            {
                MessageBox.Show("Por favor ingresa un ID válido.");
                return;
            }

            try
            {
                _dbService.DevolverPrestamo(IdPrestamoInput.Value);

                MessageBox.Show("Devolución registrada correctamente. Stock actualizado.");
                SolicitudCerrar?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo devolver: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancelar()
        {
            SolicitudCerrar?.Invoke();
        }
    }
}