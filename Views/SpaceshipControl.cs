using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace GalacticCommander.Views
{
    /// <summary>
    /// Cool spaceship design using advanced WPF graphics
    /// Demonstrates: Custom shapes, Gradients, Transforms, Visual effects
    /// </summary>
    public class SpaceshipControl : Canvas
    {
        private readonly List<Shape> _shipParts = new();
        private readonly List<Shape> _engineFlames = new();
        private bool _thrusterActive = false;
        
        public SpaceshipControl()
        {
            Width = 40;
            Height = 50;
            CreateSpaceship();
        }
        
        /// <summary>
        /// Create an awesome spaceship using multiple geometric shapes
        /// Shows: Complex path creation, Gradient brushes, Layering
        /// </summary>
        private void CreateSpaceship()
        {
            // Main hull (diamond shape)
            var hull = new Polygon
            {
                Points = new PointCollection(new[]
                {
                    new Point(20, 0),   // Top point
                    new Point(35, 15),  // Right
                    new Point(30, 40),  // Bottom right
                    new Point(20, 45),  // Bottom center
                    new Point(10, 40),  // Bottom left
                    new Point(5, 15),   // Left
                }),
                Fill = new LinearGradientBrush(
                    new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(100, 150, 255), 0),
                        new GradientStop(Color.FromRgb(50, 100, 200), 0.5),
                        new GradientStop(Color.FromRgb(20, 50, 150), 1)
                    },
                    new Point(0, 0), new Point(1, 1)
                ),
                Stroke = Brushes.Cyan,
                StrokeThickness = 1.5
            };
            
            // Cockpit (bright center)
            var cockpit = new Ellipse
            {
                Width = 12,
                Height = 8,
                Fill = new RadialGradientBrush(
                    new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(200, 230, 255), 0),
                        new GradientStop(Color.FromRgb(100, 150, 255), 0.7),
                        new GradientStop(Color.FromRgb(50, 100, 200), 1)
                    }
                ),
                Stroke = Brushes.White,
                StrokeThickness = 0.5
            };
            Canvas.SetLeft(cockpit, 14);
            Canvas.SetTop(cockpit, 8);
            
            // Wings (left and right)
            var leftWing = new Polygon
            {
                Points = new PointCollection(new[]
                {
                    new Point(5, 15),
                    new Point(0, 25),
                    new Point(8, 30),
                    new Point(12, 20)
                }),
                Fill = new LinearGradientBrush(
                    Color.FromRgb(80, 120, 200),
                    Color.FromRgb(40, 80, 160),
                    90
                ),
                Stroke = Brushes.LightBlue,
                StrokeThickness = 1
            };
            
            var rightWing = new Polygon
            {
                Points = new PointCollection(new[]
                {
                    new Point(35, 15),
                    new Point(40, 25),
                    new Point(32, 30),
                    new Point(28, 20)
                }),
                Fill = new LinearGradientBrush(
                    Color.FromRgb(80, 120, 200),
                    Color.FromRgb(40, 80, 160),
                    90
                ),
                Stroke = Brushes.LightBlue,
                StrokeThickness = 1
            };
            
            // Engine exhausts
            var leftEngine = new Rectangle
            {
                Width = 4,
                Height = 8,
                Fill = new LinearGradientBrush(
                    Color.FromRgb(150, 150, 150),
                    Color.FromRgb(80, 80, 80),
                    90
                ),
                Stroke = Brushes.Gray,
                StrokeThickness = 0.5
            };
            Canvas.SetLeft(leftEngine, 8);
            Canvas.SetTop(leftEngine, 37);
            
            var rightEngine = new Rectangle
            {
                Width = 4,
                Height = 8,
                Fill = new LinearGradientBrush(
                    Color.FromRgb(150, 150, 150),
                    Color.FromRgb(80, 80, 80),
                    90
                ),
                Stroke = Brushes.Gray,
                StrokeThickness = 0.5
            };
            Canvas.SetLeft(rightEngine, 28);
            Canvas.SetTop(rightEngine, 37);
            
            // Add all parts to canvas and tracking list
            var parts = new Shape[] { hull, leftWing, rightWing, leftEngine, rightEngine, cockpit };
            foreach (var part in parts)
            {
                Children.Add(part);
                _shipParts.Add(part);
            }
            
            CreateEngineFlames();
        }
        
        /// <summary>
        /// Create animated engine flames
        /// Demonstrates: Animation preparation, Dynamic effects
        /// </summary>
        private void CreateEngineFlames()
        {
            // Left engine flame
            var leftFlame = new Polygon
            {
                Points = new PointCollection(new[]
                {
                    new Point(8, 45),
                    new Point(12, 45),
                    new Point(11, 52),
                    new Point(10, 55),
                    new Point(9, 52)
                }),
                Fill = new LinearGradientBrush(
                    new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(255, 200, 0), 0),
                        new GradientStop(Color.FromRgb(255, 100, 0), 0.5),
                        new GradientStop(Color.FromRgb(255, 50, 0), 1)
                    },
                    new Point(0.5, 0), new Point(0.5, 1)
                ),
                Opacity = 0.8,
                Visibility = Visibility.Hidden
            };
            
            // Right engine flame
            var rightFlame = new Polygon
            {
                Points = new PointCollection(new[]
                {
                    new Point(28, 45),
                    new Point(32, 45),
                    new Point(31, 52),
                    new Point(30, 55),
                    new Point(29, 52)
                }),
                Fill = new LinearGradientBrush(
                    new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(255, 200, 0), 0),
                        new GradientStop(Color.FromRgb(255, 100, 0), 0.5),
                        new GradientStop(Color.FromRgb(255, 50, 0), 1)
                    },
                    new Point(0.5, 0), new Point(0.5, 1)
                ),
                Opacity = 0.8,
                Visibility = Visibility.Hidden
            };
            
            Children.Add(leftFlame);
            Children.Add(rightFlame);
            _engineFlames.Add(leftFlame);
            _engineFlames.Add(rightFlame);
        }
        
        /// <summary>
        /// Activate or deactivate engine flames
        /// Shows: Animation control, Visual feedback
        /// </summary>
        public void SetThruster(bool active)
        {
            _thrusterActive = active;
            var visibility = active ? Visibility.Visible : Visibility.Hidden;
            
            foreach (var flame in _engineFlames)
            {
                flame.Visibility = visibility;
            }
        }
        
        /// <summary>
        /// Get current thruster state
        /// Demonstrates: State management, Property patterns
        /// </summary>
        public bool IsThrusterActive => _thrusterActive;
        
        /// <summary>
        /// Animate engine flames (called from game loop)
        /// Shows: Procedural animation, Visual effects
        /// </summary>
        public void UpdateAnimation()
        {
            if (!_thrusterActive) return;
            
            var random = new Random();
            
            foreach (var flame in _engineFlames)
            {
                // Flicker effect
                flame.Opacity = 0.6 + (random.NextDouble() * 0.4);
                
                // Slight scale variation for flame effect
                var transform = flame.RenderTransform as ScaleTransform ?? new ScaleTransform();
                transform.ScaleY = 0.8 + (random.NextDouble() * 0.4);
                flame.RenderTransform = transform;
            }
        }
        
        /// <summary>
        /// Create damage effect on ship
        /// Demonstrates: Visual feedback, Damage indication
        /// </summary>
        public void ShowDamage()
        {
            foreach (var part in _shipParts)
            {
                // Flash red briefly
                var originalBrush = part.Fill;
                part.Fill = Brushes.Red;
                
                // Use dispatcher to restore original color after brief delay
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(100)
                };
                timer.Tick += (s, e) =>
                {
                    part.Fill = originalBrush;
                    timer.Stop();
                };
                timer.Start();
            }
        }
        
        /// <summary>
        /// Create shield effect around ship
        /// Shows: Protective visual effects, Temporary shields
        /// </summary>
        public void ShowShield(bool active)
        {
            const string shieldName = "ShieldEffect";
            
            // Remove existing shield
            var existingShield = Children.Cast<FrameworkElement>().FirstOrDefault(c => c.Name == shieldName);
            if (existingShield != null)
            {
                Children.Remove(existingShield);
            }
            
            if (active)
            {
                var shield = new Ellipse
                {
                    Name = shieldName,
                    Width = 60,
                    Height = 60,
                    Stroke = Brushes.Cyan,
                    StrokeThickness = 2,
                    Fill = new SolidColorBrush(Color.FromArgb(30, 0, 255, 255)),
                    Opacity = 0.7
                };
                
                Canvas.SetLeft(shield, -10);
                Canvas.SetTop(shield, -5);
                Children.Insert(0, shield); // Add behind ship
            }
        }
    }
    
    /// <summary>
    /// Explosion effect for destroyed enemies
    /// Demonstrates: Particle systems, Animation, Visual effects
    /// </summary>
    public class ExplosionEffect : Canvas
    {
        private readonly List<Shape> _particles = new();
        private readonly System.Windows.Threading.DispatcherTimer _animationTimer;
        private int _animationFrame = 0;
        private const int MaxFrames = 30;
        
        public ExplosionEffect(double x, double y)
        {
            Width = 100;
            Height = 100;
            Canvas.SetLeft(this, x - 50);
            Canvas.SetTop(this, y - 50);
            
            CreateParticles();
            
            _animationTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
            };
            _animationTimer.Tick += AnimateExplosion;
            _animationTimer.Start();
        }
        
        /// <summary>
        /// Create explosion particle system
        /// Shows: Procedural generation, Particle systems
        /// </summary>
        private void CreateParticles()
        {
            var random = new Random();
            
            for (int i = 0; i < 20; i++)
            {
                var particle = new Ellipse
                {
                    Width = random.NextDouble() * 6 + 2,
                    Height = random.NextDouble() * 6 + 2,
                    Fill = new SolidColorBrush(Color.FromRgb(
                        (byte)random.Next(200, 255),
                        (byte)random.Next(100, 200),
                        (byte)random.Next(0, 100)
                    ))
                };
                
                Canvas.SetLeft(particle, 45 + (random.NextDouble() - 0.5) * 10);
                Canvas.SetTop(particle, 45 + (random.NextDouble() - 0.5) * 10);
                
                Children.Add(particle);
                _particles.Add(particle);
            }
        }
        
        /// <summary>
        /// Animate explosion particles
        /// Demonstrates: Frame-based animation, Interpolation
        /// </summary>
        private void AnimateExplosion(object? sender, EventArgs e)
        {
            _animationFrame++;
            
            var progress = (double)_animationFrame / MaxFrames;
            var random = new Random(_animationFrame); // Consistent randomness
            
            for (int i = 0; i < _particles.Count; i++)
            {
                var particle = _particles[i];
                
                // Expand outward
                var angle = (i * 360.0 / _particles.Count) * Math.PI / 180;
                var distance = progress * 40;
                
                Canvas.SetLeft(particle, 45 + Math.Cos(angle) * distance);
                Canvas.SetTop(particle, 45 + Math.Sin(angle) * distance);
                
                // Fade out
                particle.Opacity = 1 - progress;
                
                // Shrink
                var scale = 1 - (progress * 0.5);
                particle.Width *= scale;
                particle.Height *= scale;
            }
            
            if (_animationFrame >= MaxFrames)
            {
                _animationTimer.Stop();
                // Signal that explosion is complete
                ExplosionComplete?.Invoke(this, EventArgs.Empty);
            }
        }
        
        /// <summary>
        /// Event fired when explosion animation completes
        /// Shows: Event patterns, Cleanup notification
        /// </summary>
        public event EventHandler? ExplosionComplete;
    }
}