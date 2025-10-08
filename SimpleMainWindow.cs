using System;
using System.Windows;
using System.Windows.Controls;
using GalacticCommander.Views;

namespace GalacticCommander
{
    public partial class SimpleMainWindow : Window
    {
        public SimpleMainWindow()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            Title = "Galactic Commander - Code-Based Version";
            Width = 820;
            Height = 640;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            // Create main grid
            var mainGrid = new Grid();
            
            // Create menu panel
            var menuPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 10, 0, 0)
            };
            
            var titleLabel = new Label
            {
                Content = "🚀 GALACTIC COMMANDER 🚀",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.Cyan
            };
            
            var infoLabel = new Label
            {
                Content = "Advanced C# Game - No XAML Required!\n\n" +
                         "Controls: WASD/Arrow Keys to move, Space to fire\n" +
                         "Demonstrates 40+ Advanced C# Concepts",
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.White
            };
            
            var startButton = new Button
            {
                Content = "▶ Start Game",
                Width = 150,
                Height = 40,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 0)
            };
            
            var conceptsButton = new Button
            {
                Content = "📚 View Concepts",
                Width = 150,
                Height = 30,
                FontSize = 12,
                Margin = new Thickness(0, 5, 0, 0)
            };
            
            startButton.Click += StartGame;
            conceptsButton.Click += ShowConcepts;
            
            menuPanel.Children.Add(titleLabel);
            menuPanel.Children.Add(infoLabel);
            menuPanel.Children.Add(startButton);
            menuPanel.Children.Add(conceptsButton);
            
            mainGrid.Children.Add(menuPanel);
            
            // Set dark background
            mainGrid.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(10, 10, 30)
            );
            
            Content = mainGrid;
        }
        
        private void StartGame(object sender, RoutedEventArgs e)
        {
            try
            {
                // Replace content with the enhanced game
                Content = new EnhancedGameView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting game: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                    "Game Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ShowConcepts(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "🎯 ADVANCED C# CONCEPTS DEMONSTRATED:\n\n" +
                "🏗️ ARCHITECTURAL PATTERNS:\n" +
                "• Entity Component System (ECS)\n" +
                "• Service-Oriented Architecture\n" +
                "• Dependency Injection Container\n" +
                "• MVVM Pattern Implementation\n\n" +
                "⚡ ADVANCED PROGRAMMING:\n" +
                "• Async/Await Patterns\n" +
                "• Task-based Programming\n" +
                "• LINQ Expressions & Queries\n" +
                "• Generic Constraints\n" +
                "• Reflection & Attributes\n" +
                "• Extension Methods\n\n" +
                "🎮 GAME DEVELOPMENT:\n" +
                "• Real-time Game Loop\n" +
                "• Collision Detection\n" +
                "• Spatial Partitioning\n" +
                "• Object Pooling\n" +
                "• Performance Optimization\n\n" +
                "🎨 WPF MASTERY:\n" +
                "• Code-based UI Creation\n" +
                "• Canvas Manipulation\n" +
                "• Custom Controls\n" +
                "• Animation Systems\n" +
                "• Event Handling\n" +
                "• Resource Management\n\n" +
                "🧮 MATHEMATICAL PROGRAMMING:\n" +
                "• Vector Mathematics\n" +
                "• Physics Simulation\n" +
                "• Interpolation Algorithms\n" +
                "• Random Number Generation",
                "Advanced C# Learning Project",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}