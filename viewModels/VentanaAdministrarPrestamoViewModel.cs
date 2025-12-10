using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SistemaDeInventarioASOEM.clases;
using SistemaDeInventarioASOEM.windows;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace SistemaDeInventarioASOEM.viewModels
{
    public partial class VentanaAdministrarPrestamoViewModel : ObservableObject
    {
        private readonly BaseDeDatos _dbService;
        public Action? SolicitudCerrar;

        // --- PROPIEDADES EXPLÍCITAS (Para evitar errores de compilación) ---

        private ObservableCollection<Prestamo> _prestamos;
        public ObservableCollection<Prestamo> Prestamos
        {
            get => _prestamos;
            set => SetProperty(ref _prestamos, value);
        }

        private Prestamo? _prestamoSeleccionado;
        public Prestamo? PrestamoSeleccionado
        {
            get => _prestamoSeleccionado;
            set => SetProperty(ref _prestamoSeleccionado, value);
        }

        // --- CONSTRUCTOR ---
        public VentanaAdministrarPrestamoViewModel()
        {
            _dbService = new BaseDeDatos();
            
            _prestamos = new ObservableCollection<Prestamo>();
            CargarPrestamos();
        }

        private void CargarPrestamos()
        {
            Prestamos.Clear();
            var lista = _dbService.ObtenerTodosLosPrestamos();
            foreach (var p in lista)
            {
                Prestamos.Add(p);
            }
        }

        // --- COMANDOS ---

        [RelayCommand]
        private void Volver()
        {
            MainWindow main = new MainWindow();
            main.Show();
            SolicitudCerrar?.Invoke();
        }

        [RelayCommand]
        private void AgregarPrestamo()
        {
            // 1. Instanciar VM y Ventana
            var vmAgregar = new VentanaAgregarPrestamoViewModel(_dbService);
            var ventana = new VentanaAgregarPrestamo();
            ventana.DataContext = vmAgregar;

            // 2. Mostrar como diálogo (bloquea la de atrás hasta que termines)
            ventana.ShowDialog();

            // 3. Al volver, recargamos la lista para ver el nuevo préstamo
            CargarPrestamos();
        }

        [RelayCommand]
        private void EliminarPrestamo()
        {
            if (PrestamoSeleccionado == null)
            {
                MessageBox.Show("Selecciona un préstamo de la lista para devolver.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar si ya está devuelto (Tu lógica de negocio: una vez devuelto no se toca más)
            if (PrestamoSeleccionado.estado == 0)
            {
                MessageBox.Show("Este préstamo ya fue devuelto y cerrado. No se puede modificar.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var mensaje = $"¿Confirmas la devolución del préstamo de {PrestamoSeleccionado.persona}?\n\n" +
                          "• Se marcará como 'Devuelto'.\n" +
                          "• Se registrará la fecha de hoy.\n" +
                          "• El stock del producto aumentará en 1.";

            if (MessageBox.Show(mensaje, "Confirmar Devolución", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    _dbService.DevolverPrestamo(PrestamoSeleccionado.Id);

                    MessageBox.Show("Devolución registrada correctamente.");
                    CargarPrestamos(); // Refrescar la tabla para ver el cambio de estado
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al procesar la devolución: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}