using System.Collections.Concurrent;
using System.Numerics;

namespace GalacticCommander.Models
{
    /// <summary>
    /// Advanced Entity Management System using Generics and Concurrent Collections
    /// Demonstrates: Singleton Pattern, Generic Constraints, Thread Safety, LINQ
    /// </summary>
    public interface IEntityManager
    {
        Guid CreateEntity();
        void DestroyEntity(Guid entityId);
        T AddComponent<T>(Guid entityId, T component) where T : class, IComponent;
        T? GetComponent<T>(Guid entityId) where T : class, IComponent;
        IEnumerable<T> GetComponents<T>() where T : class, IComponent;
        IEnumerable<Guid> GetEntitiesWithComponent<T>() where T : class, IComponent;
        bool HasComponent<T>(Guid entityId) where T : class, IComponent;
        bool RemoveComponent<T>(Guid entityId) where T : class, IComponent;
        void Update(float deltaTime);
    }

    /// <summary>
    /// Thread-safe Entity Manager with advanced generic operations
    /// Shows concurrent programming, memory management, and performance optimization
    /// </summary>
    public class EntityManager : IEntityManager
    {
        private static readonly Lazy<EntityManager> _instance = new(() => new EntityManager());
        public static EntityManager Instance => _instance.Value;

        // Thread-safe collections for high-performance concurrent access
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Type, IComponent>> _entities;
        private readonly ConcurrentDictionary<Type, ConcurrentBag<IComponent>> _componentsByType;
        private readonly ConcurrentQueue<Guid> _entitiesToDestroy;

        private EntityManager()
        {
            _entities = new ConcurrentDictionary<Guid, ConcurrentDictionary<Type, IComponent>>();
            _componentsByType = new ConcurrentDictionary<Type, ConcurrentBag<IComponent>>();
            _entitiesToDestroy = new ConcurrentQueue<Guid>();
        }

        /// <summary>
        /// Creates a new entity and returns its unique identifier
        /// Thread-safe entity creation with GUID generation
        /// </summary>
        public Guid CreateEntity()
        {
            var entityId = Guid.NewGuid();
            _entities[entityId] = new ConcurrentDictionary<Type, IComponent>();
            return entityId;
        }

        /// <summary>
        /// Marks an entity for destruction (deferred cleanup for thread safety)
        /// Demonstrates deferred execution and safe resource cleanup
        /// </summary>
        public void DestroyEntity(Guid entityId)
        {
            _entitiesToDestroy.Enqueue(entityId);
        }

        /// <summary>
        /// Adds a component to an entity with generic constraints
        /// Shows advanced generics with where clauses and type safety
        /// </summary>
        public T AddComponent<T>(Guid entityId, T component) where T : class, IComponent
        {
            if (!_entities.ContainsKey(entityId))
                throw new ArgumentException($"Entity {entityId} does not exist");

            component.EntityId = entityId;
            var componentType = typeof(T);

            // Add to entity's component collection
            _entities[entityId][componentType] = component;

            // Add to type-based lookup for fast queries
            _componentsByType.AddOrUpdate(
                componentType,
                new ConcurrentBag<IComponent> { component },
                (key, existing) =>
                {
                    existing.Add(component);
                    return existing;
                });

            return component;
        }

        /// <summary>
        /// Retrieves a component with null-safe generic casting
        /// Demonstrates safe type casting and nullable reference types
        /// </summary>
        public T? GetComponent<T>(Guid entityId) where T : class, IComponent
        {
            return _entities.TryGetValue(entityId, out var components) &&
                   components.TryGetValue(typeof(T), out var component)
                ? component as T
                : null;
        }

        /// <summary>
        /// Gets all components of a specific type using LINQ and safe casting
        /// Shows advanced LINQ operations and performance considerations
        /// </summary>
        public IEnumerable<T> GetComponents<T>() where T : class, IComponent
        {
            return _componentsByType.TryGetValue(typeof(T), out var components)
                ? components.OfType<T>().Where(c => c.IsActive)
                : Enumerable.Empty<T>();
        }

        /// <summary>
        /// Gets entities that have a specific component type
        /// Advanced LINQ query with complex filtering
        /// </summary>
        public IEnumerable<Guid> GetEntitiesWithComponent<T>() where T : class, IComponent
        {
            return _entities
                .Where(kvp => kvp.Value.ContainsKey(typeof(T)) && 
                             kvp.Value[typeof(T)].IsActive)
                .Select(kvp => kvp.Key);
        }

        /// <summary>
        /// Checks if entity has a specific component type
        /// Fast lookup with null safety
        /// </summary>
        public bool HasComponent<T>(Guid entityId) where T : class, IComponent
        {
            return _entities.TryGetValue(entityId, out var components) &&
                   components.ContainsKey(typeof(T)) &&
                   components[typeof(T)].IsActive;
        }

        /// <summary>
        /// Removes a component from an entity
        /// Safe removal with cleanup from type-based collections
        /// </summary>
        public bool RemoveComponent<T>(Guid entityId) where T : class, IComponent
        {
            if (!_entities.TryGetValue(entityId, out var components) ||
                !components.TryRemove(typeof(T), out var component))
                return false;

            // Mark as inactive instead of removing from type collection for performance
            component.IsActive = false;
            return true;
        }

        /// <summary>
        /// Updates the entity system and performs deferred cleanup
        /// Shows proper resource management and performance optimization
        /// </summary>
        public void Update(float deltaTime)
        {
            // Process entity destruction queue
            while (_entitiesToDestroy.TryDequeue(out var entityId))
            {
                if (!_entities.TryRemove(entityId, out var components))
                    continue;

                // Mark all components as inactive
                foreach (var component in components.Values)
                {
                    component.IsActive = false;
                }
            }

            // Clean up inactive components periodically (performance optimization)
            CleanupInactiveComponents();
        }

        /// <summary>
        /// Periodic cleanup of inactive components to prevent memory leaks
        /// Advanced memory management and performance optimization
        /// </summary>
        private void CleanupInactiveComponents()
        {
            // Only clean up every few frames to avoid performance impact
            if (DateTime.Now.Millisecond % 100 != 0) return;

            foreach (var (type, componentBag) in _componentsByType.ToList())
            {
                var activeComponents = componentBag.Where(c => c.IsActive).ToList();
                if (activeComponents.Count != componentBag.Count)
                {
                    _componentsByType[type] = new ConcurrentBag<IComponent>(activeComponents);
                }
            }
        }

        /// <summary>
        /// Advanced query system for complex entity filtering
        /// Demonstrates functional programming and complex LINQ operations
        /// </summary>
        public IEnumerable<Guid> Query(params Type[] componentTypes)
        {
            return _entities
                .Where(entity => componentTypes.All(type => 
                    entity.Value.ContainsKey(type) && entity.Value[type].IsActive))
                .Select(entity => entity.Key);
        }

        /// <summary>
        /// Get entities within a certain distance from a point
        /// Shows spatial queries and mathematical operations
        /// </summary>
        public IEnumerable<Guid> GetEntitiesInRange(Vector2 center, float range)
        {
            var rangeSquared = range * range;
            
            return GetComponents<TransformComponent>()
                .Where(transform => Vector2.DistanceSquared(transform.Position, center) <= rangeSquared)
                .Select(transform => transform.EntityId);
        }

        /// <summary>
        /// Get the closest entity to a point with a specific component
        /// Advanced spatial queries with generics
        /// </summary>
        public Guid? GetClosestEntity<T>(Vector2 position) where T : class, IComponent
        {
            return GetEntitiesWithComponent<T>()
                .Select(entityId => new { 
                    EntityId = entityId, 
                    Distance = Vector2.DistanceSquared(
                        GetComponent<TransformComponent>(entityId)?.Position ?? Vector2.Zero, 
                        position) 
                })
                .OrderBy(x => x.Distance)
                .FirstOrDefault()?.EntityId;
        }
    }
}