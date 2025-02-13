using Serilog;
using System.Windows;

namespace BitfinexApp.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs/log.txt")
                .CreateLogger();

            Log.Information("App is executed");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
        }
    }

}
