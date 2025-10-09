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


            // Syncfusion 라이선스 키 등록
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH9eeHRTRGBdUkV2XUtWYEg=");
            InitializeComponent();

            base.OnStartup(e);
        }
    }

}
