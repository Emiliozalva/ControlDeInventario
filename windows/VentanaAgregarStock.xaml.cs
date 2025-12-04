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
    public partial class VentanaAgregarStock : Window
    {
        public VentanaAgregarStock()
        {
            InitializeComponent();

            this.DataContextChanged += (sender, e) =>
            {
                if (e.NewValue is VentanaAgregarStockViewModel vm)
                {
                    vm.SolicitudCerrar += () => this.Close();
                }
            };
        }
    }
}
