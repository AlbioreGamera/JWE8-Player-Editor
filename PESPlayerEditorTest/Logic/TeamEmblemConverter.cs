using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Convierte los bytes BGRA32 crudos del emblema de un equipo (ya
    /// decodificados por <see cref="FlgReader"/> y guardados en
    /// <c>Team.EmblemBgra32</c>) en un BitmapSource listo para enlazarse a un
    /// control Image en el tab "Teams". Usa un IMultiValueConverter porque se
    /// necesitan los tres datos del equipo (bytes, ancho y alto) a la vez.
    /// </summary>
    public class TeamEmblemConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3 ||
                values[0] is not byte[] bgra ||
                values[1] is not int width ||
                values[2] is not int height ||
                width <= 0 || height <= 0 ||
                bgra.Length < width * height * 4)
            {
                return null;
            }

            int stride = width * 4;
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, bgra, stride);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}