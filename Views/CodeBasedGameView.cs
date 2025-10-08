using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Input;
using GalacticCommander.Models;
using GalacticCommander.Services;

namespace GalacticCommander.Views
{
    /// <summary>
    /// Code-based Game View - Demonstrates advanced WPF programming without XAML
    /// Shows: Dynamic UI creation, Canvas manipulation, Animation, Event handling
    /// </summary>
    public class CodeBasedGameView : UserControl
    {
        private Canvas _starFieldCanvas;
        private Canvas _gameCanvas;
        private List<Ellipse> _starField;
        private DispatcherTimer _gameTimer;
        private DispatcherTimer _starTimer;
        private Rectangle _player;
        private List<Rectangle> _bullets;
        private List<Rectangle> _enemies;
        private Random _random;
        private double _playerX, _playerY;
        private bool[] _keys = new bool[256];

        public CodeBasedGameView()
        {
            InitializeGame();
            SetupUI();
            StartGame();
        }

        /// <summary>
        /// Initialize game state and collections
        /// Demonstrates: Collection initialization, Random seed, Game state setup
        /// </summary>
        private void InitializeGame()
        {
            _starField = new List<Ellipse>();
            _bullets = new List<Rectangle>();
            _enemies = new List<Rectangle>();
            _random = new Random();
            _playerX = 400;
            _playerY = 300;
            
            // Make control focusable for keyboard input
            Focusable = true;
            Focus();
        }

        /// <summary>
        /// Create the UI entirely in code - demonstrates advanced WPF programming
        /// Shows: Dynamic control creation, Canvas layering, Brush creation
        /// </summary>
        private void SetupUI()
        {
            Width = 800;
            Height = 600;
            
            // Create main grid
            var mainGrid = new Grid();
            
            // Create star field canvas
            _starFieldCanvas = new Canvas
            {
                Background = new SolidColorBrush(Color.FromRgb(5, 5, 20)),
                ClipToBounds = true
            };
            
            // Create game canvas
            _gameCanvas = new Canvas
            {
                Background = Brushes.Transparent,
                ClipToBounds = true
            };
            
            // Add canvases to grid (layered)
            mainGrid.Children.Add(_starFieldCanvas);
            mainGrid.Children.Add(_gameCanvas);
            
            // Set content
            Content = mainGrid;
            
            // Generate star field
            GenerateStarField();
            CreatePlayer();
            
            // Setup event handlers
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            Loaded += (s, e) => Focus(); // Ensure we can receive keyboard input
        }

        /// <summary>
        /// Generate animated star field background
        /// Demonstrates: Dynamic shape creation, Canvas positioning, Animation setup
        /// </summary>
        private void GenerateStarField()
        {
            _starFieldCanvas.Children.Clear();
            _starField.Clear();

            // Generate 150 random stars
            for (int i = 0; i < 150; i++)
            {
                var star = new Ellipse
                {
                    Width = _random.NextDouble() * 3 + 1,
                    Height = _random.NextDouble() * 3 + 1,
                    Fill = new SolidColorBrush(Color.FromRgb(
                        (byte)_random.Next(200, 255),
                        (byte)_random.Next(200, 255),
                        (byte)_random.Next(200, 255)
                    ))
                };

                // Random position
                Canvas.SetLeft(star, _random.NextDouble() * 800);
                Canvas.SetTop(star, _random.NextDouble() * 600);

                _starFieldCanvas.Children.Add(star);
                _starField.Add(star);
            }
        }

        /// <summary>
        /// Create player ship
        /// Demonstrates: Shape creation, Gradient brushes, Canvas positioning
        /// </summary>
        private void CreatePlayer()
        {
            _player = new Rectangle
            {
                Width = 20,
                Height = 30,
                Fill = new LinearGradientBrush(
                    Color.FromRgb(0, 150, 255),
                    Color.FromRgb(0, 100, 200),
                    90
                ),
                Stroke = Brushes.Cyan,
                StrokeThickness = 1
            };

            Canvas.SetLeft(_player, _playerX);
            Canvas.SetTop(_player, _playerY);
            _gameCanvas.Children.Add(_player);
        }

        /// <summary>
        /// Start game timers and animation loops
        /// Demonstrates: DispatcherTimer, Event subscription, Animation timing
        /// </summary>
        private void StartGame()
        {
            // Main game loop - 60 FPS
            _gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            _gameTimer.Tick += GameLoop;
            _gameTimer.Start();

            // Star animation timer
            _starTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _starTimer.Tick += AnimateStars;
            _starTimer.Start();
        }

        /// <summary>
        /// Main game loop - demonstrates real-time game programming
        /// Shows: Game state updates, Collision detection, Entity management
        /// </summary>
        private void GameLoop(object? sender, EventArgs e)
        {
            UpdatePlayer();
            UpdateBullets();
            UpdateEnemies();
            SpawnEnemies();
            CheckCollisions();
        }

        /// <summary>
        /// Update player position based on input
        /// Demonstrates: Keyboard input handling, Boundary checking
        /// </summary>
        private void UpdatePlayer()
        {
            const double speed = 5.0;

            if (_keys[(int)Key.A] || _keys[(int)Key.Left])
                _playerX = Math.Max(0, _playerX - speed);
            
            if (_keys[(int)Key.D] || _keys[(int)Key.Right])
                _playerX = Math.Min(780, _playerX + speed);
                
            if (_keys[(int)Key.W] || _keys[(int)Key.Up])
                _playerY = Math.Max(0, _playerY - speed);
                
            if (_keys[(int)Key.S] || _keys[(int)Key.Down])
                _playerY = Math.Min(570, _playerY + speed);

            Canvas.SetLeft(_player, _playerX);
            Canvas.SetTop(_player, _playerY);
        }

        /// <summary>
        /// Update bullet positions and cleanup
        /// Demonstrates: Collection manipulation, LINQ usage, Performance optimization
        /// </summary>
        private void UpdateBullets()
        {
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                var bullet = _bullets[i];
                var currentY = Canvas.GetTop(bullet);
                currentY -= 10; // Bullet speed

                if (currentY < -10)
                {
                    // Remove bullet that went off screen
                    _gameCanvas.Children.Remove(bullet);
                    _bullets.RemoveAt(i);
                }
                else
                {
                    Canvas.SetTop(bullet, currentY);
                }
            }
        }

        /// <summary>
        /// Update enemy positions
        /// Demonstrates: AI behavior, Smooth movement, Game entity management
        /// </summary>
        private void UpdateEnemies()
        {
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                var enemy = _enemies[i];
                var currentY = Canvas.GetTop(enemy);
                currentY += 3; // Enemy speed

                if (currentY > 610)
                {
                    // Remove enemy that went off screen
                    _gameCanvas.Children.Remove(enemy);
                    _enemies.RemoveAt(i);
                }
                else
                {
                    Canvas.SetTop(enemy, currentY);
                }
            }
        }

        /// <summary>
        /// Spawn new enemies randomly
        /// Demonstrates: Procedural generation, Random numbers, Game balance
        /// </summary>
        private void SpawnEnemies()
        {
            if (_random.NextDouble() < 0.02) // 2% chance per frame
            {
                var enemy = new Rectangle
                {
                    Width = 15,
                    Height = 15,
                    Fill = new RadialGradientBrush(
                        Color.FromRgb(255, 50, 50),
                        Color.FromRgb(150, 0, 0)
                    ),
                    Stroke = Brushes.Red,
                    StrokeThickness = 1
                };

                Canvas.SetLeft(enemy, _random.NextDouble() * 785);
                Canvas.SetTop(enemy, -15);
                _gameCanvas.Children.Add(enemy);
                _enemies.Add(enemy);
            }
        }

        /// <summary>
        /// Check for collisions between game entities
        /// Demonstrates: Collision detection, Rectangular bounds, Game mechanics
        /// </summary>
        private void CheckCollisions()
        {
            // Bullet vs Enemy collisions
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                for (int j = _enemies.Count - 1; j >= 0; j--)
                {
                    var bullet = _bullets[i];
                    var enemy = _enemies[j];

                    var bulletRect = new Rect(
                        Canvas.GetLeft(bullet), Canvas.GetTop(bullet),
                        bullet.Width, bullet.Height
                    );

                    var enemyRect = new Rect(
                        Canvas.GetLeft(enemy), Canvas.GetTop(enemy),
                        enemy.Width, enemy.Height
                    );

                    if (bulletRect.IntersectsWith(enemyRect))
                    {
                        // Hit! Remove both
                        _gameCanvas.Children.Remove(bullet);
                        _gameCanvas.Children.Remove(enemy);
                        _bullets.RemoveAt(i);
                        _enemies.RemoveAt(j);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Animate star field for parallax effect
        /// Demonstrates: Smooth animation, Visual effects, Performance optimization
        /// </summary>
        private void AnimateStars(object? sender, EventArgs e)
        {
            foreach (var star in _starField)
            {
                var currentTop = Canvas.GetTop(star);
                currentTop += star.Width * 0.5; // Bigger stars move faster (parallax)

                if (currentTop > 610)
                {
                    Canvas.SetTop(star, -10);
                    Canvas.SetLeft(star, _random.NextDouble() * 800);
                }
                else
                {
                    Canvas.SetTop(star, currentTop);
                }
            }
        }

        /// <summary>
        /// Handle key press events
        /// Demonstrates: Event handling, Input management, Game controls
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if ((int)e.Key < _keys.Length)
                _keys[(int)e.Key] = true;

            // Fire bullet on spacebar
            if (e.Key == Key.Space)
            {
                FireBullet();
            }
        }

        /// <summary>
        /// Handle key release events
        /// </summary>
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if ((int)e.Key < _keys.Length)
                _keys[(int)e.Key] = false;
        }

        /// <summary>
        /// Fire a bullet from player position
        /// Demonstrates: Projectile creation, Dynamic object management
        /// </summary>
        private void FireBullet()
        {
            var bullet = new Rectangle
            {
                Width = 3,
                Height = 10,
                Fill = Brushes.Yellow,
                Stroke = Brushes.Orange,
                StrokeThickness = 0.5
            };

            Canvas.SetLeft(bullet, _playerX + 8); // Center on player
            Canvas.SetTop(bullet, _playerY - 5);
            _gameCanvas.Children.Add(bullet);
            _bullets.Add(bullet);
        }

        /// <summary>
        /// Cleanup resources when control is unloaded
        /// Demonstrates: Resource management, Timer cleanup, Event handling
        /// </summary>
        public void StopGame()
        {
            _gameTimer?.Stop();
            _starTimer?.Stop();
        }
        
        /// <summary>
        /// Constructor cleanup - demonstrates proper disposal pattern
        /// </summary>
        ~CodeBasedGameView()
        {
            StopGame();
        }
    }
}