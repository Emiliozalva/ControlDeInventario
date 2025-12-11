using System.Windows;
using SistemaDeInventarioASOEM.viewModels;

namespace SistemaDeInventarioASOEM.windows
{
    public partial class VentanaGenerarOrden : Window
    {
        public VentanaGenerarOrden()
        {
            InitializeComponent();
            this.DataContextChanged += (s, e) =>
            {
                if (e.NewValue is VentanaGenerarOrdenViewModel vm)
                {
                    vm.SolicitudCerrar += () => this.Close();
                }
            };
        }
    }
}