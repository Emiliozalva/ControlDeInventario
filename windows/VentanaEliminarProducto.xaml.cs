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

using SistemaDeInventarioASOEM.viewModels;

namespace SistemaDeInventarioASOEM.windows
{
    public partial class VentanaEliminarProducto : Window
    {
        public VentanaEliminarProducto()
        {
            InitializeComponent();
            this.DataContextChanged += (sender, args) =>
            {
                if (args.NewValue is VentanaEliminarProductoViewModel vm)
                {
                    vm.SolicitudCerrar += () => this.Close();
                }
            };
            this.Loaded += (sender, args) =>
            {
                if (TxtInput != null)
                {
                    TxtInput.Focus();
                }
            };
        }
    }
}
