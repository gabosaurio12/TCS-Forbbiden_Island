using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace Forbbiden.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(App));

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                MessageBox.Show(
                    e.ExceptionObject.ToString(),
                    "UnhandledException",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            };

            DispatcherUnhandledException += (s, e) =>
            {
                MessageBox.Show(
                    e.Exception.ToString(),
                    "DispatcherUnhandledException",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                e.Handled = true;
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var langCode = Forbbiden.Client.Properties.Settings.Default.languageCode;
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(langCode);
            base.OnStartup(e);

            XmlConfigurator.Configure(new FileInfo("log4net.config"));

            Log.Info("App running");
        }
    }
}