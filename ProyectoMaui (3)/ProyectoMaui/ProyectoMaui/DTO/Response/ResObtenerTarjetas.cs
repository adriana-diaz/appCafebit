using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoMaui.DTO
{
    public class ResObtenerTarjetas
    {
        public List<consultaTarjeta> listaDeconsultas { get; set; } // Cambiado de "tarjetas" a "listaDeconsultas"
        public bool resultado { get; set; }
        public string error { get; set; }
    }
}
