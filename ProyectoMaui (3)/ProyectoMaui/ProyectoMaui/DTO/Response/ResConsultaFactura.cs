using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoMaui.DTO
{
    public class ResConsultaFactura
    {
        public List<Consultar> listaDefacturas { get; set; } 
        public bool resultado { get; set; }
        public string error { get; set; }
    }
}
