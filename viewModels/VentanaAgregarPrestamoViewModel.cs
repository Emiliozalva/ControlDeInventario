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
    public partial class VentanaAgregarPrestamoViewModel : ObservableObject
    {
        private readonly BaseDeDatos _dbService;
        public Action? SolicitudCerrar;

        // Datos del formulario
        [ObservableProperty] private int? _idProductoBusqueda;
        [ObservableProperty] private string _nombreProducto = "Ingrese ID para buscar...";
        [ObservableProperty] private string _persona = string.Empty;
        [ObservableProperty] private string _area = string.Empty;
        [ObservableProperty] private string _descripcion = string.Empty;

        // Propiedad auxiliar para saber si encontramos el producto
        private bool _productoEncontrado = false;

        public VentanaAgregarPrestamoViewModel(BaseDeDatos dbService)
        {
            _dbService = dbService;
        }

        [RelayCommand]
        private void BuscarProducto()
        {
            if (IdProductoBusqueda == null || IdProductoBusqueda <= 0) return;

            var prod = _dbService.ObtenerProductoPorId(IdProductoBusqueda.Value);
            if (prod != null)
            {
                NombreProducto = $"{prod.producto} - {prod.marca} (Stock: {prod.cantidadStock})";
                _productoEncontrado = true;
            }
            else
            {
                NombreProducto = "❌ Producto no encontrado";
                _productoEncontrado = false;
            }
        }

        [RelayCommand]
        private void GuardarPrestamo()
        {
            if (!_productoEncontrado || IdProductoBusqueda == null)
            {
                MessageBox.Show("Primero debes buscar y encontrar un producto válido.");
                return;
            }
            if (string.IsNullOrWhiteSpace(Persona) || string.IsNullOrWhiteSpace(Area))
            {
                MessageBox.Show("Debes completar el nombre de la Persona y el Área.");
                return;
            }

            try
            {
                var nuevoPrestamo = new Prestamo
                {
                    idProducto = IdProductoBusqueda.Value,
                    persona = Persona,
                    area = Area,
                    description = Descripcion,
                    fecha1 = DateTime.Now.Ticks,
                    estado = 1 // 1 = Activo
                };

                _dbService.RegistrarPrestamo(nuevoPrestamo);

                MessageBox.Show("Préstamo registrado exitosamente.");
                SolicitudCerrar?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancelar() => SolicitudCerrar?.Invoke();
    }
}
