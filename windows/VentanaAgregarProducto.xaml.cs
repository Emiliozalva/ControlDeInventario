using SistemaDeInventarioASOEM.viewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SistemaDeInventarioASOEM.windows
{
    /// <summary>
    /// Lógica de interacción para VentanaAgregarProducto.xaml
    /// </summary>


namespace SistemaDeInventarioASOEM.windows
{
    public partial class VentanaAgregarProducto : Window
    {
        public VentanaAgregarProducto()
        {
            InitializeComponent();

            // Nos suscribimos al evento cuando el DataContext cambie (cuando se asigne el ViewModel)
            this.DataContextChanged += (sender, e) =>
            {
                if (e.NewValue is VentanaAgregarProductoViewModel vm)
                {
                    // Cuando el ViewModel diga "SolicitudCerrar", cerramos esta ventana
                    vm.SolicitudCerrar += () => this.Close();
                }
            };
        }
    }
}