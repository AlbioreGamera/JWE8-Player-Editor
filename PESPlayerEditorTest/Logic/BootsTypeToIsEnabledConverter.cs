using System.Globalization;
using System.Windows.Data;
using PlayerLibrary;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Convierte el BootsType del jugador en un bool IsEnabled para los demás campos de Boots
    /// (colores, etc.). A partir del tipo de bota "3" (índice 2, primera bota con licencia:
    /// BOOT_ADIDAS_F50_BLACK_WHITE) en adelante, las botas son licenciadas y modeladas por el
    /// juego, por lo que sus colores no pueden ser editados.
    /// </summary>
    public class BootsTypeToIsEnabledConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BootsType bootsType)
            {
                return (int)bootsType < 2;
            }

            return true;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotSupportedException();
        }
    }
}
