using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProyectoMaui.DTO;

namespace ProyectoMaui.DTO
{
    public class ResObtenerProductosCarrito
    {
        public bool Resultado { get; set; }
        public List<consulta> listaDeconsultas { get; set; }
        public string Error { get; set; }
    }
}
