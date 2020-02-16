using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GameCrashReporter
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        // Called when the app starts up
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Only start the reporter if we received a valid crash GUID
            bool shouldStart = false;

            // Check the command line if we got everything to run
            foreach (var arg in e.Args)
            {
                if (arg.Contains("-CrashGUID") || arg.Contains("-CrashVersion"))
                {
                    shouldStart = true;
                }
            }

            if (!shouldStart)
            {
                Shutdown();
            }
        }
    }
}
