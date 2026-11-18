using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PlayerLibrary;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Traduce el SkinColour del jugador al color de fondo de la tarjeta de
    /// cara en la lista de jugadores, imitando la paleta usada para los
    /// prefijos de código de cara (W/Y/R/K):
    /// W (SKIN_COLOUR_1) = tono por defecto (naranja claro),
    /// Y (SKIN_COLOUR_2) = amarillo,
    /// R (SKIN_COLOUR_3) = naranja oscuro ("special"),
    /// K (SKIN_COLOUR_4) = negro/gris oscuro.
    /// </summary>
    public class SkinColourToBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush DefaultBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xDD, 0xC4));
        private static readonly SolidColorBrush YellowBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xCB, 0x93));
        private static readonly SolidColorBrush SpecialBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0x7B, 0x42));
        private static readonly SolidColorBrush DarkBrush = new SolidColorBrush(Color.FromRgb(0x41, 0x27, 0x1C));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not SkinColour skinColour)
            {
                return DefaultBrush;
            }

            return skinColour switch
            {
                SkinColour.SKIN_COLOUR_1 => DefaultBrush, // W
                SkinColour.SKIN_COLOUR_2 => YellowBrush,  // Y
                SkinColour.SKIN_COLOUR_3 => SpecialBrush, // R
                SkinColour.SKIN_COLOUR_4 => DarkBrush,    // K
                _ => DefaultBrush,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}