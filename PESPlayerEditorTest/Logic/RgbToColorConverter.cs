using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Combina los tres valores Hair Colour R/G/B (0-252 cada uno, codificados en
    /// el formato binario del juego) en un SolidColorBrush para previsualizar el
    /// color de cabello resultante, donde R=rojo, G=verde y B=azul.
    /// </summary>
    public class RgbToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3 ||
                !int.TryParse(values[0]?.ToString(), out int r) ||
                !int.TryParse(values[1]?.ToString(), out int g) ||
                !int.TryParse(values[2]?.ToString(), out int b))
            {
                return Brushes.Transparent;
            }

            byte clampedR = (byte)Math.Clamp(r, 0, 255);
            byte clampedG = (byte)Math.Clamp(g, 0, 255);
            byte clampedB = (byte)Math.Clamp(b, 0, 255);

            return new SolidColorBrush(Color.FromRgb(clampedR, clampedG, clampedB));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}