using System;
using System.Windows;

namespace PESPlayerEditorTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly Uri AppIconUri = new Uri("pack://application:,,,/resources/jwe8ico.ico", UriKind.Absolute);

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Apply dark title bar / dark native system menu + consistent app icon
            // to every window the app creates, without needing per-window code.
            EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnAnyWindowLoaded));
        }

        private void OnAnyWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Window window)
            {
                WindowThemeHelper.ApplyAppIcon(window, AppIconUri);
                WindowThemeHelper.ApplyDarkTitleBar(window);
            }
        }
    }
}
