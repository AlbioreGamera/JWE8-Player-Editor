using System;
using System.Globalization;
using System.Windows.Data;
using PlayerLibrary;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Traduce el FaceType del jugador a la etiqueta mostrada en el panel de
    /// detalle de la lista de jugadores: las caras PRESET (preestablecidas,
    /// vinculadas a un FaceId real mediante Relink) se muestran como
    /// "Special"; el resto (BUILD/ORIGINAL, caras generadas/editadas
    /// manualmente) se muestran como "Default".
    /// </summary>
    public class FaceTypeToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is FaceType faceType && faceType == FaceType.PRESET ? "Special" : "Default";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}