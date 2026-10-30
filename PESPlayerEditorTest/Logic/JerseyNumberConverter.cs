using System;
using System.Globalization;
using System.Windows.Data;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// El byte crudo de número de camiseta puede valer hasta 255, que al convertirse
    /// a 1-based (ReadNumber suma 1) se muestra como 256. Ese valor representa
    /// "sin número asignado" dentro del archivo del juego, por lo que se muestra
    /// como "-" en lugar del número crudo para no confundir al usuario.
    /// </summary>
    public class JerseyNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int number && number >= 256)
            {
                return "-";
            }

            return value?.ToString() ?? "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}