using System.Diagnostics;
using System.Windows;

namespace cymatics
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                Debug.WriteLine("Attempting to load NVidia Optimus enabler");
                NVidiaUtils.NvAPI_Initialize_64();
                Debug.WriteLine("success!");
            }
            catch
            {
                Debug.WriteLine("Failed to load optimus. too bad. continuing regardless.");
            }

            new MainWindow();
        }
    }
}
