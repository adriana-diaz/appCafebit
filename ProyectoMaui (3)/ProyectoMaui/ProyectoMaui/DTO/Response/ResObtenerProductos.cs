using ProyectoMaui.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoMaui.DTO
{
    public class ResObtenerProductos
    {
        public bool Resultado { get; set; }
        public List<Producto> ListaDePublicaciones { get; set; }
        public string Error { get; set; }
    }
}
