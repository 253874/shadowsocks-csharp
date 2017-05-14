using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Guldan.Models;
using Guldan.Services;
using Microsoft.Shell;
using Shadowsocks;

namespace Guldan
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private const string UniqueName = @"3021e68df9a7200135725c6331369a22";
        private static App _application;

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(UniqueName))
            {
                _application = new App();
                _application.InitializeComponent();
                _application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        private async void PerformStartup(object sender, StartupEventArgs e)
        {
            var screen = new SplashScreen("Resources/Images/startup.png");
            screen.Show(false, true);


            ServiceInjector.InjectServices();
            var svc = new WarlockService();
            svc.Load();
            ServiceManager.Instance.AddService<Services.Interfaces.IWarlockService>(svc);
            if (svc.Servers == null)
            {
                svc.Servers = new ObservableCollection<Server>(new []{ new Server() });
                screen.Close(TimeSpan.FromSeconds(1));
                MainWindowViewModel.Instance.ShowMainWindow();
            }
            else
            {
                if (svc.Servers.Any(c => c.enabled))
                {
                    try
                    {
                        MainWindowViewModel.Instance.Status = Status.Busy;
                        await svc.StartAll().ConfigureAwait(true);
                        MainWindowViewModel.Instance.Status = Status.Ready;
                    }
                    catch (Exception ex)
                    {
                        MainWindowViewModel.Instance.Status = Status.Disabled;
                        Logging.LogUsefulException(ex);
                    }
                    screen.Close(TimeSpan.Zero);
                    MainWindowViewModel.Instance.MinimizeToTray();
                }
                else
                {
                    screen.Close(TimeSpan.Zero);
                    MainWindowViewModel.Instance.ShowMainWindow();
                }
            }
        }

        #region ISingleInstanceApp Members

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // this is executed in the original instance

            // we get the arguments to the second instance and can send them to the existing instance if desired

            // here we bring the existing instance to the front
            _application.MainWindow.BringToFront();

            // handle command line arguments of second instance

            return true;
        }

        #endregion
    }
}
