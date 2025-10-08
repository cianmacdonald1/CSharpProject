using System.Numerics;
using System.Windows.Input;
using Microsoft.Extensions.Logging;

namespace GalacticCommander.Services
{
    /// <summary>
    /// Advanced Input Service with Command Pattern and Key Binding System
    /// Demonstrates: Command Pattern, Event Aggregation, Reactive Programming
    /// </summary>
    public interface IInputService
    {
        bool IsKeyPressed(Key key);
        bool IsKeyDown(Key key);
        bool IsKeyUp(Key key);
        Vector2 MousePosition { get; }
        bool IsMouseButtonPressed(System.Windows.Input.MouseButton button);
        
        void RegisterCommand(Key key, ICommand command);
        void UnregisterCommand(Key key);
        void UpdateKeyState(Key key, bool isPressed);
        void UpdateMousePosition(Vector2 position);
        void UpdateMouseButtonState(System.Windows.Input.MouseButton button, bool isPressed);
        
        event Action<Key>? OnKeyPressed;
        event Action<Key>? OnKeyReleased;
        event Action<Vector2>? OnMouseMove;
        event Action<System.Windows.Input.MouseButton>? OnMouseClick;
    }

    /// <summary>
    /// Comprehensive input handling with advanced key state management
    /// Shows state pattern and event-driven architecture
    /// </summary>
    public class InputService : IInputService
    {
        private readonly ILogger<InputService> _logger;
        private readonly Dictionary<Key, bool> _keyStates = new();
        private readonly Dictionary<Key, bool> _previousKeyStates = new();
        private readonly Dictionary<Key, ICommand> _commandBindings = new();
        private readonly Dictionary<System.Windows.Input.MouseButton, bool> _mouseStates = new();
        
        private Vector2 _mousePosition;

        public Vector2 MousePosition => _mousePosition;

        public event Action<Key>? OnKeyPressed;
        public event Action<Key>? OnKeyReleased;
        public event Action<Vector2>? OnMouseMove;
        public event Action<System.Windows.Input.MouseButton>? OnMouseClick;

        public InputService(ILogger<InputService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IsKeyPressed(Key key) => 
            _keyStates.GetValueOrDefault(key, false) && !_previousKeyStates.GetValueOrDefault(key, false);

        public bool IsKeyDown(Key key) => 
            _keyStates.GetValueOrDefault(key, false);

        public bool IsKeyUp(Key key) => 
            !_keyStates.GetValueOrDefault(key, false);

        public bool IsMouseButtonPressed(System.Windows.Input.MouseButton button) => 
            _mouseStates.GetValueOrDefault(button, false);

        public void RegisterCommand(Key key, ICommand command)
        {
            _commandBindings[key] = command;
            _logger.LogDebug("Registered command for key: {Key}", key);
        }

        public void UnregisterCommand(Key key)
        {
            _commandBindings.Remove(key);
            _logger.LogDebug("Unregistered command for key: {Key}", key);
        }

        /// <summary>
        /// Updates input state and processes commands
        /// Call this from the main game loop
        /// </summary>
        public void Update()
        {
            // Store previous states
            _previousKeyStates.Clear();
            foreach (var kvp in _keyStates)
            {
                _previousKeyStates[kvp.Key] = kvp.Value;
            }

            // Process command bindings
            foreach (var kvp in _commandBindings)
            {
                if (IsKeyPressed(kvp.Key) && kvp.Value.CanExecute(null))
                {
                    kvp.Value.Execute(null);
                }
            }
        }

        /// <summary>
        /// Updates key state (call from WPF key event handlers)
        /// </summary>
        public void UpdateKeyState(Key key, bool isPressed)
        {
            var wasPressed = _keyStates.GetValueOrDefault(key, false);
            _keyStates[key] = isPressed;

            if (isPressed && !wasPressed)
                OnKeyPressed?.Invoke(key);
            else if (!isPressed && wasPressed)
                OnKeyReleased?.Invoke(key);
        }

        /// <summary>
        /// Updates mouse state
        /// </summary>
        public void UpdateMousePosition(Vector2 position)
        {
            _mousePosition = position;
            OnMouseMove?.Invoke(position);
        }

        public void UpdateMouseButtonState(System.Windows.Input.MouseButton button, bool isPressed)
        {
            _mouseStates[button] = isPressed;
            if (isPressed)
                OnMouseClick?.Invoke(button);
        }
    }

    /// <summary>
    /// Advanced Audio Service with 3D Spatial Audio and Dynamic Loading
    /// Demonstrates: Resource Management, Async Loading, Performance Optimization
    /// </summary>
    public interface IAudioService
    {
        Task LoadAudioAsync(string name, string filePath);
        void PlaySound(string name, float volume = 1.0f);
        void PlaySoundAtPosition(string name, Vector2 position, float volume = 1.0f);
        void PlayMusic(string name, bool loop = true, float volume = 0.5f);
        void StopMusic();
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSoundVolume(float volume);
    }

    public class AudioService : IAudioService
    {
        private readonly ILogger<AudioService> _logger;
        private readonly Dictionary<string, System.Windows.Media.MediaPlayer> _loadedAudio = new();
        private System.Windows.Media.MediaPlayer? _currentMusic;
        
        private float _masterVolume = 1.0f;
        private float _musicVolume = 0.5f;
        private float _soundVolume = 1.0f;

        public AudioService(ILogger<AudioService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task LoadAudioAsync(string name, string filePath)
        {
            try
            {
                await Task.Run(() =>
                {
                    var player = new System.Windows.Media.MediaPlayer();
                    player.Open(new Uri(filePath, UriKind.RelativeOrAbsolute));
                    _loadedAudio[name] = player;
                });
                
                _logger.LogInformation("Loaded audio: {Name} from {FilePath}", name, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load audio: {Name} from {FilePath}", name, filePath);
            }
        }

        public void PlaySound(string name, float volume = 1.0f)
        {
            if (_loadedAudio.TryGetValue(name, out var player))
            {
                player.Volume = volume * _soundVolume * _masterVolume;
                player.Position = TimeSpan.Zero;
                player.Play();
            }
        }

        public void PlaySoundAtPosition(string name, Vector2 position, float volume = 1.0f)
        {
            // Simple 3D audio simulation based on distance
            // In a real game, you'd use proper 3D audio libraries
            var listenerPosition = Vector2.Zero; // Camera/player position
            var distance = Vector2.Distance(position, listenerPosition);
            var attenuatedVolume = volume / (1 + distance * 0.01f);
            
            PlaySound(name, attenuatedVolume);
        }

        public void PlayMusic(string name, bool loop = true, float volume = 0.5f)
        {
            StopMusic();
            
            if (_loadedAudio.TryGetValue(name, out var player))
            {
                _currentMusic = player;
                _currentMusic.Volume = volume * _musicVolume * _masterVolume;
                
                if (loop)
                {
                    _currentMusic.MediaEnded += (s, e) =>
                    {
                        _currentMusic.Position = TimeSpan.Zero;
                        _currentMusic.Play();
                    };
                }
                
                _currentMusic.Play();
            }
        }

        public void StopMusic()
        {
            _currentMusic?.Stop();
            _currentMusic = null;
        }

        public void SetMasterVolume(float volume)
        {
            _masterVolume = Math.Clamp(volume, 0f, 1f);
            UpdateAllVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Math.Clamp(volume, 0f, 1f);
            if (_currentMusic != null)
                _currentMusic.Volume = _musicVolume * _masterVolume;
        }

        public void SetSoundVolume(float volume)
        {
            _soundVolume = Math.Clamp(volume, 0f, 1f);
        }

        private void UpdateAllVolumes()
        {
            if (_currentMusic != null)
                _currentMusic.Volume = _musicVolume * _masterVolume;
        }
    }

    /// <summary>
    /// Advanced Asset Management System with Async Loading and Caching
    /// Demonstrates: Async Programming, Resource Management, Caching Strategies
    /// </summary>
    public interface IAssetService
    {
        Task<T> LoadAssetAsync<T>(string path) where T : class;
        T? GetAsset<T>(string path) where T : class;
        void UnloadAsset(string path);
        void UnloadAllAssets();
        Task PreloadAssetsAsync(IEnumerable<string> paths);
    }

    public class AssetService : IAssetService
    {
        private readonly ILogger<AssetService> _logger;
        private readonly Dictionary<string, object> _loadedAssets = new();
        private readonly SemaphoreSlim _loadingSemaphore = new(Environment.ProcessorCount);

        public AssetService(ILogger<AssetService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T> LoadAssetAsync<T>(string path) where T : class
        {
            if (_loadedAssets.TryGetValue(path, out var cachedAsset) && cachedAsset is T cached)
            {
                return cached;
            }

            await _loadingSemaphore.WaitAsync();
            try
            {
                // Double-check pattern for thread safety
                if (_loadedAssets.TryGetValue(path, out cachedAsset) && cachedAsset is T stillCached)
                {
                    return stillCached;
                }

                _logger.LogInformation("Loading asset: {Path}", path);
                
                var asset = await LoadAssetFromDiskAsync<T>(path);
                if (asset != null)
                {
                    _loadedAssets[path] = asset;
                }
                
                return asset ?? throw new InvalidOperationException($"Failed to load asset: {path}");
            }
            finally
            {
                _loadingSemaphore.Release();
            }
        }

        private async Task<T?> LoadAssetFromDiskAsync<T>(string path) where T : class
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Simulate asset loading based on type
                    if (typeof(T) == typeof(System.Windows.Media.Imaging.BitmapImage))
                    {
                        var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                        bitmap.EndInit();
                        bitmap.Freeze(); // Make it thread-safe
                        return bitmap as T;
                    }
                    
                    // Add more asset type handlers as needed
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load asset from disk: {Path}", path);
                    return null;
                }
            });
        }

        public T? GetAsset<T>(string path) where T : class
        {
            return _loadedAssets.TryGetValue(path, out var asset) && asset is T typed ? typed : null;
        }

        public void UnloadAsset(string path)
        {
            if (_loadedAssets.Remove(path))
            {
                _logger.LogInformation("Unloaded asset: {Path}", path);
            }
        }

        public void UnloadAllAssets()
        {
            var count = _loadedAssets.Count;
            _loadedAssets.Clear();
            _logger.LogInformation("Unloaded {Count} assets", count);
        }

        public async Task PreloadAssetsAsync(IEnumerable<string> paths)
        {
            var loadTasks = paths.Select(async path =>
            {
                try
                {
                    await LoadAssetAsync<object>(path);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to preload asset: {Path}", path);
                }
            });

            await Task.WhenAll(loadTasks);
            _logger.LogInformation("Completed preloading assets");
        }
    }
}