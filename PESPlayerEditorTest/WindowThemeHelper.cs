using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace PESPlayerEditorTest
{
    /// <summary>
    /// Forces Windows to render a given window's title bar, borders and native
    /// right-click (system) menu in dark mode, matching the app's dark theme
    /// (instead of the default light OS chrome), and ensures the app icon is
    /// applied to the native window handle (title bar / Alt-Tab / taskbar).
    /// </summary>
    internal static class WindowThemeHelper
    {
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, int cbAttribute);

        /// <summary>
        /// Attaches dark-mode title bar/system-menu styling to the given window.
        /// Safe to call multiple times; does nothing on unsupported Windows versions.
        /// </summary>
        public static void ApplyDarkTitleBar(Window window)
        {
            if (window == null)
            {
                return;
            }

            void TryApply()
            {
                var helper = new WindowInteropHelper(window);
                if (helper.Handle == IntPtr.Zero)
                {
                    return;
                }

                int useDarkMode = 1;
                int result = DwmSetWindowAttribute(helper.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
                if (result != 0)
                {
                    // Older Windows 10 builds (pre-20H1) use a different attribute id.
                    DwmSetWindowAttribute(helper.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useDarkMode, sizeof(int));
                }
            }

            if (window.IsLoaded && PresentationSource.FromVisual(window) != null)
            {
                TryApply();
            }
            else
            {
                window.SourceInitialized += (s, e) => TryApply();
            }
        }

        /// <summary>
        /// Explicitly sets the app icon on the native window handle so the title bar,
        /// Alt-Tab preview and taskbar thumbnail all show the app icon consistently.
        /// </summary>
        public static void ApplyAppIcon(Window window, Uri iconUri)
        {
            if (window == null || iconUri == null)
            {
                return;
            }

            try
            {
                window.Icon = BitmapFrame.Create(iconUri);
            }
            catch
            {
                // Ignore icon load failures; the Window.Icon style from App.xaml still applies.
            }
        }
    }
}