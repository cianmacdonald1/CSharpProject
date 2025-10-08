using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Input;
using GalacticCommander.Models;
using System.Linq;

namespace GalacticCommander.Views
{
    /// <summary>
    /// Enhanced Game View with menu system, high scores, and player death
    /// Demonstrates: State management, Game systems, Advanced WPF programming
    /// </summary>
    public class EnhancedGameView : UserControl
    {
        // Game state management
        private GameState _currentState = GameState.MainMenu;
        private GameSettings _settings;
        private HighScoreManager _highScoreManager;
        private GameStats _gameStats;
        
        // UI Canvases for different states
        private Canvas _menuCanvas = null!;
        private Canvas _gameCanvas = null!;
        private Canvas _starFieldCanvas = null!;
        private Canvas _uiCanvas = null!;
        
        // Game objects
        private SpaceshipControl _player = null!;
        private List<Ellipse> _starField = new();
        private List<Rectangle> _bullets = new();
        private List<Rectangle> _enemies = new();
        private List<ExplosionEffect> _explosions = new();
        
        // Game mechanics
        private DispatcherTimer _gameTimer = null!;
        private DispatcherTimer _starTimer = null!;
        private Random _random = new();
        private double _playerX = 400;
        private double _playerY = 300;
        private bool[] _keys = new bool[256];
        private DateTime _lastShotTime = DateTime.MinValue;
        private bool _isInvulnerable = false;
        private DateTime _invulnerabilityStartTime;
        
        // UI Elements
        private TextBlock _scoreText = null!;
        private TextBlock _livesText = null!;
        private TextBlock _comboText = null!;

        public EnhancedGameView()
        {
            _settings = GameSettings.Load();
            _highScoreManager = new HighScoreManager();
            _highScoreManager.Load();
            _gameStats = new GameStats();
            
            InitializeGame();
            SetupUI();
            ShowMainMenu();
        }

        /// <summary>
        /// Initialize game state and collections
        /// </summary>
        private void InitializeGame()
        {
            Focusable = true;
            Focus();
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
        }

        /// <summary>
        /// Create the main UI structure
        /// </summary>
        private void SetupUI()
        {
            Width = 1000;
            Height = 700;
            
            var mainGrid = new Grid();
            
            // Create all canvases
            _starFieldCanvas = new Canvas
            {
                Background = new SolidColorBrush(Color.FromRgb(5, 5, 20)),
                ClipToBounds = true
            };
            
            _gameCanvas = new Canvas
            {
                Background = Brushes.Transparent,
                ClipToBounds = true
            };
            
            _menuCanvas = new Canvas
            {
                Background = new SolidColorBrush(Color.FromArgb(200, 10, 10, 30))
            };
            
            _uiCanvas = new Canvas
            {
                Background = Brushes.Transparent
            };
            
            // Layer canvases
            mainGrid.Children.Add(_starFieldCanvas);
            mainGrid.Children.Add(_gameCanvas);
            mainGrid.Children.Add(_menuCanvas);
            mainGrid.Children.Add(_uiCanvas);
            
            Content = mainGrid;
            
            GenerateStarField();
            CreateUI();
        }

        /// <summary>
        /// Create game UI elements
        /// </summary>
        private void CreateUI()
        {
            _scoreText = new TextBlock
            {
                Text = "Score: 0",
                Foreground = Brushes.Cyan,
                FontSize = 18,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(_scoreText, 10);
            Canvas.SetTop(_scoreText, 10);
            _uiCanvas.Children.Add(_scoreText);
            
            _livesText = new TextBlock
            {
                Text = "Lives: 3",
                Foreground = Brushes.Red,
                FontSize = 18,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(_livesText, 10);
            Canvas.SetTop(_livesText, 40);
            _uiCanvas.Children.Add(_livesText);
            
            _comboText = new TextBlock
            {
                Text = "",
                Foreground = Brushes.Yellow,
                FontSize = 16,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(_comboText, 10);
            Canvas.SetTop(_comboText, 70);
            _uiCanvas.Children.Add(_comboText);
        }

        /// <summary>
        /// Show main menu
        /// </summary>
        private void ShowMainMenu()
        {
            // Stop any running game
            _gameTimer?.Stop();
            _starTimer?.Stop();
            
            _currentState = GameState.MainMenu;
            _menuCanvas.Children.Clear();
            _menuCanvas.Visibility = Visibility.Visible;
            _uiCanvas.Visibility = Visibility.Hidden; // Hide game UI
            
            // Title
            var title = new TextBlock
            {
                Text = "🚀 GALACTIC COMMANDER 🚀",
                FontSize = 36,
                FontWeight = FontWeights.Bold,
                Foreground = new LinearGradientBrush(
                    Color.FromRgb(100, 200, 255),
                    Color.FromRgb(50, 150, 255),
                    90
                ),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Canvas.SetLeft(title, 200);
            Canvas.SetTop(title, 100);
            _menuCanvas.Children.Add(title);
            
            // Menu buttons
            var playButton = CreateMenuButton("▶ PLAY GAME", 400, 250);
            playButton.Click += (s, e) => StartNewGame();
            
            var scoresButton = CreateMenuButton("🏆 HIGH SCORES", 400, 320);
            scoresButton.Click += (s, e) => ShowHighScores();
            
            var settingsButton = CreateMenuButton("⚙ SETTINGS", 400, 390);
            settingsButton.Click += (s, e) => ShowSettings();
            
            var exitButton = CreateMenuButton("❌ EXIT", 400, 460);
            exitButton.Click += (s, e) => Application.Current.Shutdown();
            
            _menuCanvas.Children.Add(playButton);
            _menuCanvas.Children.Add(scoresButton);
            _menuCanvas.Children.Add(settingsButton);
            _menuCanvas.Children.Add(exitButton);
            
            // Instructions
            var instructions = new TextBlock
            {
                Text = "Controls: WASD/Arrows to move, SPACE to fire, ESC for menu",
                FontSize = 14,
                Foreground = Brushes.LightGray,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Canvas.SetLeft(instructions, 250);
            Canvas.SetTop(instructions, 580);
            _menuCanvas.Children.Add(instructions);
        }

        /// <summary>
        /// Create a styled menu button
        /// </summary>
        private Button CreateMenuButton(string text, double x, double y)
        {
            var button = new Button
            {
                Content = text,
                Width = 200,
                Height = 50,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Background = new LinearGradientBrush(
                    Color.FromRgb(50, 100, 150),
                    Color.FromRgb(30, 70, 120),
                    90
                ),
                Foreground = Brushes.White,
                BorderBrush = Brushes.Cyan,
                BorderThickness = new Thickness(2)
            };
            
            Canvas.SetLeft(button, x);
            Canvas.SetTop(button, y);
            
            return button;
        }

        /// <summary>
        /// Show high scores screen
        /// </summary>
        private void ShowHighScores()
        {
            _currentState = GameState.HighScores;
            _menuCanvas.Children.Clear();
            
            var title = new TextBlock
            {
                Text = "🏆 HIGH SCORES 🏆",
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Gold
            };
            Canvas.SetLeft(title, 300);
            Canvas.SetTop(title, 50);
            _menuCanvas.Children.Add(title);
            
            var scores = _highScoreManager.TopScores;
            for (int i = 0; i < Math.Min(scores.Count, 10); i++)
            {
                var score = scores[i];
                var scoreText = new TextBlock
                {
                    Text = $"{i + 1}. {score.PlayerName} - {score.Score:N0} ({score.Accuracy:F1}%)",
                    FontSize = 18,
                    Foreground = i < 3 ? Brushes.Gold : Brushes.White
                };
                Canvas.SetLeft(scoreText, 200);
                Canvas.SetTop(scoreText, 120 + (i * 30));
                _menuCanvas.Children.Add(scoreText);
            }
            
            if (scores.Count == 0)
            {
                var noScores = new TextBlock
                {
                    Text = "No high scores yet! Be the first!",
                    FontSize = 18,
                    Foreground = Brushes.Gray
                };
                Canvas.SetLeft(noScores, 300);
                Canvas.SetTop(noScores, 200);
                _menuCanvas.Children.Add(noScores);
            }
            
            var backButton = CreateMenuButton("← BACK", 400, 550);
            backButton.Click += (s, e) => ShowMainMenu();
            _menuCanvas.Children.Add(backButton);
        }

        /// <summary>
        /// Show settings screen
        /// </summary>
        private void ShowSettings()
        {
            _currentState = GameState.Settings;
            _menuCanvas.Children.Clear();
            
            var title = new TextBlock
            {
                Text = "⚙ SETTINGS ⚙",
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Cyan
            };
            Canvas.SetLeft(title, 350);
            Canvas.SetTop(title, 50);
            _menuCanvas.Children.Add(title);
            
            // Difficulty setting
            var diffLabel = new TextBlock
            {
                Text = "Difficulty:",
                FontSize = 18,
                Foreground = Brushes.White
            };
            Canvas.SetLeft(diffLabel, 200);
            Canvas.SetTop(diffLabel, 150);
            _menuCanvas.Children.Add(diffLabel);
            
            var diffCombo = new ComboBox
            {
                Width = 150,
                SelectedIndex = _settings.Difficulty
            };
            diffCombo.Items.Add("Easy");
            diffCombo.Items.Add("Normal");
            diffCombo.Items.Add("Hard");
            diffCombo.SelectionChanged += (s, e) => _settings.Difficulty = diffCombo.SelectedIndex;
            Canvas.SetLeft(diffCombo, 350);
            Canvas.SetTop(diffCombo, 150);
            _menuCanvas.Children.Add(diffCombo);
            
            // Player name
            var nameLabel = new TextBlock
            {
                Text = "Player Name:",
                FontSize = 18,
                Foreground = Brushes.White
            };
            Canvas.SetLeft(nameLabel, 200);
            Canvas.SetTop(nameLabel, 200);
            _menuCanvas.Children.Add(nameLabel);
            
            var nameBox = new TextBox
            {
                Width = 150,
                Text = _settings.PlayerName
            };
            nameBox.TextChanged += (s, e) => _settings.PlayerName = nameBox.Text;
            Canvas.SetLeft(nameBox, 350);
            Canvas.SetTop(nameBox, 200);
            _menuCanvas.Children.Add(nameBox);
            
            // Save and back buttons
            var saveButton = CreateMenuButton("💾 SAVE", 300, 400);
            saveButton.Click += (s, e) => { _settings.Save(); ShowMainMenu(); };
            _menuCanvas.Children.Add(saveButton);
            
            var backButton = CreateMenuButton("← BACK", 520, 400);
            backButton.Click += (s, e) => ShowMainMenu();
            _menuCanvas.Children.Add(backButton);
        }

        /// <summary>
        /// Start a new game
        /// </summary>
        private void StartNewGame()
        {
            try
            {
                _currentState = GameState.Playing;
                _menuCanvas.Visibility = Visibility.Hidden;
                _uiCanvas.Visibility = Visibility.Visible; // Make sure UI is visible
                _gameStats.Reset();
                
                // Clear game objects
                _gameCanvas.Children.Clear();
                _bullets.Clear();
                _enemies.Clear();
                _explosions.Clear();
                
                // Reset player position
                _playerX = 400; // Center it better
                _playerY = 400;
                
                // Create player ship
                _player = new SpaceshipControl();
                Canvas.SetLeft(_player, _playerX);
                Canvas.SetTop(_player, _playerY);
                _gameCanvas.Children.Add(_player);
                
                UpdateUI();
                StartGameTimers();
                
                // Set focus to this control for keyboard input
                Focusable = true;
                Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting game: {ex.Message}\n\nStack trace: {ex.StackTrace}", 
                    "Game Start Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Return to menu on error
                _currentState = GameState.MainMenu;
                _menuCanvas.Visibility = Visibility.Visible;
                _uiCanvas.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Start game timers
        /// </summary>
        private void StartGameTimers()
        {
            _gameTimer?.Stop();
            _starTimer?.Stop();
            
            _gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // 60 FPS
            };
            _gameTimer.Tick += GameLoop;
            _gameTimer.Start();
            
            _starTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _starTimer.Tick += AnimateStars;
            _starTimer.Start();
        }

        /// <summary>
        /// Main game loop
        /// </summary>
        private void GameLoop(object? sender, EventArgs e)
        {
            if (_currentState != GameState.Playing) return;
            
            UpdatePlayer();
            UpdateBullets();
            UpdateEnemies();
            UpdateExplosions();
            SpawnEnemies();
            CheckCollisions();
            UpdateUI();
            
            // Update player ship animation
            _player.UpdateAnimation();
            
            // Handle invulnerability
            if (_isInvulnerable && DateTime.Now - _invulnerabilityStartTime > TimeSpan.FromSeconds(2))
            {
                _isInvulnerable = false;
                _player.ShowShield(false);
            }
        }

        /// <summary>
        /// Update player movement and effects
        /// </summary>
        private void UpdatePlayer()
        {
            const double speed = 6.0;
            bool moving = false;
            
            if (_keys[(int)Key.A] || _keys[(int)Key.Left])
            {
                _playerX = Math.Max(0, _playerX - speed);
                moving = true;
            }
            if (_keys[(int)Key.D] || _keys[(int)Key.Right])
            {
                _playerX = Math.Min(960, _playerX + speed);
                moving = true;
            }
            if (_keys[(int)Key.W] || _keys[(int)Key.Up])
            {
                _playerY = Math.Max(0, _playerY - speed);
                moving = true;
            }
            if (_keys[(int)Key.S] || _keys[(int)Key.Down])
            {
                _playerY = Math.Min(650, _playerY + speed);
                moving = true;
            }
            
            Canvas.SetLeft(_player, _playerX);
            Canvas.SetTop(_player, _playerY);
            _player.SetThruster(moving);
        }

        /// <summary>
        /// Update bullets
        /// </summary>
        private void UpdateBullets()
        {
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                var bullet = _bullets[i];
                var currentY = Canvas.GetTop(bullet);
                currentY -= 12; // Bullet speed
                
                if (currentY < -10)
                {
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
        /// Update enemies with difficulty scaling
        /// </summary>
        private void UpdateEnemies()
        {
            var speed = 2.0 + (_settings.Difficulty * 1.0);
            
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                var enemy = _enemies[i];
                var currentY = Canvas.GetTop(enemy);
                currentY += speed;
                
                if (currentY > 710)
                {
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
        /// Update explosion effects
        /// </summary>
        private void UpdateExplosions()
        {
            for (int i = _explosions.Count - 1; i >= 0; i--)
            {
                var explosion = _explosions[i];
                // Explosions handle their own animation and will fire completion event
            }
        }

        /// <summary>
        /// Spawn enemies based on difficulty
        /// </summary>
        private void SpawnEnemies()
        {
            var spawnRate = 0.015 + (_settings.Difficulty * 0.01);
            
            if (_random.NextDouble() < spawnRate)
            {
                var enemy = new Rectangle
                {
                    Width = 20,
                    Height = 20,
                    Fill = new RadialGradientBrush(
                        Color.FromRgb(255, 100, 100),
                        Color.FromRgb(200, 0, 0)
                    ),
                    Stroke = Brushes.DarkRed,
                    StrokeThickness = 2
                };
                
                Canvas.SetLeft(enemy, _random.NextDouble() * 980);
                Canvas.SetTop(enemy, -20);
                _gameCanvas.Children.Add(enemy);
                _enemies.Add(enemy);
            }
        }

        /// <summary>
        /// Check all collision types
        /// </summary>
        private void CheckCollisions()
        {
            CheckBulletEnemyCollisions();
            CheckPlayerEnemyCollisions();
        }

        /// <summary>
        /// Check bullet vs enemy collisions
        /// </summary>
        private void CheckBulletEnemyCollisions()
        {
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
                        // Create explosion
                        var explosion = new ExplosionEffect(
                            Canvas.GetLeft(enemy) + enemy.Width / 2,
                            Canvas.GetTop(enemy) + enemy.Height / 2
                        );
                        explosion.ExplosionComplete += (s, e) => {
                            _gameCanvas.Children.Remove(explosion);
                            _explosions.Remove(explosion);
                        };
                        _gameCanvas.Children.Add(explosion);
                        _explosions.Add(explosion);
                        
                        // Remove bullet and enemy
                        _gameCanvas.Children.Remove(bullet);
                        _gameCanvas.Children.Remove(enemy);
                        _bullets.RemoveAt(i);
                        _enemies.RemoveAt(j);
                        
                        // Update score and stats
                        _gameStats.EnemiesKilled++;
                        _gameStats.IncrementCombo();
                        _gameStats.AddScore(100);
                        
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Check player vs enemy collisions (death mechanic)
        /// </summary>
        private void CheckPlayerEnemyCollisions()
        {
            if (_isInvulnerable) return;
            
            var playerRect = new Rect(_playerX, _playerY, _player.Width, _player.Height);
            
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                var enemy = _enemies[i];
                var enemyRect = new Rect(
                    Canvas.GetLeft(enemy), Canvas.GetTop(enemy),
                    enemy.Width, enemy.Height
                );
                
                if (playerRect.IntersectsWith(enemyRect))
                {
                    // Player hit!
                    _gameCanvas.Children.Remove(enemy);
                    _enemies.RemoveAt(i);
                    
                    PlayerHit();
                    break;
                }
            }
        }

        /// <summary>
        /// Handle player being hit
        /// </summary>
        private void PlayerHit()
        {
            _gameStats.Lives--;
            _gameStats.ResetCombo();
            _player.ShowDamage();
            
            if (_gameStats.Lives <= 0)
            {
                GameOver();
            }
            else
            {
                // Temporary invulnerability
                _isInvulnerable = true;
                _invulnerabilityStartTime = DateTime.Now;
                _player.ShowShield(true);
            }
        }

        /// <summary>
        /// Handle game over
        /// </summary>
        private void GameOver()
        {
            _gameTimer?.Stop();
            _starTimer?.Stop();
            _currentState = GameState.GameOver;
            
            // Check for high score
            if (_highScoreManager.IsHighScore(_gameStats.CurrentScore))
            {
                var entry = new HighScoreEntry
                {
                    PlayerName = _settings.PlayerName,
                    Score = _gameStats.CurrentScore,
                    Date = DateTime.Now,
                    PlayTime = _gameStats.PlayTime,
                    EnemiesKilled = _gameStats.EnemiesKilled,
                    ShotsFired = _gameStats.ShotsFired
                };
                _highScoreManager.AddScore(entry);
            }
            
            ShowGameOverScreen();
        }

        /// <summary>
        /// Show game over screen
        /// </summary>
        private void ShowGameOverScreen()
        {
            _currentState = GameState.GameOver;
            _menuCanvas.Children.Clear();
            _menuCanvas.Visibility = Visibility.Visible;
            _uiCanvas.Visibility = Visibility.Hidden; // Hide game UI
            
            var title = new TextBlock
            {
                Text = "💥 GAME OVER 💥",
                FontSize = 36,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Red
            };
            Canvas.SetLeft(title, 300);
            Canvas.SetTop(title, 100);
            _menuCanvas.Children.Add(title);
            
            var scoreText = new TextBlock
            {
                Text = $"Final Score: {_gameStats.CurrentScore:N0}",
                FontSize = 24,
                Foreground = Brushes.Yellow
            };
            Canvas.SetLeft(scoreText, 350);
            Canvas.SetTop(scoreText, 180);
            _menuCanvas.Children.Add(scoreText);
            
            var statsText = new TextBlock
            {
                Text = $"Enemies Killed: {_gameStats.EnemiesKilled}\n" +
                       $"Accuracy: {_gameStats.Accuracy:F1}%\n" +
                       $"Play Time: {_gameStats.PlayTime:mm\\:ss}\n" +
                       $"Max Combo: {_gameStats.MaxCombo}",
                FontSize = 16,
                Foreground = Brushes.White
            };
            Canvas.SetLeft(statsText, 400);
            Canvas.SetTop(statsText, 220);
            _menuCanvas.Children.Add(statsText);
            
            var playAgainButton = CreateMenuButton("🔄 PLAY AGAIN", 300, 400);
            playAgainButton.Click += (s, e) => {
                try
                {
                    StartNewGame();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error in Play Again: {ex.Message}", "Error");
                }
            };
            _menuCanvas.Children.Add(playAgainButton);
            
            var menuButton = CreateMenuButton("📋 MAIN MENU", 520, 400);
            menuButton.Click += (s, e) => {
                try
                {
                    ShowMainMenu();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error in Main Menu: {ex.Message}", "Error");
                }
            };
            _menuCanvas.Children.Add(menuButton);
        }

        /// <summary>
        /// Update UI elements
        /// </summary>
        private void UpdateUI()
        {
            _scoreText.Text = $"Score: {_gameStats.CurrentScore:N0}";
            _livesText.Text = $"Lives: {_gameStats.Lives}";
            _comboText.Text = _gameStats.Combo > 0 ? $"Combo: {_gameStats.Combo}x" : "";
        }

        /// <summary>
        /// Generate animated star field
        /// </summary>
        private void GenerateStarField()
        {
            _starFieldCanvas.Children.Clear();
            _starField.Clear();
            
            for (int i = 0; i < 200; i++)
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
                
                Canvas.SetLeft(star, _random.NextDouble() * 1000);
                Canvas.SetTop(star, _random.NextDouble() * 700);
                
                _starFieldCanvas.Children.Add(star);
                _starField.Add(star);
            }
        }

        /// <summary>
        /// Animate star field
        /// </summary>
        private void AnimateStars(object? sender, EventArgs e)
        {
            foreach (var star in _starField)
            {
                var currentTop = Canvas.GetTop(star);
                currentTop += star.Width * 0.5;
                
                if (currentTop > 710)
                {
                    Canvas.SetTop(star, -10);
                    Canvas.SetLeft(star, _random.NextDouble() * 1000);
                }
                else
                {
                    Canvas.SetTop(star, currentTop);
                }
            }
        }

        /// <summary>
        /// Handle key press events
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if ((int)e.Key < _keys.Length)
                _keys[(int)e.Key] = true;
            
            if (e.Key == Key.Space && _currentState == GameState.Playing)
            {
                FireBullet();
            }
            
            if (e.Key == Key.Escape)
            {
                if (_currentState == GameState.Playing)
                {
                    _gameTimer?.Stop();
                    _starTimer?.Stop();
                    ShowMainMenu();
                }
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
        /// Fire a bullet with rate limiting
        /// </summary>
        private void FireBullet()
        {
            var now = DateTime.Now;
            if (now - _lastShotTime < TimeSpan.FromMilliseconds(150)) // Rate limit
                return;
            
            _lastShotTime = now;
            _gameStats.ShotsFired++;
            
            var bullet = new Rectangle
            {
                Width = 4,
                Height = 12,
                Fill = new LinearGradientBrush(
                    Color.FromRgb(255, 255, 0),
                    Color.FromRgb(255, 150, 0),
                    90
                ),
                Stroke = Brushes.Yellow,
                StrokeThickness = 0.5
            };
            
            Canvas.SetLeft(bullet, _playerX + 18); // Center on player
            Canvas.SetTop(bullet, _playerY - 5);
            _gameCanvas.Children.Add(bullet);
            _bullets.Add(bullet);
        }

        /// <summary>
        /// Cleanup on destruction
        /// </summary>
        public void StopGame()
        {
            _gameTimer?.Stop();
            _starTimer?.Stop();
        }
    }
}