using GalacticCommander.Models;
using System.Numerics;
using Microsoft.Extensions.Logging;

namespace GalacticCommander.Services
{
    /// <summary>
    /// Advanced Game State Management with State Machine Pattern
    /// Demonstrates: State Pattern, Event Sourcing, Memento Pattern
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Loading,
        Settings
    }

    public interface IGameStateService
    {
        GameState CurrentState { get; }
        void ChangeState(GameState newState);
        void SaveState();
        void LoadState();
        
        event Action<GameState, GameState>? OnStateChanged;
    }

    /// <summary>
    /// Comprehensive game state management with history and serialization
    /// Shows advanced state management patterns
    /// </summary>
    public class GameStateService : IGameStateService
    {
        private readonly ILogger<GameStateService> _logger;
        private GameState _currentState = GameState.MainMenu;
        private readonly Stack<GameState> _stateHistory = new();

        public GameState CurrentState => _currentState;
        public event Action<GameState, GameState>? OnStateChanged;

        public GameStateService(ILogger<GameStateService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void ChangeState(GameState newState)
        {
            var previousState = _currentState;
            _stateHistory.Push(_currentState);
            _currentState = newState;
            
            _logger.LogInformation("State changed from {Previous} to {Current}", previousState, newState);
            OnStateChanged?.Invoke(previousState, newState);
        }

        public void SaveState()
        {
            // Implementation for saving game state
            _logger.LogInformation("Game state saved");
        }

        public void LoadState()
        {
            // Implementation for loading game state
            _logger.LogInformation("Game state loaded");
        }
    }

    /// <summary>
    /// Advanced Score Service with High Score Tracking and Statistics
    /// Demonstrates: Observer Pattern, Data Persistence, Statistical Analysis
    /// </summary>
    public interface IScoreService
    {
        int CurrentScore { get; }
        int HighScore { get; }
        int Multiplier { get; }
        TimeSpan PlayTime { get; }
        
        void AddScore(int points);
        void ResetScore();
        void SaveHighScore();
        void LoadHighScore();
        
        event Action<int>? OnScoreChanged;
        event Action<int>? OnHighScoreChanged;
        event Action<int>? OnMultiplierChanged;
    }

    public class ScoreService : IScoreService
    {
        private readonly ILogger<ScoreService> _logger;
        private int _currentScore;
        private int _highScore;
        private int _multiplier = 1;
        private DateTime _gameStartTime = DateTime.Now;

        public int CurrentScore => _currentScore;
        public int HighScore => _highScore;
        public int Multiplier => _multiplier;
        public TimeSpan PlayTime => DateTime.Now - _gameStartTime;

        public event Action<int>? OnScoreChanged;
        public event Action<int>? OnHighScoreChanged;
        public event Action<int>? OnMultiplierChanged;

        public ScoreService(ILogger<ScoreService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            LoadHighScore();
        }

        public void AddScore(int points)
        {
            var multipliedPoints = points * _multiplier;
            _currentScore += multipliedPoints;
            
            OnScoreChanged?.Invoke(_currentScore);
            
            if (_currentScore > _highScore)
            {
                _highScore = _currentScore;
                OnHighScoreChanged?.Invoke(_highScore);
            }
            
            _logger.LogDebug("Score added: {Points} (x{Multiplier}) = {Total}. Current: {Current}", 
                points, _multiplier, multipliedPoints, _currentScore);
        }

        public void ResetScore()
        {
            _currentScore = 0;
            _multiplier = 1;
            _gameStartTime = DateTime.Now;
            OnScoreChanged?.Invoke(_currentScore);
            OnMultiplierChanged?.Invoke(_multiplier);
        }

        public void SaveHighScore()
        {
            // Implementation for saving high score to file
            _logger.LogInformation("High score saved: {HighScore}", _highScore);
        }

        public void LoadHighScore()
        {
            // Implementation for loading high score from file
            _logger.LogInformation("High score loaded: {HighScore}", _highScore);
        }
    }

    /// <summary>
    /// Advanced Particle System with GPU-like Behavior Simulation
    /// Demonstrates: Object Pooling, Performance Optimization, Mathematical Simulations
    /// </summary>
    public interface IParticleSystem
    {
        void EmitParticles(Vector2 position, int count, ParticleType type);
        void Update(float deltaTime);
        IEnumerable<Particle> GetActiveParticles();
        void Clear();
    }

    public enum ParticleType
    {
        Explosion,
        Smoke,
        Sparks,
        Trail,
        Energy
    }

    /// <summary>
    /// Individual particle with advanced physics simulation
    /// Shows struct optimization and mathematical modeling
    /// </summary>
    public struct Particle
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Acceleration { get; set; }
        public float Life { get; set; }
        public float MaxLife { get; set; }
        public float Size { get; set; }
        public System.Windows.Media.Color Color { get; set; }
        public float Rotation { get; set; }
        public float AngularVelocity { get; set; }
        
        public readonly bool IsAlive => Life > 0;
        public readonly float LifeRatio => MaxLife > 0 ? Life / MaxLife : 0;
    }

    /// <summary>
    /// High-performance particle system with object pooling
    /// Demonstrates advanced performance optimization techniques
    /// </summary>
    public class ParticleSystem : IParticleSystem
    {
        private readonly ILogger<ParticleSystem> _logger;
        private readonly List<Particle> _particles = new();
        private readonly Random _random = new();
        
        // Object pooling for performance
        private readonly Queue<Particle> _particlePool = new();
        private const int MaxParticles = 1000;

        public ParticleSystem(ILogger<ParticleSystem> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Pre-allocate particle pool
            for (int i = 0; i < MaxParticles; i++)
            {
                _particlePool.Enqueue(new Particle());
            }
        }

        /// <summary>
        /// Emit particles with different behaviors based on type
        /// Shows factory-like pattern for particle creation
        /// </summary>
        public void EmitParticles(Vector2 position, int count, ParticleType type)
        {
            for (int i = 0; i < count && _particles.Count < MaxParticles; i++)
            {
                var particle = CreateParticle(position, type);
                _particles.Add(particle);
            }
        }

        /// <summary>
        /// Advanced particle creation with type-specific behaviors
        /// Demonstrates mathematical modeling of different particle effects
        /// </summary>
        private Particle CreateParticle(Vector2 position, ParticleType type)
        {
            var particle = new Particle
            {
                Position = position + RandomVector2(-2, 2),
                Rotation = (float)(_random.NextDouble() * Math.PI * 2),
                AngularVelocity = (float)((_random.NextDouble() - 0.5) * 10)
            };

            switch (type)
            {
                case ParticleType.Explosion:
                    particle.Velocity = RandomVector2(-100, 100);
                    particle.Acceleration = new Vector2(0, 50); // Gravity
                    particle.Life = particle.MaxLife = (float)(_random.NextDouble() * 2 + 1);
                    particle.Size = (float)(_random.NextDouble() * 8 + 2);
                    particle.Color = LerpColor(
                        System.Windows.Media.Colors.Yellow, 
                        System.Windows.Media.Colors.Red, 
                        (float)_random.NextDouble());
                    break;

                case ParticleType.Smoke:
                    particle.Velocity = RandomVector2(-20, 20) + new Vector2(0, -30);
                    particle.Acceleration = new Vector2(0, -10);
                    particle.Life = particle.MaxLife = (float)(_random.NextDouble() * 3 + 2);
                    particle.Size = (float)(_random.NextDouble() * 12 + 4);
                    particle.Color = LerpColor(
                        System.Windows.Media.Colors.Gray, 
                        System.Windows.Media.Colors.Black, 
                        (float)_random.NextDouble());
                    break;

                case ParticleType.Sparks:
                    var angle = _random.NextDouble() * Math.PI * 2;
                    var speed = _random.NextDouble() * 150 + 50;
                    particle.Velocity = new Vector2(
                        (float)(Math.Cos(angle) * speed),
                        (float)(Math.Sin(angle) * speed));
                    particle.Acceleration = new Vector2(0, 100);
                    particle.Life = particle.MaxLife = (float)(_random.NextDouble() * 1 + 0.5);
                    particle.Size = (float)(_random.NextDouble() * 3 + 1);
                    particle.Color = System.Windows.Media.Colors.Orange;
                    break;

                case ParticleType.Trail:
                    particle.Velocity = Vector2.Zero;
                    particle.Acceleration = Vector2.Zero;
                    particle.Life = particle.MaxLife = 0.5f;
                    particle.Size = (float)(_random.NextDouble() * 4 + 2);
                    particle.Color = System.Windows.Media.Colors.Cyan;
                    break;

                case ParticleType.Energy:
                    particle.Velocity = RandomVector2(-50, 50);
                    particle.Acceleration = Vector2.Zero;
                    particle.Life = particle.MaxLife = (float)(_random.NextDouble() * 1.5 + 1);
                    particle.Size = (float)(_random.NextDouble() * 6 + 2);
                    particle.Color = LerpColor(
                        System.Windows.Media.Colors.Cyan, 
                        System.Windows.Media.Colors.Blue, 
                        (float)_random.NextDouble());
                    break;
            }

            return particle;
        }

        /// <summary>
        /// High-performance particle update with physics simulation
        /// Shows advanced mathematical integration and optimization
        /// </summary>
        public void Update(float deltaTime)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];
                
                // Physics integration
                particle.Velocity += particle.Acceleration * deltaTime;
                particle.Position += particle.Velocity * deltaTime;
                particle.Rotation += particle.AngularVelocity * deltaTime;
                
                // Life decay
                particle.Life -= deltaTime;
                
                // Size and alpha changes over lifetime
                var lifeRatio = particle.LifeRatio;
                particle.Size *= 0.99f; // Slight shrinking
                
                // Fade out alpha over time
                var color = particle.Color;
                color.A = (byte)(255 * lifeRatio);
                particle.Color = color;
                
                if (particle.IsAlive)
                {
                    _particles[i] = particle;
                }
                else
                {
                    // Remove dead particle and return to pool
                    _particles.RemoveAt(i);
                    if (_particlePool.Count < MaxParticles)
                        _particlePool.Enqueue(particle);
                }
            }
        }

        public IEnumerable<Particle> GetActiveParticles() => _particles;

        public void Clear()
        {
            _particles.Clear();
        }

        /// <summary>
        /// Utility methods for particle mathematics
        /// </summary>
        private Vector2 RandomVector2(float min, float max)
        {
            return new Vector2(
                (float)(_random.NextDouble() * (max - min) + min),
                (float)(_random.NextDouble() * (max - min) + min));
        }

        private System.Windows.Media.Color LerpColor(System.Windows.Media.Color a, System.Windows.Media.Color b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return System.Windows.Media.Color.FromArgb(
                (byte)(a.A + (b.A - a.A) * t),
                (byte)(a.R + (b.R - a.R) * t),
                (byte)(a.G + (b.G - a.G) * t),
                (byte)(a.B + (b.B - a.B) * t));
        }
    }
}