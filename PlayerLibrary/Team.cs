using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerLibrary
{
    public class Team
    {
        public int TeamId { get; set; }
        public string? TeamCode { get; set; }
        public string? TeamName { get; set; }
        public string? TeamNameRomaji { get; set; }

        // Offset y longitud reales (en bytes) de cada campo dentro del archivo.
        // El espacio disponible varía por registro, por lo que no se puede asumir un tamaño fijo:
        // cualquier edición debe truncarse/rellenarse dentro de estos límites para no desalinear
        // los registros siguientes.
        public long NameOffset { get; set; }
        public int NameFieldLength { get; set; }
        public long RomajiOffset { get; set; }
        public int RomajiFieldLength { get; set; }
        public long CodeOffset { get; set; }
        public int CodeFieldLength { get; set; }
    }
}
