using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Convierte el identificador num�rico de posici�n (0-10) en su nombre
    /// legible (ej. "Centre Forward"), reutilizando la lista est�tica de
    /// posiciones definida en PlayerWindow, para mostrarlo como texto de
    /// solo lectura en el resumen del jugador.
    /// </summary>
    public class PositionIdToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int positionId)
            {
                return string.Empty;
            }

            return PESPlayerEditorTest.PlayerWindow.Positions
                .FirstOrDefault(p => p.PositionId == positionId)?.PositionName ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}