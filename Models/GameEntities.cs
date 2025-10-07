using GalacticCommander.Models;
using GalacticCommander.Services;
using System.Numerics;
using Microsoft.Extensions.Logging;

namespace GalacticCommander.Models
{
    /// <summary>
    /// Advanced Game Entity Factory with Builder Pattern and Fluent Interface
    /// Demonstrates: Factory Pattern, Builder Pattern, Fluent APIs, Composition over Inheritance
    /// </summary>
    public interface IEntityFactory
    {
        Guid CreatePlayer(Vector2 position);
        Guid CreateEnemy(Vector2 position, EnemyType type);
        Guid CreateProjectile(Vector2 position, Vector2 velocity, ProjectileType type, Guid ownerId);
        Guid CreatePowerUp(Vector2 position, PowerUpType type);
        Guid CreateExplosion(Vector2 position, float scale = 1.0f);
    }

    /// <summary>
    /// Comprehensive entity factory with advanced creation patterns
    /// Shows sophisticated object creation and configuration
    /// </summary>
    public class EntityFactory : IEntityFactory
    {
        private readonly IEntityManager _entityManager;
        private readonly ILogger<EntityFactory> _logger;

        public EntityFactory(IEntityManager entityManager, ILogger<EntityFactory> logger)
        {
            _entityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a player entity with comprehensive component setup
        /// Shows fluent builder-like pattern for entity creation
        /// </summary>
        public Guid CreatePlayer(Vector2 position)
        {
            var entityId = _entityManager.CreateEntity();
            
            // Transform component
            _entityManager.AddComponent(entityId, new TransformComponent
            {
                Position = position,
                Scale = Vector2.One
            });
            
            // Render component
            _entityManager.AddComponent(entityId, new RenderComponent
            {
                TexturePath = "Assets/player_ship.png",
                Size = new Vector2(32, 32),
                Color = System.Windows.Media.Colors.Cyan,
                Layer = 10
            });
            
            // Physics component
            var physics = new PhysicsComponent
            {
                Mass = 1.0f,
                Radius = 16f,
                Drag = 0.95f
            };
            _entityManager.AddComponent(entityId, physics);
            
            // Health component
            var health = new HealthComponent(100f, 50f);
            _entityManager.AddComponent(entityId, health);
            
            // Player-specific components
            _entityManager.AddComponent(entityId, new PlayerControlComponent());
            _entityManager.AddComponent(entityId, new WeaponComponent
            {
                FireRate = 0.2f,
                ProjectileSpeed = 300f,
                Damage = 25f
            });
            
            _logger.LogDebug("Created player entity: {EntityId} at {Position}", entityId, position);
            return entityId;
        }

        /// <summary>
        /// Creates enemy entities with type-specific behaviors
        /// Demonstrates polymorphic entity creation
        /// </summary>
        public Guid CreateEnemy(Vector2 position, EnemyType type)
        {
            var entityId = _entityManager.CreateEntity();
            
            // Base components for all enemies
            _entityManager.AddComponent(entityId, new TransformComponent
            {
                Position = position,
                Scale = Vector2.One
            });
            
            var (size, health, speed, color, aiType) = GetEnemyStats(type);
            
            _entityManager.AddComponent(entityId, new RenderComponent
            {
                TexturePath = $"Assets/enemy_{type.ToString().ToLower()}.png",
                Size = size,
                Color = color,
                Layer = 5
            });
            
            _entityManager.AddComponent(entityId, new PhysicsComponent
            {
                Mass = 1.5f,
                Radius = Math.Max(size.X, size.Y) / 2,
                Drag = 0.98f
            });
            
            _entityManager.AddComponent(entityId, new HealthComponent(health));
            
            _entityManager.AddComponent(entityId, new AIComponent
            {
                Type = aiType,
                Speed = speed,
                DetectionRange = 150f,
                AttackRange = 80f
            });
            
            _entityManager.AddComponent(entityId, new WeaponComponent
            {
                FireRate = GetEnemyFireRate(type),
                ProjectileSpeed = 200f,
                Damage = GetEnemyDamage(type)
            });
            
            _logger.LogDebug("Created {EnemyType} enemy: {EntityId} at {Position}", type, entityId, position);
            return entityId;
        }

        /// <summary>
        /// Creates projectiles with physics and collision detection
        /// Shows advanced projectile physics simulation
        /// </summary>
        public Guid CreateProjectile(Vector2 position, Vector2 velocity, ProjectileType type, Guid ownerId)
        {
            var entityId = _entityManager.CreateEntity();
            
            _entityManager.AddComponent(entityId, new TransformComponent
            {
                Position = position,
                Velocity = velocity,
                Rotation = (float)Math.Atan2(velocity.Y, velocity.X)
            });
            
            var (size, damage, color, lifetime) = GetProjectileStats(type);
            
            _entityManager.AddComponent(entityId, new RenderComponent
            {
                TexturePath = $"Assets/projectile_{type.ToString().ToLower()}.png",
                Size = size,
                Color = color,
                Layer = 8
            });
            
            _entityManager.AddComponent(entityId, new PhysicsComponent
            {
                Mass = 0.1f,
                Radius = Math.Max(size.X, size.Y) / 2,
                IsKinematic = true // Projectiles move at constant velocity
            });
            
            _entityManager.AddComponent(entityId, new ProjectileComponent
            {
                Damage = damage,
                OwnerId = ownerId,
                Lifetime = lifetime,
                Type = type
            });
            
            return entityId;
        }

        /// <summary>
        /// Creates power-ups with special effects and behaviors
        /// </summary>
        public Guid CreatePowerUp(Vector2 position, PowerUpType type)
        {
            var entityId = _entityManager.CreateEntity();
            
            _entityManager.AddComponent(entityId, new TransformComponent
            {
                Position = position
            });
            
            var (size, color, effect) = GetPowerUpStats(type);
            
            _entityManager.AddComponent(entityId, new RenderComponent
            {
                TexturePath = $"Assets/powerup_{type.ToString().ToLower()}.png",
                Size = size,
                Color = color,
                Layer = 6
            });
            
            _entityManager.AddComponent(entityId, new PhysicsComponent
            {
                Mass = 0.5f,
                Radius = Math.Max(size.X, size.Y) / 2,
                Drag = 0.99f
            });
            
            _entityManager.AddComponent(entityId, new PowerUpComponent
            {
                Type = type,
                Effect = effect,
                Duration = GetPowerUpDuration(type)
            });
            
            return entityId;
        }

        /// <summary>
        /// Creates explosion effects with particle systems
        /// </summary>
        public Guid CreateExplosion(Vector2 position, float scale = 1.0f)
        {
            var entityId = _entityManager.CreateEntity();
            
            _entityManager.AddComponent(entityId, new TransformComponent
            {
                Position = position,
                Scale = new Vector2(scale)
            });
            
            _entityManager.AddComponent(entityId, new ExplosionComponent
            {
                MaxRadius = 50f * scale,
                Duration = 1.0f,
                Damage = 50f * scale
            });
            
            return entityId;
        }

        /// <summary>
        /// Enemy statistics configuration
        /// Shows data-driven entity configuration
        /// </summary>
        private (Vector2 size, float health, float speed, System.Windows.Media.Color color, AIType aiType) GetEnemyStats(EnemyType type)
        {
            return type switch
            {
                EnemyType.Grunt => (new Vector2(24, 24), 50f, 80f, System.Windows.Media.Colors.Red, AIType.Aggressive),
                EnemyType.Scout => (new Vector2(20, 20), 30f, 120f, System.Windows.Media.Colors.Orange, AIType.Evasive),
                EnemyType.Heavy => (new Vector2(40, 40), 150f, 40f, System.Windows.Media.Colors.DarkRed, AIType.Tank),
                EnemyType.Boss => (new Vector2(80, 80), 500f, 60f, System.Windows.Media.Colors.Purple, AIType.Boss),
                _ => (new Vector2(24, 24), 50f, 80f, System.Windows.Media.Colors.Red, AIType.Aggressive)
            };
        }

        private float GetEnemyFireRate(EnemyType type) => type switch
        {
            EnemyType.Grunt => 1.0f,
            EnemyType.Scout => 0.8f,
            EnemyType.Heavy => 1.5f,
            EnemyType.Boss => 0.3f,
            _ => 1.0f
        };

        private float GetEnemyDamage(EnemyType type) => type switch
        {
            EnemyType.Grunt => 15f,
            EnemyType.Scout => 10f,
            EnemyType.Heavy => 30f,
            EnemyType.Boss => 50f,
            _ => 15f
        };

        private (Vector2 size, float damage, System.Windows.Media.Color color, float lifetime) GetProjectileStats(ProjectileType type)
        {
            return type switch
            {
                ProjectileType.Bullet => (new Vector2(8, 2), 25f, System.Windows.Media.Colors.Yellow, 3.0f),
                ProjectileType.Laser => (new Vector2(16, 4), 35f, System.Windows.Media.Colors.Cyan, 2.5f),
                ProjectileType.Missile => (new Vector2(12, 6), 60f, System.Windows.Media.Colors.Orange, 4.0f),
                ProjectileType.Plasma => (new Vector2(10, 10), 45f, System.Windows.Media.Colors.Magenta, 2.0f),
                _ => (new Vector2(8, 2), 25f, System.Windows.Media.Colors.Yellow, 3.0f)
            };
        }

        private (Vector2 size, System.Windows.Media.Color color, PowerUpEffect effect) GetPowerUpStats(PowerUpType type)
        {
            return type switch
            {
                PowerUpType.Health => (new Vector2(20, 20), System.Windows.Media.Colors.Green, PowerUpEffect.Heal),
                PowerUpType.Shield => (new Vector2(20, 20), System.Windows.Media.Colors.Blue, PowerUpEffect.Shield),
                PowerUpType.Weapon => (new Vector2(20, 20), System.Windows.Media.Colors.Red, PowerUpEffect.WeaponUpgrade),
                PowerUpType.Speed => (new Vector2(20, 20), System.Windows.Media.Colors.Yellow, PowerUpEffect.SpeedBoost),
                _ => (new Vector2(20, 20), System.Windows.Media.Colors.White, PowerUpEffect.Score)
            };
        }

        private float GetPowerUpDuration(PowerUpType type) => type switch
        {
            PowerUpType.Health => 0f, // Instant
            PowerUpType.Shield => 0f, // Instant
            PowerUpType.Weapon => 15f,
            PowerUpType.Speed => 10f,
            _ => 5f
        };
    }

    /// <summary>
    /// Enumerations for entity types and behaviors
    /// Shows organized data modeling
    /// </summary>
    public enum EnemyType
    {
        Grunt,
        Scout,
        Heavy,
        Boss
    }

    public enum ProjectileType
    {
        Bullet,
        Laser,
        Missile,
        Plasma
    }

    public enum PowerUpType
    {
        Health,
        Shield,
        Weapon,
        Speed,
        Score
    }

    public enum PowerUpEffect
    {
        Heal,
        Shield,
        WeaponUpgrade,
        SpeedBoost,
        Score
    }

    public enum AIType
    {
        Passive,
        Aggressive,
        Evasive,
        Tank,
        Boss
    }

    /// <summary>
    /// Specialized game components for different entity behaviors
    /// Shows component composition for complex behaviors
    /// </summary>
    
    public class PlayerControlComponent : ComponentBase
    {
        public float Speed { get; set; } = 200f;
        public float TurnSpeed { get; set; } = 5f;
    }

    public class WeaponComponent : ComponentBase
    {
        private float _lastFireTime;
        
        public float FireRate { get; set; } = 1.0f; // Shots per second
        public float ProjectileSpeed { get; set; } = 300f;
        public float Damage { get; set; } = 25f;
        public ProjectileType ProjectileType { get; set; } = ProjectileType.Bullet;
        
        public bool CanFire => Time.time - _lastFireTime >= 1f / FireRate;
        
        public void Fire()
        {
            _lastFireTime = Time.time;
        }
    }

    public class AIComponent : ComponentBase
    {
        public AIType Type { get; set; }
        public float Speed { get; set; } = 100f;
        public float DetectionRange { get; set; } = 100f;
        public float AttackRange { get; set; } = 50f;
        public Guid? TargetId { get; set; }
        public Vector2 Direction { get; set; }
        public float StateTimer { get; set; }
        public AIState CurrentState { get; set; } = AIState.Patrol;
    }

    public enum AIState
    {
        Patrol,
        Pursue,
        Attack,
        Evade,
        Idle
    }

    public class ProjectileComponent : ComponentBase
    {
        public float Damage { get; set; }
        public Guid OwnerId { get; set; }
        public float Lifetime { get; set; }
        public ProjectileType Type { get; set; }
        
        private float _timeAlive;
        
        public bool IsExpired => _timeAlive >= Lifetime;
        
        public void Update(float deltaTime)
        {
            _timeAlive += deltaTime;
        }
    }

    public class PowerUpComponent : ComponentBase
    {
        public PowerUpType Type { get; set; }
        public PowerUpEffect Effect { get; set; }
        public float Duration { get; set; }
        public float RotationSpeed { get; set; } = 2f;
        public float BobSpeed { get; set; } = 3f;
        public float BobHeight { get; set; } = 5f;
        
        private float _timeAlive;
        private Vector2 _originalPosition;
        
        public void Update(float deltaTime, TransformComponent transform)
        {
            _timeAlive += deltaTime;
            
            // Floating animation
            if (_originalPosition == Vector2.Zero)
                _originalPosition = transform.Position;
            
            transform.Position = _originalPosition + new Vector2(0, (float)Math.Sin(_timeAlive * BobSpeed) * BobHeight);
            transform.Rotation += RotationSpeed * deltaTime;
        }
    }

    public class ExplosionComponent : ComponentBase
    {
        public float MaxRadius { get; set; }
        public float Duration { get; set; }
        public float Damage { get; set; }
        
        private float _timeAlive;
        
        public float CurrentRadius => (MaxRadius * (_timeAlive / Duration));
        public bool IsFinished => _timeAlive >= Duration;
        
        public void Update(float deltaTime)
        {
            _timeAlive += deltaTime;
        }
    }

    /// <summary>
    /// Simple time utility for game timing
    /// </summary>
    public static class Time
    {
        private static readonly DateTime _startTime = DateTime.Now;
        public static float time => (float)(DateTime.Now - _startTime).TotalSeconds;
    }
}