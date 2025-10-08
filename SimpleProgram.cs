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
                Console.WriteLine("App created...");
                
                var window = new SimpleMainWindow();
                Console.WriteLine("Window created...");
                
                window.Show();
                Console.WriteLine("Window shown...");
                
                app.Run(window);
                Console.WriteLine("App finished...");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting application: {ex.Message}\n\nDetails: {ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Error: {ex}");
                Console.ReadKey(); // Wait for key press to see error
            }
        }
    }
}