using System;
using System.Globalization;
using System.Windows.Data;
using PlayerLibrary;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Compone el texto "Slots: faceId / maxFaceId" mostrado en el panel de
    /// detalle de la lista de jugadores, combinando el FaceId actual del
    /// jugador con el máximo disponible para su SkinColour (según el
    /// catálogo de caras preestablecidas de LoadFacePlayers).
    /// </summary>
    public class FaceSlotsToLabelConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] is not int faceId || values[1] is not SkinColour skinColour)
            {
                return string.Empty;
            }

            int maxFaceId = LoadFacePlayers.GetMaxFaceId(skinColour);
            return $"{faceId} / {maxFaceId}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}