using System.Windows;

namespace PESPlayerEditorTest
{
    /// <summary>
    /// Ventana de progreso modal sencilla, usada para evitar que la UI se congele
    /// mientras se procesan archivos AFS grandes (potencialmente con cientos de
    /// sub-archivos anidados).
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
        }

        public void UpdateStatus(string detail)
        {
            DetailText.Text = detail;
        }
    }
}