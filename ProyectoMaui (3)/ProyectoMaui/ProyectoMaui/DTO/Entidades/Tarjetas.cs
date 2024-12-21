using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoMaui.DTO
{
    public class InsertarTarjetas
    {
        public string numero_trajeta { get; set; }
        public string fecha_expiracion { get; set; }
        public string CVV { get; set; }
        public int? id_usuario { get; set; }

    }
    public class consultaTarjeta
    {
        public int? id_usuario { get; set; }
        public int? id_tarjeta { get; set; }
        public string numero_trajeta { get; set; }
        public DateTime fecha_expiracion { get; set; }
        public string CVV { get; set; }
    }
}
