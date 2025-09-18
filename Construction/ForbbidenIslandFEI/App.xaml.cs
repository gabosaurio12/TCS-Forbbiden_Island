using System;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace ForbbidenIslandFEI
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("es");

            var win = new RegisterWindow();
            win.Show();
        }
    }
}
