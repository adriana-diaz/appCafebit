using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoMaui.DTO
{
    public class ResObtenerTiendas
    {
        public bool Resultado { get; set; }
        public List<Tiendas> ListaDeConsultas { get; set; }
        public string Error { get; set; }
    }
}
