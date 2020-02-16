using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace GameCrashReporter
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Create a new instance of the CrashReporter class which implements the logic
            crashReporter = new CrashReporter();
        }

        // Called when the user clicks the "Close without sending" button
        private void CloseWithoutSendingButton_Click(object sender, RoutedEventArgs e)
        {
            // Just close the reporter
            Environment.Exit(0);
        }

        // Called when the user clicks the "Send and close" button
        private void SendAndCloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Try to load the crash data
            if (crashReporter.LoadCrashData())
            {
                // If that worked, try to compress the crash data to a .zip file
                FileInfo zip = crashReporter.CompressCrashData();

                // Make sure we have a valid archive now
                if (zip != null)
                {
                    // Send the crash data
                    crashReporter.SendCrashData(zip);

                    // Close afterwards
                    Environment.Exit(0);
                }
            }
        }

        // Called when the user inputs text to the description box
        private void CrashDescTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Cast to the text box
            TextBox textBox = (TextBox)e.Source;

            // Notify the crash reporter about the new text
            crashReporter.SetUserDescription(textBox.Text);
        }

        // The crash reporter instance to use
        private CrashReporter crashReporter;
    }
}
