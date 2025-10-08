using System;
using System.Windows;

namespace GalacticCommander
{
    public class TestApp
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                var app = new Application();
                var window = new Window
                {
                    Title = "Test Window",
                    Width = 400,
                    Height = 300,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                
                app.Run(window);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Application Error");
            }
        }
    }
}