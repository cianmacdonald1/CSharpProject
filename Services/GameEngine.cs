using GalacticCommander.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Numerics;

namespace GalacticCommander.Services
{
    /// <summary>
    /// Advanced Game Engine with Async Game Loop and Performance Monitoring
    /// Demonstrates: Async Programming, High-Resolution Timing, Performance Optimization, Event-Driven Architecture
    /// </summary>
    public interface IGameEngine
    {
        bool IsRunning { get; }
        float DeltaTime { get; }
        float FrameRate { get; }
        TimeSpan TotalGameTime { get; }
        
        Task StartAsync();
        Task StopAsync();
        void Pause();
        void Resume();
        
        event Action<float>? OnUpdate;
        event Action? OnFixedUpdate;
        event Action<float>? OnRender;
    }

    /// <summary>
    /// High-performance game engine with fixed timestep and variable rendering
    /// Shows advanced timing, async patterns, and performance monitoring
    /// </summary>
    public class GameEngine : IGameEngine, IDisposable
    {
        private readonly ILogger<GameEngine> _logger;
        private readonly IPhysicsService _physicsService;
        private readonly IEntityManager _entityManager;
        
        // Advanced timing system
        private readonly Stopwatch _gameTimer = new();
        private readonly Stopwatch _frameTimer = new();
        private CancellationTokenSource? _cancellationTokenSource;
        
        // Performance monitoring
        private readonly Queue<float> _frameTimeHistory = new(60);
        private float _accumulatedTime;
        private int _frameCount;
        private DateTime _lastFpsUpdate = DateTime.Now;
        
        // Game loop configuration
        private const float TargetFrameTime = 1f / 60f; // 60 FPS
        private const float FixedTimeStep = 1f / 120f; // 120 Hz physics
        private const int MaxFrameSkip = 5;

        // State management
        private bool _isRunning;
        private bool _isPaused;
        private TimeSpan _totalGameTime;
        private float _deltaTime;
        private float _frameRate;

        public bool IsRunning => _isRunning;
        public float DeltaTime => _deltaTime;
        public float FrameRate => _frameRate;
        public TimeSpan TotalGameTime => _totalGameTime;

        // Events for game loop phases
        public event Action<float>? OnUpdate;
        public event Action? OnFixedUpdate;
        public event Action<float>? OnRender;

        public GameEngine(
            ILogger<GameEngine> logger,
            IPhysicsService physicsService,
            IEntityManager entityManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _physicsService = physicsService ?? throw new ArgumentNullException(nameof(physicsService));
            _entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
        }

        /// <summary>
        /// Starts the advanced game loop with proper async handling
        /// Demonstrates high-performance game loop architecture
        /// </summary>
        public async Task StartAsync()
        {
            if (_isRunning)
            {
                _logger.LogWarning("Game engine is already running");
                return;
            }

            _logger.LogInformation("Starting game engine...");
            
            _cancellationTokenSource = new CancellationTokenSource();
            _isRunning = true;
            _gameTimer.Start();
            
            try
            {
                await GameLoopAsync(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Game loop cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Game loop error");
                throw;
            }
        }

        /// <summary>
        /// Advanced game loop with fixed timestep physics and variable rendering
        /// Shows sophisticated timing control and performance optimization
        /// </summary>
        private async Task GameLoopAsync(CancellationToken cancellationToken)
        {
            var lastUpdateTime = _gameTimer.Elapsed;
            var physicsAccumulator = 0f;
            
            while (!cancellationToken.IsCancellationRequested)
            {
                _frameTimer.Restart();
                var currentTime = _gameTimer.Elapsed;
                var frameTime = (float)(currentTime - lastUpdateTime).TotalSeconds;
                lastUpdateTime = currentTime;

                // Prevent spiral of death (large frame times)
                frameTime = Math.Min(frameTime, TargetFrameTime * MaxFrameSkip);
                
                if (!_isPaused)
                {
                    _deltaTime = frameTime;
                    _totalGameTime = currentTime;
                    
                    // Update game logic
                    OnUpdate?.Invoke(frameTime);
                    _entityManager.Update(frameTime);
                    
                    // Fixed timestep physics updates
                    physicsAccumulator += frameTime;
                    var physicsSteps = 0;
                    
                    while (physicsAccumulator >= FixedTimeStep && physicsSteps < MaxFrameSkip)
                    {
                        OnFixedUpdate?.Invoke();
                        _physicsService.Update(FixedTimeStep);
                        physicsAccumulator -= FixedTimeStep;
                        physicsSteps++;
                    }
                    
                    // Interpolation factor for smooth rendering
                    var interpolation = physicsAccumulator / FixedTimeStep;
                    OnRender?.Invoke(interpolation);
                }

                // Performance monitoring
                UpdatePerformanceMetrics();
                
                // Yield control to prevent blocking the UI thread
                await Task.Delay(1, cancellationToken);
                
                // Frame rate limiting (if we're running too fast)
                var targetFrameTime = TimeSpan.FromSeconds(TargetFrameTime);
                var actualFrameTime = _frameTimer.Elapsed;
                
                if (actualFrameTime < targetFrameTime)
                {
                    var sleepTime = targetFrameTime - actualFrameTime;
                    if (sleepTime.TotalMilliseconds > 1)
                    {
                        await Task.Delay(sleepTime, cancellationToken);
                    }
                }
            }
        }

        /// <summary>
        /// Performance monitoring and metrics collection
        /// Shows performance analysis and moving averages
        /// </summary>
        private void UpdatePerformanceMetrics()
        {
            var frameTime = (float)_frameTimer.Elapsed.TotalSeconds;
            
            // Track frame time history for smooth FPS calculation
            _frameTimeHistory.Enqueue(frameTime);
            if (_frameTimeHistory.Count > 60)
                _frameTimeHistory.Dequeue();
            
            _frameCount++;
            
            // Update FPS every second
            if (DateTime.Now - _lastFpsUpdate >= TimeSpan.FromSeconds(1))
            {
                // Calculate average frame rate from history
                var averageFrameTime = _frameTimeHistory.Average();
                _frameRate = averageFrameTime > 0 ? 1f / averageFrameTime : 0f;
                
                _logger.LogDebug("FPS: {FrameRate:F1}, Frame Time: {FrameTime:F3}ms", 
                    _frameRate, averageFrameTime * 1000);
                
                _lastFpsUpdate = DateTime.Now;
            }
        }

        /// <summary>
        /// Graceful shutdown with proper resource cleanup
        /// </summary>
        public async Task StopAsync()
        {
            if (!_isRunning) return;
            
            _logger.LogInformation("Stopping game engine...");
            
            _cancellationTokenSource?.Cancel();
            _isRunning = false;
            _gameTimer.Stop();
            
            // Wait a bit for the game loop to finish
            await Task.Delay(100);
            
            _logger.LogInformation("Game engine stopped");
        }

        public void Pause()
        {
            if (_isPaused) return;
            
            _isPaused = true;
            _logger.LogInformation("Game paused");
        }

        public void Resume()
        {
            if (!_isPaused) return;
            
            _isPaused = false;
            _logger.LogInformation("Game resumed");
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _gameTimer?.Stop();
            _frameTimer?.Stop();
        }
    }

    /// <summary>
    /// Advanced Physics Service with Collision Detection and Spatial Optimization
    /// Demonstrates: Spatial Partitioning, Advanced Mathematics, Performance Optimization
    /// </summary>
    public interface IPhysicsService
    {
        void Update(float deltaTime);
        bool CheckCollision(PhysicsComponent a, PhysicsComponent b);
        CollisionInfo? GetCollisionInfo(PhysicsComponent a, PhysicsComponent b);
        IEnumerable<PhysicsComponent> GetNearbyObjects(Vector2 position, float radius);
    }

    public class PhysicsService : IPhysicsService
    {
        private readonly IEntityManager _entityManager;
        private readonly ILogger<PhysicsService> _logger;
        
        // Spatial partitioning grid for collision optimization
        private readonly Dictionary<(int, int), List<PhysicsComponent>> _spatialGrid = new();
        private const float GridCellSize = 64f;

        public PhysicsService(IEntityManager entityManager, ILogger<PhysicsService> logger)
        {
            _entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Advanced physics update with spatial partitioning optimization
        /// Shows performance optimization through spatial data structures
        /// </summary>
        public void Update(float deltaTime)
        {
            // Clear spatial grid
            _spatialGrid.Clear();
            
            // Get all physics components
            var physicsComponents = _entityManager.GetComponents<PhysicsComponent>().ToList();
            
            // Update physics and populate spatial grid
            foreach (var physics in physicsComponents)
            {
                UpdatePhysicsComponent(physics, deltaTime);
                AddToSpatialGrid(physics);
            }
            
            // Efficient collision detection using spatial grid
            DetectCollisions();
        }

        /// <summary>
        /// Advanced physics integration using Verlet integration
        /// Shows numerical integration methods and physics simulation
        /// </summary>
        private void UpdatePhysicsComponent(PhysicsComponent physics, float deltaTime)
        {
            var transform = _entityManager.GetComponent<TransformComponent>(physics.EntityId);
            if (transform == null || physics.IsKinematic) return;

            // Apply forces and calculate acceleration
            var acceleration = physics.Acceleration;
            
            // Verlet integration for better stability
            var newVelocity = transform.Velocity + acceleration * deltaTime;
            
            // Apply drag
            newVelocity *= physics.Drag;
            
            // Update position
            var newPosition = transform.Position + newVelocity * deltaTime;
            
            // Update transform
            transform.Velocity = newVelocity;
            transform.Position = newPosition;
            
            // Reset forces
            physics.Force = Vector2.Zero;
        }

        /// <summary>
        /// Spatial grid population for efficient collision detection
        /// Demonstrates spatial data structures and performance optimization
        /// </summary>
        private void AddToSpatialGrid(PhysicsComponent physics)
        {
            var transform = _entityManager.GetComponent<TransformComponent>(physics.EntityId);
            if (transform == null) return;

            var gridX = (int)(transform.Position.X / GridCellSize);
            var gridY = (int)(transform.Position.Y / GridCellSize);
            
            // Add to multiple cells if object spans across them
            var radius = physics.Radius;
            var cellRadius = (int)Math.Ceiling(radius / GridCellSize);
            
            for (int x = gridX - cellRadius; x <= gridX + cellRadius; x++)
            {
                for (int y = gridY - cellRadius; y <= gridY + cellRadius; y++)
                {
                    var key = (x, y);
                    if (!_spatialGrid.ContainsKey(key))
                        _spatialGrid[key] = new List<PhysicsComponent>();
                    
                    _spatialGrid[key].Add(physics);
                }
            }
        }

        /// <summary>
        /// Efficient collision detection using spatial partitioning
        /// Shows advanced collision detection optimization
        /// </summary>
        private void DetectCollisions()
        {
            var checkedPairs = new HashSet<(Guid, Guid)>();
            
            foreach (var cell in _spatialGrid.Values)
            {
                for (int i = 0; i < cell.Count; i++)
                {
                    for (int j = i + 1; j < cell.Count; j++)
                    {
                        var a = cell[i];
                        var b = cell[j];
                        
                        // Avoid duplicate checks
                        var pairKey = a.EntityId.CompareTo(b.EntityId) < 0 
                            ? (a.EntityId, b.EntityId) 
                            : (b.EntityId, a.EntityId);
                        
                        if (checkedPairs.Contains(pairKey)) continue;
                        checkedPairs.Add(pairKey);
                        
                        // Check collision
                        var collisionInfo = GetCollisionInfo(a, b);
                        if (collisionInfo.HasValue)
                        {
                            a.TriggerCollision(b, collisionInfo.Value);
                            
                            // Create reversed collision info for the other object
                            var reversedInfo = new CollisionInfo(
                                collisionInfo.Value.ContactPoint,
                                -collisionInfo.Value.Normal,
                                collisionInfo.Value.Penetration,
                                -collisionInfo.Value.RelativeVelocity
                            );
                            b.TriggerCollision(a, reversedInfo);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Advanced collision detection with detailed collision information
        /// Shows mathematical collision detection and resolution
        /// </summary>
        public CollisionInfo? GetCollisionInfo(PhysicsComponent a, PhysicsComponent b)
        {
            var transformA = _entityManager.GetComponent<TransformComponent>(a.EntityId);
            var transformB = _entityManager.GetComponent<TransformComponent>(b.EntityId);
            
            if (transformA == null || transformB == null) return null;
            
            var distance = Vector2.Distance(transformA.Position, transformB.Position);
            var minDistance = a.Radius + b.Radius;
            
            if (distance >= minDistance) return null;
            
            // Calculate collision details
            var direction = Vector2.Normalize(transformB.Position - transformA.Position);
            if (direction.LengthSquared() == 0) direction = Vector2.UnitX; // Handle zero vector
            
            var penetration = minDistance - distance;
            var contactPoint = transformA.Position + direction * a.Radius;
            var relativeVelocity = Vector2.Dot(transformB.Velocity - transformA.Velocity, direction);
            
            return new CollisionInfo(contactPoint, direction, penetration, relativeVelocity);
        }

        public bool CheckCollision(PhysicsComponent a, PhysicsComponent b)
        {
            return GetCollisionInfo(a, b).HasValue;
        }

        /// <summary>
        /// Spatial query for nearby objects
        /// </summary>
        public IEnumerable<PhysicsComponent> GetNearbyObjects(Vector2 position, float radius)
        {
            var gridX = (int)(position.X / GridCellSize);
            var gridY = (int)(position.Y / GridCellSize);
            var cellRadius = (int)Math.Ceiling(radius / GridCellSize);
            
            var nearbyObjects = new HashSet<PhysicsComponent>();
            
            for (int x = gridX - cellRadius; x <= gridX + cellRadius; x++)
            {
                for (int y = gridY - cellRadius; y <= gridY + cellRadius; y++)
                {
                    if (_spatialGrid.TryGetValue((x, y), out var cell))
                    {
                        foreach (var obj in cell)
                        {
                            var transform = _entityManager.GetComponent<TransformComponent>(obj.EntityId);
                            if (transform != null && 
                                Vector2.Distance(transform.Position, position) <= radius)
                            {
                                nearbyObjects.Add(obj);
                            }
                        }
                    }
                }
            }
            
            return nearbyObjects;
        }
    }
}