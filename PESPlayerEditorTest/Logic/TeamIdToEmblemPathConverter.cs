using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Convierte el TeamId de un equipo en el BitmapImage de su emblema,
    /// buscado en la carpeta "resources/team" junto al ejecutable. Los
    /// emblemas deben extraerse PREVIAMENTE (fuera de la app, con una
    /// herramienta externa) desde flag_4030.flg/flag_160f.flg, ya que
    /// decodificar los .flg en tiempo de ejecución resultaba demasiado lento
    /// para la carga del editor.
    ///
    /// Los archivos están numerados en base 0 (el primer equipo, TeamId=1,
    /// corresponde a "0.png"), mientras que TeamId es en base 1; de ahí el
    /// ajuste "teamId - 1" al construir el nombre del archivo.
    ///
    /// Se decodifica manualmente con BitmapImage (en vez de devolver solo la
    /// ruta y dejar que WPF la resuelva) para poder detectar y loguear
    /// archivos con formatos no soportados (algunos .bmp/.png extraídos del
    /// juego usan variantes de paleta/profundidad de color que el decodificador
    /// por defecto de WPF rechaza, fallando en silencio y dejando la imagen en
    /// blanco sin ningún aviso).
    /// </summary>
    public class TeamIdToEmblemPathConverter : IValueConverter
    {
        private static readonly string EmblemsFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "team");

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int teamId)
            {
                return null;
            }

            int emblemIndex = teamId - 1;
            string path = Path.Combine(EmblemsFolder, $"{emblemIndex}.png");

            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(path);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = new MemoryStream(bytes);
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            }
            catch (Exception ex)
            {
                // Formato no soportado por el decodificador de WPF (ej. BMP con
                // paleta/profundidad de color inusual). Se loguea para poder
                // identificar qué archivos necesitan re-exportarse/convertirse,
                // pero no se rompe la carga del resto de emblemas.
                Debug.WriteLine($"[TeamIdToEmblemPathConverter] No se pudo decodificar '{path}': {ex.Message}");
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}