using GalacticCommander.ViewModels;
using GalacticCommander.Models;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GalacticCommander.Views
{
    /// <summary>
    /// Advanced Game View with Custom Rendering and Real-time Updates
    /// Demonstrates: Custom WPF Controls, High-Performance Rendering, Data Binding Optimization
    /// </summary>
    public partial class GameView : UserControl
    {
        private readonly Random _random = new();
        private readonly List<Ellipse> _starField = new();

        public GameView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            GenerateStarField();
        }

        /// <summary>
        /// Generates a dynamic star field background
        /// Shows procedural content generation and WPF canvas manipulation
        /// </summary>
        private void GenerateStarField()
        {
            StarFieldCanvas.Children.Clear();
            _starField.Clear();

            // Generate random stars
            for (int i = 0; i < 200; i++)
            {
                var star = new Ellipse
                {
                    Width = _random.NextDouble() * 2 + 1,
                    Height = _random.NextDouble() * 2 + 1,
                    Fill = new SolidColorBrush(Color.FromArgb(
                        (byte)(_random.Next(100, 255)),
                        255, 255, 255))
                };

                Canvas.SetLeft(star, _random.NextDouble() * 1200);
                Canvas.SetTop(star, _random.NextDouble() * 800);

                StarFieldCanvas.Children.Add(star);
                _starField.Add(star);
            }
        }

        /// <summary>
        /// Handles view model changes and event binding
        /// Shows proper MVVM event handling and cleanup
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is GameViewModel oldViewModel)
            {
                // Unsubscribe from old events
                oldViewModel.GameEntities.CollectionChanged -= OnEntitiesChanged;
                oldViewModel.Particles.CollectionChanged -= OnParticlesChanged;
            }

            if (e.NewValue is GameViewModel newViewModel)
            {
                // Subscribe to new events
                newViewModel.GameEntities.CollectionChanged += OnEntitiesChanged;
                newViewModel.Particles.CollectionChanged += OnParticlesChanged;
            }
        }

        /// <summary>
        /// High-performance entity rendering with object pooling concepts
        /// Shows optimized UI updates and rendering techniques
        /// </summary>
        private void OnEntitiesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Clear existing entities
            var toRemove = GameCanvas.Children.OfType<FrameworkElement>()
                .Where(child => child.Tag?.ToString() == "GameEntity").ToList();
            
            foreach (var child in toRemove)
            {
                GameCanvas.Children.Remove(child);
            }

            // Add new entities
            if (DataContext is GameViewModel viewModel)
            {
                foreach (var entity in viewModel.GameEntities)
                {
                    var entityElement = CreateEntityElement(entity);
                    GameCanvas.Children.Add(entityElement);
                }
            }
        }

        /// <summary>
        /// Creates visual elements for game entities with advanced styling
        /// Shows dynamic UI element creation and styling
        /// </summary>
        private FrameworkElement CreateEntityElement(GameEntityViewModel entity)
        {
            // Create a rectangle for the entity (in a real game, you'd load textures)
            var rectangle = new Rectangle
            {
                Width = entity.Size.X,
                Height = entity.Size.Y,
                Fill = new SolidColorBrush(entity.Color),
                Tag = "GameEntity"
            };

            // Apply transform
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new TranslateTransform(
                entity.Position.X - entity.Size.X / 2,
                entity.Position.Y - entity.Size.Y / 2));
            transformGroup.Children.Add(new RotateTransform(
                entity.Rotation * 180 / Math.PI,
                entity.Size.X / 2,
                entity.Size.Y / 2));

            rectangle.RenderTransform = transformGroup;

            // Add glow effect for certain entities
            if (entity.Color == Colors.Cyan) // Player
            {
                rectangle.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Cyan,
                    BlurRadius = 10,
                    ShadowDepth = 0,
                    Opacity = 0.8
                };
            }

            return rectangle;
        }

        /// <summary>
        /// High-performance particle rendering
        /// Shows advanced visual effects and performance optimization
        /// </summary>
        private void OnParticlesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Clear existing particles
            var toRemove = GameCanvas.Children.OfType<FrameworkElement>()
                .Where(child => child.Tag?.ToString() == "Particle").ToList();
            
            foreach (var child in toRemove)
            {
                GameCanvas.Children.Remove(child);
            }

            // Add new particles
            if (DataContext is GameViewModel viewModel)
            {
                foreach (var particle in viewModel.Particles)
                {
                    var particleElement = CreateParticleElement(particle);
                    GameCanvas.Children.Add(particleElement);
                }
            }
        }

        /// <summary>
        /// Creates optimized particle visual elements
        /// Shows performance-optimized rendering for many small objects
        /// </summary>
        private FrameworkElement CreateParticleElement(ParticleViewModel particle)
        {
            var ellipse = new Ellipse
            {
                Width = particle.Size,
                Height = particle.Size,
                Fill = new SolidColorBrush(particle.Color),
                Tag = "Particle"
            };

            Canvas.SetLeft(ellipse, particle.Position.X - particle.Size / 2);
            Canvas.SetTop(ellipse, particle.Position.Y - particle.Size / 2);

            // Add rotation if needed
            if (particle.Rotation != 0)
            {
                var rotateTransform = new RotateTransform(particle.Rotation * 180 / Math.PI);
                ellipse.RenderTransform = rotateTransform;
            }

            return ellipse;
        }
    }
}