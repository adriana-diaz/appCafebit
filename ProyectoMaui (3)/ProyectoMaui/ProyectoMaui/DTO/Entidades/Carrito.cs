using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoMaui.DTO
{
    public class Carrito
    {
        // Propiedades y métodos de la clase Carrito (si los hay)
    }

    public class AgregarC
    {
        public int id_usuario { get; set; }
        public string nombre_producto { get; set; }
        public int cantidad { get; set; }
        public String tamanho { get; set; }
        public string nombre_tienda { get; set; }
    }
    public class consulta
    {
        public int? id_usuario { get; set; }

        public int? id_producto { get; set; }
        public string nombre_producto { get; set; }
        public string descripcion { get; set; }
        public decimal precio_producto { get; set; }
        public DateTime fecha_agregado { get; set; }
        public int cantidad { get; set; }
        public int? id_carrito { get; set; }
        public string tamanho { get; set; }
        public string nombre_tienda { get; set; }
    }
    public class EliminarC
    {
        public int id_usuario { get; set; }
        public int id_carrito { get; set; }
    }
}
