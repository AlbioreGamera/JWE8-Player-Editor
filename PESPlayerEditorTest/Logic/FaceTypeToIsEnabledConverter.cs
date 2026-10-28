using System;
using System.Globalization;
using System.Windows.Data;
using PlayerLibrary;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Habilita el TextBox de Face Id únicamente cuando el Face Type
    /// seleccionado es PRESET; en los demás casos (BUILD, ORIGINAL) el
    /// campo se deshabilita, ya que solo las caras preestablecidas usan
    /// un código numérico (FaceId) vinculado a un jugador real.
    /// </summary>
    public class FaceTypeToIsEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is FaceType faceType && faceType == FaceType.PRESET;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
