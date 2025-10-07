using GalacticCommander.Models;
using GalacticCommander.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Text;

namespace GalacticCommander.Services
{
    /// <summary>
    /// Advanced Serialization Service with Reflection and Async I/O
    /// Demonstrates: JSON Serialization, Reflection, Async File I/O, Custom Serialization Logic, Error Handling
    /// </summary>
    public interface ISerializationService
    {
        Task<T?> LoadAsync<T>(string filePath) where T : class;
        Task SaveAsync<T>(T data, string filePath) where T : class;
        Task<GameSaveData?> LoadGameStateAsync(string saveSlot = "default");
        Task SaveGameStateAsync(GameSaveData gameState, string saveSlot = "default");
        Task<List<string>> GetAvailableSavesAsync();
        Task<bool> DeleteSaveAsync(string saveSlot);
    }

    /// <summary>
    /// Comprehensive serialization service with advanced features
    /// Shows modern C# serialization techniques and error handling
    /// </summary>
    public class SerializationService : ISerializationService
    {
        private readonly ILogger<SerializationService> _logger;
        private readonly JsonSerializerSettings _jsonSettings;
        private readonly string _saveDirectory;

        public SerializationService(ILogger<SerializationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Configure JSON serialization settings
            _jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = { new ComponentJsonConverter(), new Vector2JsonConverter() }
            };

            // Setup save directory
            _saveDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GalacticCommander",
                "Saves"
            );

            Directory.CreateDirectory(_saveDirectory);
        }

        /// <summary>
        /// Generic async loading with comprehensive error handling
        /// Shows advanced async I/O and generic constraints
        /// </summary>
        public async Task<T?> LoadAsync<T>(string filePath) where T : class
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("File not found: {FilePath}", filePath);
                    return null;
                }

                _logger.LogInformation("Loading {Type} from {FilePath}", typeof(T).Name, filePath);

                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
                using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
                
                var json = await streamReader.ReadToEndAsync();
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning("Empty or invalid file: {FilePath}", filePath);
                    return null;
                }

                var result = JsonConvert.DeserializeObject<T>(json, _jsonSettings);
                
                _logger.LogInformation("Successfully loaded {Type} from {FilePath}", typeof(T).Name, filePath);
                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error for {FilePath}", filePath);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load {Type} from {FilePath}", typeof(T).Name, filePath);
                return null;
            }
        }

        /// <summary>
        /// Generic async saving with atomic write operations
        /// Shows safe file writing and error recovery
        /// </summary>
        public async Task SaveAsync<T>(T data, string filePath) where T : class
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            try
            {
                _logger.LogInformation("Saving {Type} to {FilePath}", typeof(T).Name, filePath);

                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(data, _jsonSettings);
                var tempFilePath = filePath + ".tmp";

                // Atomic write: write to temp file then move
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    await streamWriter.WriteAsync(json);
                    await streamWriter.FlushAsync();
                }

                // Atomic move
                if (File.Exists(filePath))
                {
                    File.Replace(tempFilePath, filePath, filePath + ".bak");
                }
                else
                {
                    File.Move(tempFilePath, filePath);
                }

                // Cleanup backup
                var backupPath = filePath + ".bak";
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }

                _logger.LogInformation("Successfully saved {Type} to {FilePath}", typeof(T).Name, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save {Type} to {FilePath}", typeof(T).Name, filePath);
                throw;
            }
        }

        /// <summary>
        /// Game-specific save/load operations with metadata
        /// Shows domain-specific serialization logic
        /// </summary>
        public async Task<GameSaveData?> LoadGameStateAsync(string saveSlot = "default")
        {
            var filePath = Path.Combine(_saveDirectory, $"{saveSlot}.save");
            
            var saveData = await LoadAsync<GameSaveData>(filePath);
            if (saveData != null)
            {
                // Validate save data integrity
                if (IsValidSaveData(saveData))
                {
                    _logger.LogInformation("Loaded game state for slot: {SaveSlot}", saveSlot);
                    return saveData;
                }
                else
                {
                    _logger.LogWarning("Save data validation failed for slot: {SaveSlot}", saveSlot);
                    return null;
                }
            }

            return null;
        }

        public async Task SaveGameStateAsync(GameSaveData gameState, string saveSlot = "default")
        {
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            // Add metadata
            gameState.SavedAt = DateTime.UtcNow;
            gameState.Version = GetGameVersion();
            gameState.Checksum = CalculateChecksum(gameState);

            var filePath = Path.Combine(_saveDirectory, $"{saveSlot}.save");
            await SaveAsync(gameState, filePath);
            
            _logger.LogInformation("Saved game state for slot: {SaveSlot}", saveSlot);
        }

        /// <summary>
        /// Advanced file system operations for save management
        /// Shows directory enumeration and file manipulation
        /// </summary>
        public async Task<List<string>> GetAvailableSavesAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!Directory.Exists(_saveDirectory))
                        return new List<string>();

                    return Directory.GetFiles(_saveDirectory, "*.save")
                        .Select(path => Path.GetFileNameWithoutExtension(path))
                        .OrderByDescending(name => File.GetLastWriteTime(Path.Combine(_saveDirectory, name + ".save")))
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get available saves");
                    return new List<string>();
                }
            });
        }

        public async Task<bool> DeleteSaveAsync(string saveSlot)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var filePath = Path.Combine(_saveDirectory, $"{saveSlot}.save");
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        _logger.LogInformation("Deleted save slot: {SaveSlot}", saveSlot);
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete save slot: {SaveSlot}", saveSlot);
                    return false;
                }
            });
        }

        /// <summary>
        /// Save data validation with integrity checking
        /// Shows data validation and security considerations
        /// </summary>
        private bool IsValidSaveData(GameSaveData saveData)
        {
            if (saveData == null) return false;

            // Version compatibility check
            if (saveData.Version != GetGameVersion())
            {
                _logger.LogWarning("Save data version mismatch. Expected: {Expected}, Got: {Actual}", 
                    GetGameVersion(), saveData.Version);
            }

            // Checksum validation
            var calculatedChecksum = CalculateChecksum(saveData);
            if (saveData.Checksum != calculatedChecksum)
            {
                _logger.LogWarning("Save data checksum validation failed");
                return false;
            }

            // Data integrity checks
            return saveData.PlayerData != null && 
                   saveData.GameStats != null &&
                   saveData.SavedAt > DateTime.MinValue;
        }

        /// <summary>
        /// Checksum calculation for data integrity
        /// Shows cryptographic hashing for data validation
        /// </summary>
        private string CalculateChecksum(GameSaveData saveData)
        {
            try
            {
                // Create a copy without checksum for calculation
                var dataForChecksum = new
                {
                    saveData.PlayerData,
                    saveData.GameStats,
                    saveData.Entities,
                    saveData.SavedAt,
                    saveData.Version
                };

                var json = JsonConvert.SerializeObject(dataForChecksum, Formatting.None);
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
                return Convert.ToBase64String(hash);
            }
            catch
            {
                return string.Empty;
            }
        }

        private string GetGameVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        }
    }

    /// <summary>
    /// Comprehensive game save data structure
    /// Shows complex data modeling for serialization
    /// </summary>
    public class GameSaveData
    {
        public PlayerSaveData? PlayerData { get; set; }
        public GameStatsSaveData? GameStats { get; set; }
        public List<EntitySaveData> Entities { get; set; } = new();
        public DateTime SavedAt { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Checksum { get; set; } = string.Empty;
        public Dictionary<string, object> CustomData { get; set; } = new();
    }

    public class PlayerSaveData
    {
        public float Health { get; set; }
        public float Shield { get; set; }
        public int Lives { get; set; }
        public System.Numerics.Vector2 Position { get; set; }
        public Dictionary<string, int> Inventory { get; set; } = new();
        public List<string> UnlockedUpgrades { get; set; } = new();
    }

    public class GameStatsSaveData
    {
        public int Score { get; set; }
        public int HighScore { get; set; }
        public TimeSpan PlayTime { get; set; }
        public int EnemiesDefeated { get; set; }
        public int ShotsFired { get; set; }
        public float Accuracy { get; set; }
        public Dictionary<string, int> Statistics { get; set; } = new();
    }

    public class EntitySaveData
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public System.Numerics.Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Dictionary<string, object> ComponentData { get; set; } = new();
    }

    /// <summary>
    /// Custom JSON converters for complex types
    /// Shows advanced JSON serialization customization
    /// </summary>
    public class ComponentJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IComponent).IsAssignableFrom(objectType);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            // Custom component deserialization logic
            return serializer.Deserialize(reader, objectType);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is IComponent component)
            {
                var data = new
                {
                    Type = component.GetType().Name,
                    Data = component
                };
                serializer.Serialize(writer, data);
            }
        }
    }

    public class Vector2JsonConverter : JsonConverter<System.Numerics.Vector2>
    {
        public override System.Numerics.Vector2 ReadJson(JsonReader reader, Type objectType, System.Numerics.Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var obj = serializer.Deserialize<dynamic>(reader);
                return new System.Numerics.Vector2((float)obj.X, (float)obj.Y);
            }
            return System.Numerics.Vector2.Zero;
        }

        public override void WriteJson(JsonWriter writer, System.Numerics.Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            writer.WriteValue(value.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(value.Y);
            writer.WriteEndObject();
        }
    }
}