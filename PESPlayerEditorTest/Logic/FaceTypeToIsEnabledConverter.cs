using System.Globalization;
using System.Windows.Data;
using PlayerLibrary;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Convierte el FaceType del jugador en un bool IsEnabled para el campo Face Id.
    /// Cuando FaceType es BUILD, el Face Id se bloquea (no editable) porque la cara
    /// se genera a partir de los parámetros de Physique/Head en lugar de un Id preestablecido.
    /// </summary>
    public class FaceTypeToIsEnabledConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FaceType faceType)
            {
                return faceType != FaceType.BUILD;
            }

            return true;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotSupportedException();
        }
    }
}
