using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Convierte un stat numérico (0-99) en un color de fondo:
    /// - Menos de 80: sin color (transparente) 
    /// - 80 a 89: amarillo
    /// - 90 a 94: naranjo
    /// - 95 o más: rojo
    /// </summary>
    public class StatValueToBackgroundConverter : IValueConverter
    {
        private static readonly SolidColorBrush Yellow = new SolidColorBrush(Color.FromRgb(255, 235, 59));
        private static readonly SolidColorBrush Orange = new SolidColorBrush(Color.FromRgb(255, 152, 0));
        private static readonly SolidColorBrush Red = new SolidColorBrush(Color.FromRgb(244, 67, 54));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int statValue = 0;
            if (value is int intValue)
            {
                statValue = intValue;
            }
            else if (value != null && int.TryParse(value.ToString(), out int parsed))
            {
                statValue = parsed;
            }

            if (statValue >= 95)
            {
                return Red;
            }
            if (statValue >= 90)
            {
                return Orange;
            }
            if (statValue >= 80)
            {
                return Yellow;
            }

            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
