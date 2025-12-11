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
            var vmAgregar = new VentanaAgregarPrestamoViewModel(_dbService);
            var ventana = new VentanaAgregarPrestamo();
            ventana.DataContext = vmAgregar;
            ventana.ShowDialog();
            CargarPrestamos();
        }

        [RelayCommand]
        private void DevolverPrestamo()
        {
            var vmDevolver = new VentanaDevolverPrestamoViewModel(_dbService);
            var ventana = new VentanaDevolverPrestamo();
            ventana.DataContext = vmDevolver;
            ventana.ShowDialog();
            CargarPrestamos();
        }
    }
}