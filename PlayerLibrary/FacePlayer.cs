using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerLibrary
{
    /// <summary>
    /// Representa al jugador real asociado a un código de cara preestablecida
    /// (ej. W001, Y034, R007, K012), usado para mostrar a quién pertenece
    /// la cara seleccionada mediante FaceId + SkinColour.
    /// </summary>
    public class FacePlayer
    {
        public string? FaceCode { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Country { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();

        public override string ToString()
        {
            return string.IsNullOrEmpty(Country) ? FullName : $"{FullName} ({Country})";
        }
    }
}