using System;
using System.Windows;

namespace GalacticCommander
{
    public class SimpleApp : Application
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                Console.WriteLine("Starting Galactic Commander...");
                
                var app = new SimpleApp();
                var window = new SimpleMainWindow();
                
                app.Run(window);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting application: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Error: {ex}");
            }
        }
    }
}