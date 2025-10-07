using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace GalacticCommander.Models
{
    /// <summary>
    /// Advanced Entity Component System (ECS) Architecture
    /// Demonstrates: Generics, Interfaces, Composition over Inheritance, Observer Pattern
    /// </summary>
    
    /// <summary>
    /// Base interface for all components in the ECS system
    /// Shows interface segregation and generic constraints
    /// </summary>
    public interface IComponent
    {
        Guid EntityId { get; set; }
        bool IsActive { get; set; }
    }

    /// <summary>
    /// Base component class with property change notification
    /// Demonstrates: INotifyPropertyChanged, Generic base classes, Attribute usage
    /// </summary>
    public abstract class ComponentBase : IComponent, INotifyPropertyChanged
    {
        private bool _isActive = true;
        
        public Guid EntityId { get; set; }
        
        public bool IsActive 
        { 
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>
    /// Transform component for position, rotation, and scale
    /// Shows Vector mathematics and property binding
    /// </summary>
    public class TransformComponent : ComponentBase
    {
        private Vector2 _position;
        private float _rotation;
        private Vector2 _scale = Vector2.One;
        private Vector2 _velocity;

        public Vector2 Position 
        { 
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public float Rotation 
        { 
            get => _rotation;
            set => SetProperty(ref _rotation, value);
        }

        public Vector2 Scale 
        { 
            get => _scale;
            set => SetProperty(ref _scale, value);
        }

        public Vector2 Velocity 
        { 
            get => _velocity;
            set => SetProperty(ref _velocity, value);
        }

        /// <summary>
        /// Advanced vector mathematics for forward direction
        /// </summary>
        public Vector2 Forward => new Vector2(
            (float)Math.Cos(Rotation), 
            (float)Math.Sin(Rotation)
        );
    }

    /// <summary>
    /// Render component with advanced graphics properties
    /// Shows complex property relationships and validation
    /// </summary>
    public class RenderComponent : ComponentBase
    {
        private string _texturePath = string.Empty;
        private System.Windows.Media.Color _color = System.Windows.Media.Colors.White;
        private double _opacity = 1.0;
        private Vector2 _size = new(32, 32);
        private int _layer;

        public string TexturePath 
        { 
            get => _texturePath;
            set => SetProperty(ref _texturePath, value ?? string.Empty);
        }

        public System.Windows.Media.Color Color 
        { 
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public double Opacity 
        { 
            get => _opacity;
            set => SetProperty(ref _opacity, Math.Clamp(value, 0.0, 1.0));
        }

        public Vector2 Size 
        { 
            get => _size;
            set => SetProperty(ref _size, value);
        }

        public int Layer 
        { 
            get => _layer;
            set => SetProperty(ref _layer, value);
        }

        /// <summary>
        /// Computed property for bounds checking
        /// </summary>
        public System.Windows.Rect Bounds => new(
            Position.X - Size.X / 2,
            Position.Y - Size.Y / 2,
            Size.X,
            Size.Y
        );

        private Vector2 Position => 
            GalacticCommander.Models.EntityManager.Instance?.GetComponent<TransformComponent>(EntityId)?.Position ?? Vector2.Zero;
    }

    /// <summary>
    /// Physics component with collision detection
    /// Demonstrates advanced physics calculations and event handling
    /// </summary>
    public class PhysicsComponent : ComponentBase
    {
        private float _mass = 1.0f;
        private Vector2 _force;
        private float _drag = 0.98f;
        private bool _isKinematic;
        private float _radius;

        public float Mass 
        { 
            get => _mass;
            set => SetProperty(ref _mass, Math.Max(value, 0.01f));
        }

        public Vector2 Force 
        { 
            get => _force;
            set => SetProperty(ref _force, value);
        }

        public float Drag 
        { 
            get => _drag;
            set => SetProperty(ref _drag, Math.Clamp(value, 0f, 1f));
        }

        public bool IsKinematic 
        { 
            get => _isKinematic;
            set => SetProperty(ref _isKinematic, value);
        }

        public float Radius 
        { 
            get => _radius;
            set => SetProperty(ref _radius, Math.Max(value, 0f));
        }

        /// <summary>
        /// Physics calculation for acceleration
        /// Shows Newton's second law implementation
        /// </summary>
        public Vector2 Acceleration => IsKinematic ? Vector2.Zero : Force / Mass;

        /// <summary>
        /// Event for collision detection
        /// Demonstrates custom event handling with generic constraints
        /// </summary>
        public event Action<PhysicsComponent, CollisionInfo>? OnCollision;

        public void TriggerCollision(PhysicsComponent other, CollisionInfo info)
        {
            OnCollision?.Invoke(other, info);
        }
    }

    /// <summary>
    /// Collision information structure
    /// Shows advanced struct design and mathematical calculations
    /// </summary>
    public readonly struct CollisionInfo
    {
        public Vector2 ContactPoint { get; init; }
        public Vector2 Normal { get; init; }
        public float Penetration { get; init; }
        public float RelativeVelocity { get; init; }

        public CollisionInfo(Vector2 contactPoint, Vector2 normal, float penetration, float relativeVelocity)
        {
            ContactPoint = contactPoint;
            Normal = normal;
            Penetration = penetration;
            RelativeVelocity = relativeVelocity;
        }
    }

    /// <summary>
    /// Health component with damage system
    /// Shows event-driven programming and state management
    /// </summary>
    public class HealthComponent : ComponentBase
    {
        private float _currentHealth;
        private float _maxHealth;
        private float _shield;
        private float _maxShield;
        private DateTime _lastDamageTime;

        public float CurrentHealth 
        { 
            get => _currentHealth;
            private set => SetProperty(ref _currentHealth, Math.Max(0, value));
        }

        public float MaxHealth 
        { 
            get => _maxHealth;
            set 
            { 
                SetProperty(ref _maxHealth, Math.Max(1, value));
                if (CurrentHealth > MaxHealth)
                    CurrentHealth = MaxHealth;
            }
        }

        public float Shield 
        { 
            get => _shield;
            private set => SetProperty(ref _shield, Math.Max(0, value));
        }

        public float MaxShield 
        { 
            get => _maxShield;
            set 
            { 
                SetProperty(ref _maxShield, Math.Max(0, value));
                if (Shield > MaxShield)
                    Shield = MaxShield;
            }
        }

        public bool IsAlive => CurrentHealth > 0;
        public float HealthPercentage => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0;
        public float ShieldPercentage => MaxShield > 0 ? Shield / MaxShield : 0;

        public DateTime LastDamageTime 
        { 
            get => _lastDamageTime;
            private set => SetProperty(ref _lastDamageTime, value);
        }

        public event Action<float>? OnHealthChanged;
        public event Action<float>? OnDamage;
        public event Action? OnDeath;
        public event Action<float>? OnHeal;

        /// <summary>
        /// Advanced damage calculation with shield mechanics
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (damage <= 0 || !IsAlive) return;

            var originalHealth = CurrentHealth;
            var originalShield = Shield;

            // Shield absorbs damage first
            if (Shield > 0)
            {
                var shieldDamage = Math.Min(damage, Shield);
                Shield -= shieldDamage;
                damage -= shieldDamage;
            }

            // Remaining damage affects health
            if (damage > 0)
            {
                CurrentHealth -= damage;
                LastDamageTime = DateTime.Now;
            }

            // Trigger events
            OnDamage?.Invoke(originalHealth + originalShield - CurrentHealth - Shield);
            OnHealthChanged?.Invoke(CurrentHealth);

            if (!IsAlive)
                OnDeath?.Invoke();
        }

        /// <summary>
        /// Healing with overflow to shield
        /// </summary>
        public void Heal(float amount)
        {
            if (amount <= 0) return;

            var originalHealth = CurrentHealth;
            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
            var actualHeal = CurrentHealth - originalHealth;

            // Overflow healing goes to shield
            var overflow = amount - actualHeal;
            if (overflow > 0 && MaxShield > 0)
            {
                Shield = Math.Min(MaxShield, Shield + overflow);
            }

            OnHeal?.Invoke(actualHeal);
            OnHealthChanged?.Invoke(CurrentHealth);
        }

        public HealthComponent(float maxHealth, float maxShield = 0)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            MaxShield = maxShield;
            Shield = maxShield;
        }
    }
}