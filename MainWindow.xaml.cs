using GalacticCommander.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;

namespace GalacticCommander
{
    /// <summary>
    /// Advanced WPF Main Window with Dependency Injection and Modern MVVM
    /// Demonstrates: Dependency Injection in WPF, Advanced Window Management, Input Handling
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;
            
            // Configure window properties
            SetupWindow();
            
            // Setup input handling
            SetupInputHandling();
        }

        /// <summary>
        /// Advanced window configuration with modern WPF features
        /// Shows proper window setup and event handling
        /// </summary>
        private void SetupWindow()
        {
            // Window properties for game
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            
            // Allow the window to receive focus
            Focusable = true;
            
            // Handle window events
            Loaded += OnWindowLoaded;
            Closing += OnWindowClosing;
            KeyDown += OnWindowKeyDown;
            KeyUp += OnWindowKeyUp;
            MouseMove += OnWindowMouseMove;
            MouseDown += OnWindowMouseDown;
        }

        /// <summary>
        /// Comprehensive input event handling with service integration
        /// Shows advanced input processing and event routing
        /// </summary>
        private void SetupInputHandling()
        {
            // Ensure we can capture keyboard input
            Focus();
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Initialize any post-load operations
            Focus();
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Cleanup resources
            if (_viewModel.CurrentViewModel is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// Advanced keyboard input handling with service routing
        /// Shows input service integration and event processing
        /// </summary>
        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            // Route to input service through DI container
            var inputService = ((App)Application.Current)._host?.Services.GetService<Services.IInputService>();
            inputService?.UpdateKeyState(e.Key, true);

            // Handle special keys
            switch (e.Key)
            {
                case Key.Escape:
                    if (_viewModel.IsGameRunning)
                        _viewModel.NavigateToMenuCommand.Execute(null);
                    else
                        Close();
                    break;
                case Key.F11:
                    ToggleFullscreen();
                    break;
                case Key.F1:
                    ShowHelp();
                    break;
            }

            e.Handled = true;
        }

        private void OnWindowKeyUp(object sender, KeyEventArgs e)
        {
            var inputService = ((App)Application.Current)._host?.Services.GetService<Services.IInputService>();
            inputService?.UpdateKeyState(e.Key, false);
            e.Handled = true;
        }

        private void OnWindowMouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(this);
            var inputService = ((App)Application.Current)._host?.Services.GetService<Services.IInputService>();
            inputService?.UpdateMousePosition(new System.Numerics.Vector2((float)position.X, (float)position.Y));
        }

        private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            var inputService = ((App)Application.Current)._host?.Services.GetService<Services.IInputService>();
            inputService?.UpdateMouseButtonState(e.ChangedButton, true);
        }

        /// <summary>
        /// Advanced window management features
        /// </summary>
        private void ToggleFullscreen()
        {
            if (WindowState == WindowState.Maximized && WindowStyle == WindowStyle.None)
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
            }
            else
            {
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
            }
        }

        private void ShowHelp()
        {
            var helpText = @"GALACTIC COMMANDER - HELP

CONTROLS:
• WASD - Move ship
• Mouse - Aim
• Space - Fire weapon
• P - Pause/Unpause
• Esc - Main menu
• F11 - Toggle fullscreen
• F1 - This help

FEATURES DEMONSTRATED:
• Entity Component System (ECS)
• Advanced MVVM with dependency injection
• Real-time physics simulation
• Particle systems
• AI behavior trees
• Spatial partitioning for collision detection
• Async/await patterns
• LINQ and generics
• Memory management and object pooling
• Event-driven architecture
• Custom WPF controls and animations";

            MessageBox.Show(helpText, "Help - Advanced C# Features", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}