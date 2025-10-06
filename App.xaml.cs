using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;

namespace TheScheduler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            CultureInfo japanese = new CultureInfo("ja-JP");
            Thread.CurrentThread.CurrentCulture = japanese;
            Thread.CurrentThread.CurrentUICulture = japanese;

            base.OnStartup(e);
        }
    }

}
