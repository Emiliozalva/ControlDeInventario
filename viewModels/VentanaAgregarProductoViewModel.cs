using CommunityToolkit.Mvvm.ComponentModel;
using SistemaDeInventarioASOEM.clases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaDeInventarioASOEM.viewModels
{
    public partial class VentanaAgregarProductoViewModel : ObservableObject
    {
        private readonly BaseDeDatos _dbService;

        public VentanaAgregarProductoViewModel(BaseDeDatos dbService)
        {
            _dbService = dbService;
        }

        // ... resto de tu código
    }
}
