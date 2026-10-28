using System;
using System.Globalization;
using System.Windows.Data;
using PlayerLibrary;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Habilita los campos de personalizaci�n de botas (colores base, l�neas,
    /// lengueta, tacos) �nicamente cuando el BootsType seleccionado es uno de
    /// los modelos editables (BOOT_EDITABLE_1 o BOOT_EDITABLE_2); el resto de
    /// los modelos (marcas reales como Adidas F50/Predator) tienen colores
    /// fijos y no admiten personalizaci�n.
    /// </summary>
    public class BootsTypeToIsEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is BootsType bootsType &&
                (bootsType == BootsType.BOOT_EDITABLE_1 || bootsType == BootsType.BOOT_EDITABLE_2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
