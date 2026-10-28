using JWE8Editor.Logic;
using PlayerLibrary;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace PESPlayerEditorTest
{
    /// <summary>
    /// Combina SkinColour (que determina el grupo W/Y/R/K) y FaceId (el número dentro
    /// de ese grupo) para armar el código de cara (ej. W001, Y034, R007, K012) y
    /// mostrar el nombre del jugador real al que pertenece esa cara preestablecida.
    /// </summary>
    public class FaceIdToPlayerNameConverter : IMultiValueConverter
    {
        private static readonly string[] Prefixes = { "W", "Y", "R", "K" };

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] == null || values[1] == null)
            {
                return string.Empty;
            }

            if (!int.TryParse(values[0].ToString(), out int faceId) || faceId <= 0)
            {
                return string.Empty;
            }

            if (values[1] is not SkinColour skinColour)
            {
                return string.Empty;
            }

            int prefixIndex = (int)skinColour;
            if (prefixIndex < 0 || prefixIndex >= Prefixes.Length)
            {
                return string.Empty;
            }

            string code = $"{Prefixes[prefixIndex]}{faceId:D3}";
            FacePlayer? match = LoadFacePlayers.FacePlayers
                .FirstOrDefault(p => string.Equals(p.FaceCode, code, StringComparison.OrdinalIgnoreCase));

            return match?.ToString() ?? "Unknown";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}