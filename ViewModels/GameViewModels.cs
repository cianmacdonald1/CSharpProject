using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GalacticCommander.Services;
using GalacticCommander.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using System.Numerics;

namespace GalacticCommander.ViewModels
{
    /// <summary>
    /// Advanced MVVM ViewModels with Modern C# Patterns
    /// Demonstrates: MVVM Pattern, Command Pattern, Observable Collections, Data Binding, Reactive Programming
    /// </summary>
    
    /// <summary>
    /// Main application ViewModel with advanced navigation and state management
    /// Shows comprehensive MVVM architecture with dependency injection
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly Services.IGameStateService _gameStateService;
        private readonly ILogger<MainViewModel> _logger;
        private readonly GameViewModel _gameViewModel;
        private readonly MenuViewModel _menuViewModel;

        [ObservableProperty]
        private string _title = "Galactic Commander - Advanced C# Space Game";

        [ObservableProperty]
        private ObservableObject? _currentViewModel;

        [ObservableProperty]
        private bool _isGameRunning;

        [ObservableProperty]
        private GameState _currentGameState;

        public MainViewModel(
            Services.IGameStateService gameStateService,
            ILogger<MainViewModel> logger,
            GameViewModel gameViewModel,
            MenuViewModel menuViewModel)
        {
            _gameStateService = gameStateService ?? throw new ArgumentNullException(nameof(gameStateService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gameViewModel = gameViewModel ?? throw new ArgumentNullException(nameof(gameViewModel));
            _menuViewModel = menuViewModel ?? throw new ArgumentNullException(nameof(menuViewModel));

            // Subscribe to state changes
            _gameStateService.OnStateChanged += OnGameStateChanged;
            CurrentGameState = _gameStateService.CurrentState;
            
            // Initialize with menu
            NavigateToMenu();
        }

        /// <summary>
        /// Advanced state-based navigation with proper cleanup
        /// Shows state management and view switching
        /// </summary>
        private void OnGameStateChanged(GameState previousState, GameState newState)
        {
            CurrentGameState = newState;
            
            switch (newState)
            {
                case GameState.MainMenu:
                    NavigateToMenu();
                    break;
                case GameState.Playing:
                case GameState.Paused:
                    NavigateToGame();
                    break;
                case GameState.GameOver:
                    // Handle game over
                    break;
            }
            
            _logger.LogInformation("Navigated from {Previous} to {Current}", previousState, newState);
        }

        [RelayCommand]
        private void NavigateToGame()
        {
            CurrentViewModel = _gameViewModel;
            IsGameRunning = true;
        }

        [RelayCommand]
        private void NavigateToMenu()
        {
            CurrentViewModel = _menuViewModel;
            IsGameRunning = false;
        }

        [RelayCommand]
        private async Task StartNewGame()
        {
            await _gameViewModel.StartNewGameAsync();
            _gameStateService.ChangeState(GameState.Playing);
        }

        [RelayCommand]
        private void ExitGame()
        {
            _logger.LogInformation("Exiting game");
            System.Windows.Application.Current.Shutdown();
        }
    }

    /// <summary>
    /// Comprehensive Game ViewModel with Real-time Data Binding
    /// Shows advanced data binding, performance optimization, and game state management
    /// </summary>
    public partial class GameViewModel : ObservableObject, IDisposable
    {
        private readonly Services.IGameEngine _gameEngine;
        private readonly IEntityManager _entityManager;
        private readonly Services.IInputService _inputService;
        private readonly Services.IScoreService _scoreService;
        private readonly IEntityFactory _entityFactory;
        private readonly Services.IParticleSystem _particleSystem;
        private readonly ILogger<GameViewModel> _logger;

        // Observable collections for real-time UI updates
        [ObservableProperty]
        private ObservableCollection<GameEntityViewModel> _gameEntities = new();

        [ObservableProperty]
        private ObservableCollection<ParticleViewModel> _particles = new();

        // Game state properties
        [ObservableProperty]
        private int _currentScore;

        [ObservableProperty]
        private int _highScore;

        [ObservableProperty]
        private int _lives = 3;

        [ObservableProperty]
        private float _playerHealth = 100f;

        [ObservableProperty]
        private float _playerShield = 0f;

        [ObservableProperty]
        private string _gameStatus = "Ready";

        [ObservableProperty]
        private bool _isPaused;

        [ObservableProperty]
        private TimeSpan _playTime;

        [ObservableProperty]
        private float _frameRate;

        // Player entity reference
        private Guid _playerId;
        private System.Windows.Threading.DispatcherTimer? _uiUpdateTimer;

        public GameViewModel(
            Services.IGameEngine gameEngine,
            IEntityManager entityManager,
            Services.IInputService inputService,
            Services.IScoreService scoreService,
            IEntityFactory entityFactory,
            Services.IParticleSystem particleSystem,
            ILogger<GameViewModel> logger)
        {
            _gameEngine = gameEngine ?? throw new ArgumentNullException(nameof(gameEngine));
            _entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
            _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
            _scoreService = scoreService ?? throw new ArgumentNullException(nameof(scoreService));
            _entityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
            _particleSystem = particleSystem ?? throw new ArgumentNullException(nameof(particleSystem));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            InitializeGame();
        }

        /// <summary>
        /// Advanced game initialization with event binding and timer setup
        /// Shows proper resource initialization and event handling
        /// </summary>
        private void InitializeGame()
        {
            // Bind to game engine events
            _gameEngine.OnUpdate += OnGameUpdate;
            _gameEngine.OnRender += OnGameRender;

            // Bind to score service events
            _scoreService.OnScoreChanged += OnScoreChanged;
            _scoreService.OnHighScoreChanged += score => HighScore = score;

            // Setup input commands
            SetupInputCommands();

            // UI update timer for smooth property updates
            _uiUpdateTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS UI updates
            };
            _uiUpdateTimer.Tick += OnUIUpdate;
            _uiUpdateTimer.Start();

            _logger.LogInformation("Game initialized");
        }

        /// <summary>
        /// Advanced input command setup with key binding
        /// Shows command pattern implementation
        /// </summary>
        private void SetupInputCommands()
        {
            _inputService.RegisterCommand(System.Windows.Input.Key.Space, new RelayCommand(FireWeapon));
            _inputService.RegisterCommand(System.Windows.Input.Key.P, new RelayCommand(TogglePause));
            _inputService.RegisterCommand(System.Windows.Input.Key.Escape, new RelayCommand(PauseGame));
        }

        /// <summary>
        /// Starts a new game with proper initialization
        /// Shows async game state management
        /// </summary>
        public async Task StartNewGameAsync()
        {
            _logger.LogInformation("Starting new game");
            
            // Clear existing entities
            GameEntities.Clear();
            Particles.Clear();
            
            // Reset game state
            _scoreService.ResetScore();
            Lives = 3;
            GameStatus = "Playing";
            
            // Create player
            _playerId = _entityFactory.CreatePlayer(new Vector2(400, 300));
            
            // Start game engine
            await _gameEngine.StartAsync();
            
            // Spawn initial enemies
            SpawnInitialEnemies();
        }

        /// <summary>
        /// Game loop update handler with entity management
        /// Shows high-performance game loop integration with MVVM
        /// </summary>
        private void OnGameUpdate(float deltaTime)
        {
            if (IsPaused) return;

            // Update play time
            PlayTime = _gameEngine.TotalGameTime;

            // Update player input
            HandlePlayerInput(deltaTime);

            // Update AI systems
            UpdateAISystems(deltaTime);

            // Update game systems
            UpdateGameSystems(deltaTime);

            // Check win/lose conditions
            CheckGameConditions();
        }

        /// <summary>
        /// Advanced player input handling with physics integration
        /// Shows real-time input processing and physics application
        /// </summary>
        private void HandlePlayerInput(float deltaTime)
        {
            var playerTransform = _entityManager.GetComponent<TransformComponent>(_playerId);
            var playerControl = _entityManager.GetComponent<PlayerControlComponent>(_playerId);
            var playerPhysics = _entityManager.GetComponent<PhysicsComponent>(_playerId);
            
            if (playerTransform == null || playerControl == null || playerPhysics == null) return;

            var inputForce = Vector2.Zero;
            
            // Movement input
            if (_inputService.IsKeyDown(System.Windows.Input.Key.W))
                inputForce.Y -= 1;
            if (_inputService.IsKeyDown(System.Windows.Input.Key.S))
                inputForce.Y += 1;
            if (_inputService.IsKeyDown(System.Windows.Input.Key.A))
                inputForce.X -= 1;
            if (_inputService.IsKeyDown(System.Windows.Input.Key.D))
                inputForce.X += 1;

            // Normalize and apply force
            if (inputForce.LengthSquared() > 0)
            {
                inputForce = Vector2.Normalize(inputForce) * playerControl.Speed;
                playerPhysics.Force = inputForce;
            }

            // Rotation
            var mousePos = _inputService.MousePosition;
            var direction = Vector2.Normalize(mousePos - playerTransform.Position);
            if (direction.LengthSquared() > 0)
            {
                playerTransform.Rotation = (float)Math.Atan2(direction.Y, direction.X);
            }
        }

        /// <summary>
        /// AI system updates with complex behavior patterns
        /// Shows advanced AI processing and decision making
        /// </summary>
        private void UpdateAISystems(float deltaTime)
        {
            var aiComponents = _entityManager.GetComponents<AIComponent>();
            var playerTransform = _entityManager.GetComponent<TransformComponent>(_playerId);
            
            foreach (var ai in aiComponents)
            {
                var transform = _entityManager.GetComponent<TransformComponent>(ai.EntityId);
                if (transform == null || playerTransform == null) continue;

                // Simple AI state machine
                var distanceToPlayer = Vector2.Distance(transform.Position, playerTransform.Position);
                
                switch (ai.Type)
                {
                    case AIType.Aggressive:
                        UpdateAggressiveAI(ai, transform, playerTransform, distanceToPlayer, deltaTime);
                        break;
                    case AIType.Evasive:
                        UpdateEvasiveAI(ai, transform, playerTransform, distanceToPlayer, deltaTime);
                        break;
                    // Add more AI types...
                }
            }
        }

        /// <summary>
        /// Complex AI behavior implementation
        /// Shows state machine pattern and mathematical AI
        /// </summary>
        private void UpdateAggressiveAI(AIComponent ai, TransformComponent transform, TransformComponent playerTransform, float distance, float deltaTime)
        {
            var physics = _entityManager.GetComponent<PhysicsComponent>(ai.EntityId);
            if (physics == null) return;

            if (distance <= ai.DetectionRange)
            {
                ai.TargetId = _playerId;
                ai.CurrentState = AIState.Pursue;
                
                // Move toward player
                var direction = Vector2.Normalize(playerTransform.Position - transform.Position);
                physics.Force = direction * ai.Speed;
                
                // Attack if in range
                if (distance <= ai.AttackRange)
                {
                    ai.CurrentState = AIState.Attack;
                    var weapon = _entityManager.GetComponent<WeaponComponent>(ai.EntityId);
                    if (weapon?.CanFire == true)
                    {
                        FireEnemyWeapon(ai.EntityId, direction);
                        weapon.Fire();
                    }
                }
            }
        }

        private void UpdateEvasiveAI(AIComponent ai, TransformComponent transform, TransformComponent playerTransform, float distance, float deltaTime)
        {
            var physics = _entityManager.GetComponent<PhysicsComponent>(ai.EntityId);
            if (physics == null) return;

            if (distance <= ai.DetectionRange)
            {
                // Evade player while occasionally attacking
                var directionFromPlayer = Vector2.Normalize(transform.Position - playerTransform.Position);
                var perpendicular = new Vector2(-directionFromPlayer.Y, directionFromPlayer.X);
                
                // Combine evasion with some random movement
                var evasionForce = directionFromPlayer * ai.Speed * 0.7f;
                var randomForce = perpendicular * ai.Speed * 0.3f * (float)Math.Sin(Time.time * 2);
                
                physics.Force = evasionForce + randomForce;
            }
        }

        /// <summary>
        /// Game systems update coordination
        /// Shows system coordination and performance optimization
        /// </summary>
        private void UpdateGameSystems(float deltaTime)
        {
            // Update particle system
            _particleSystem.Update(deltaTime);
            
            // Update projectiles
            UpdateProjectiles(deltaTime);
            
            // Update power-ups
            UpdatePowerUps(deltaTime);
            
            // Spawn enemies periodically
            SpawnEnemies(deltaTime);
            
            // Update UI elements
            UpdatePlayerStatus();
        }

        /// <summary>
        /// Advanced projectile management with lifecycle handling
        /// Shows entity lifecycle management and collision processing
        /// </summary>
        private void UpdateProjectiles(float deltaTime)
        {
            var projectiles = _entityManager.GetComponents<ProjectileComponent>().ToList();
            
            foreach (var projectile in projectiles)
            {
                projectile.Update(deltaTime);
                
                if (projectile.IsExpired)
                {
                    _entityManager.DestroyEntity(projectile.EntityId);
                    continue;
                }
                
                // Check for collisions
                var projectilePhysics = _entityManager.GetComponent<PhysicsComponent>(projectile.EntityId);
                if (projectilePhysics == null) continue;
                
                var nearbyObjects = _entityManager.GetComponents<PhysicsComponent>()
                    .Where(p => p.EntityId != projectile.EntityId && p.EntityId != projectile.OwnerId);
                
                foreach (var target in nearbyObjects)
                {
                    if (_gameEngine is PhysicsService physicsService && 
                        physicsService.CheckCollision(projectilePhysics, target))
                    {
                        // Handle collision
                        HandleProjectileCollision(projectile, target.EntityId);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Collision handling with damage and effects
        /// </summary>
        private void HandleProjectileCollision(ProjectileComponent projectile, Guid targetId)
        {
            var targetHealth = _entityManager.GetComponent<HealthComponent>(targetId);
            var targetTransform = _entityManager.GetComponent<TransformComponent>(targetId);
            
            if (targetHealth != null && targetTransform != null)
            {
                targetHealth.TakeDamage(projectile.Damage);
                
                // Create particle effects
                _particleSystem.EmitParticles(targetTransform.Position, 10, ParticleType.Sparks);
                
                // Add score if enemy was killed
                if (!targetHealth.IsAlive && targetId != _playerId)
                {
                    _scoreService.AddScore(100);
                    _particleSystem.EmitParticles(targetTransform.Position, 20, ParticleType.Explosion);
                    _entityManager.DestroyEntity(targetId);
                }
            }
            
            // Destroy projectile
            _entityManager.DestroyEntity(projectile.EntityId);
        }

        /// <summary>
        /// UI update handler for smooth property changes
        /// Shows performance-optimized UI updates
        /// </summary>
        private void OnUIUpdate(object? sender, EventArgs e)
        {
            // Update frame rate
            FrameRate = _gameEngine.FrameRate;
            
            // Update entity view models for rendering
            UpdateEntityViewModels();
            UpdateParticleViewModels();
        }

        /// <summary>
        /// Efficient entity view model updates
        /// Shows MVVM performance optimization techniques
        /// </summary>
        private void UpdateEntityViewModels()
        {
            var renderComponents = _entityManager.GetComponents<RenderComponent>();
            
            // Clear and rebuild (could be optimized with change tracking)
            GameEntities.Clear();
            
            foreach (var render in renderComponents)
            {
                var transform = _entityManager.GetComponent<TransformComponent>(render.EntityId);
                if (transform != null)
                {
                    GameEntities.Add(new GameEntityViewModel
                    {
                        Position = transform.Position,
                        Rotation = transform.Rotation,
                        Size = render.Size,
                        Color = render.Color,
                        TexturePath = render.TexturePath
                    });
                }
            }
        }

        private void UpdateParticleViewModels()
        {
            Particles.Clear();
            
            foreach (var particle in _particleSystem.GetActiveParticles())
            {
                Particles.Add(new ParticleViewModel
                {
                    Position = particle.Position,
                    Size = particle.Size,
                    Color = particle.Color,
                    Rotation = particle.Rotation
                });
            }
        }

        private void UpdatePowerUps(float deltaTime)
        {
            var powerUps = _entityManager.GetComponents<PowerUpComponent>();
            
            foreach (var powerUp in powerUps)
            {
                var transform = _entityManager.GetComponent<TransformComponent>(powerUp.EntityId);
                if (transform != null)
                {
                    powerUp.Update(deltaTime, transform);
                }
            }
        }

        private void UpdatePlayerStatus()
        {
            var playerHealth = _entityManager.GetComponent<HealthComponent>(_playerId);
            if (playerHealth != null)
            {
                PlayerHealth = playerHealth.CurrentHealth;
                PlayerShield = playerHealth.Shield;
            }
        }

        private void CheckGameConditions()
        {
            // Check if player is dead
            var playerHealth = _entityManager.GetComponent<HealthComponent>(_playerId);
            if (playerHealth?.IsAlive == false)
            {
                Lives--;
                if (Lives <= 0)
                {
                    GameStatus = "Game Over";
                    // Handle game over
                }
                else
                {
                    // Respawn player
                    RespawnPlayer();
                }
            }
        }

        private void RespawnPlayer()
        {
            var playerHealth = _entityManager.GetComponent<HealthComponent>(_playerId);
            var playerTransform = _entityManager.GetComponent<TransformComponent>(_playerId);
            
            if (playerHealth != null && playerTransform != null)
            {
                playerHealth.Heal(playerHealth.MaxHealth);
                playerTransform.Position = new Vector2(400, 300); // Safe respawn position
            }
        }

        private void SpawnInitialEnemies()
        {
            var random = new Random();
            for (int i = 0; i < 5; i++)
            {
                var position = new Vector2(
                    random.Next(100, 700),
                    random.Next(100, 500)
                );
                _entityFactory.CreateEnemy(position, EnemyType.Grunt);
            }
        }

        private void SpawnEnemies(float deltaTime)
        {
            // Periodic enemy spawning logic
        }

        private void OnGameRender(float interpolation)
        {
            // Render interpolation for smooth visuals
        }

        private void OnScoreChanged(int newScore)
        {
            CurrentScore = newScore;
        }

        [RelayCommand]
        private void FireWeapon()
        {
            var playerTransform = _entityManager.GetComponent<TransformComponent>(_playerId);
            var weapon = _entityManager.GetComponent<WeaponComponent>(_playerId);
            
            if (playerTransform != null && weapon?.CanFire == true)
            {
                var direction = new Vector2(
                    (float)Math.Cos(playerTransform.Rotation),
                    (float)Math.Sin(playerTransform.Rotation)
                );
                
                var velocity = direction * weapon.ProjectileSpeed;
                _entityFactory.CreateProjectile(playerTransform.Position, velocity, weapon.ProjectileType, _playerId);
                weapon.Fire();
                
                // Muzzle flash particles
                _particleSystem.EmitParticles(playerTransform.Position, 5, ParticleType.Sparks);
            }
        }

        private void FireEnemyWeapon(Guid enemyId, Vector2 direction)
        {
            var enemyTransform = _entityManager.GetComponent<TransformComponent>(enemyId);
            var weapon = _entityManager.GetComponent<WeaponComponent>(enemyId);
            
            if (enemyTransform != null && weapon != null)
            {
                var velocity = direction * weapon.ProjectileSpeed;
                _entityFactory.CreateProjectile(enemyTransform.Position, velocity, ProjectileType.Bullet, enemyId);
            }
        }

        [RelayCommand]
        private void TogglePause()
        {
            IsPaused = !IsPaused;
            if (IsPaused)
                _gameEngine.Pause();
            else
                _gameEngine.Resume();
        }

        [RelayCommand]
        private void PauseGame()
        {
            IsPaused = true;
            _gameEngine.Pause();
        }

        public void Dispose()
        {
            _uiUpdateTimer?.Stop();
            _gameEngine.OnUpdate -= OnGameUpdate;
            _gameEngine.OnRender -= OnGameRender;
            _scoreService.OnScoreChanged -= OnScoreChanged;
        }
    }

    /// <summary>
    /// Menu ViewModel with navigation and settings
    /// </summary>
    public partial class MenuViewModel : ObservableObject
    {
        private readonly ILogger<MenuViewModel> _logger;

        [ObservableProperty]
        private string _version = "1.0.0";

        [ObservableProperty]
        private bool _soundEnabled = true;

        [ObservableProperty]
        private float _masterVolume = 0.8f;

        public MenuViewModel(ILogger<MenuViewModel> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [RelayCommand]
        private void StartGame()
        {
            _logger.LogInformation("Start game requested from menu");
        }

        [RelayCommand]
        private void ShowSettings()
        {
            _logger.LogInformation("Settings requested");
        }

        [RelayCommand]
        private void ShowHighScores()
        {
            _logger.LogInformation("High scores requested");
        }

        [RelayCommand]
        private void ExitApplication()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }

    /// <summary>
    /// View Models for rendering entities and particles
    /// Shows data transfer objects for UI binding
    /// </summary>
    public class GameEntityViewModel
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Vector2 Size { get; set; }
        public System.Windows.Media.Color Color { get; set; }
        public string TexturePath { get; set; } = string.Empty;
    }

    public class ParticleViewModel
    {
        public Vector2 Position { get; set; }
        public float Size { get; set; }
        public System.Windows.Media.Color Color { get; set; }
        public float Rotation { get; set; }
    }
}