using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Shell;

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

        private void PerformStartup(object sender, StartupEventArgs e)
        {

            SplashScreen screen = new SplashScreen("Resources/Images/startup.png");
            screen.Show(true, true);
            new MainWindow().Show();
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
