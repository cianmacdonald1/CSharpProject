using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace GalacticCommander.Models
{
    /// <summary>
    /// Game state management and settings
    /// Demonstrates: State pattern, Serialization, Configuration management
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        GameOver,
        HighScores,
        Settings,
        Paused
    }

    /// <summary>
    /// Game settings with persistence
    /// Shows: JSON serialization, Configuration pattern, Default values
    /// </summary>
    public class GameSettings
    {
        public float MasterVolume { get; set; } = 0.7f;
        public float SfxVolume { get; set; } = 0.8f;
        public float MusicVolume { get; set; } = 0.6f;
        public int Difficulty { get; set; } = 1; // 0=Easy, 1=Normal, 2=Hard
        public bool ShowFPS { get; set; } = false;
        public bool ParticleEffects { get; set; } = true;
        public string PlayerName { get; set; } = "Player";
        
        private static readonly string SettingsPath = "game_settings.json";
        
        /// <summary>
        /// Save settings to JSON file
        /// Demonstrates: File I/O, Exception handling, Serialization
        /// </summary>
        public void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Load settings from JSON file with fallback to defaults
        /// Shows: Deserialization, Error handling, Factory pattern
        /// </summary>
        public static GameSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonConvert.DeserializeObject<GameSettings>(json) ?? new GameSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load settings, using defaults: {ex.Message}");
            }
            
            return new GameSettings();
        }
    }

    /// <summary>
    /// High score entry with player data
    /// Demonstrates: Data structures, IComparable, DateTime handling
    /// </summary>
    public class HighScoreEntry : IComparable<HighScoreEntry>
    {
        public string PlayerName { get; set; } = "";
        public int Score { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan PlayTime { get; set; }
        public int EnemiesKilled { get; set; }
        public int ShotsFired { get; set; }
        
        /// <summary>
        /// Compare for sorting (highest score first)
        /// Shows: IComparable implementation, Sorting algorithms
        /// </summary>
        public int CompareTo(HighScoreEntry? other)
        {
            return other?.Score.CompareTo(Score) ?? -1;
        }
        
        /// <summary>
        /// Calculate accuracy percentage
        /// Demonstrates: Mathematical calculations, Property patterns
        /// </summary>
        public double Accuracy => ShotsFired > 0 ? (double)EnemiesKilled / ShotsFired * 100 : 0;
    }

    /// <summary>
    /// High score management with persistence
    /// Shows: Collection management, File I/O, LINQ operations
    /// </summary>
    public class HighScoreManager
    {
        private static readonly string ScoresPath = "high_scores.json";
        private List<HighScoreEntry> _scores = new();
        public const int MaxEntries = 10;
        
        /// <summary>
        /// Get top scores sorted by score
        /// Demonstrates: LINQ operations, Collection methods
        /// </summary>
        public IReadOnlyList<HighScoreEntry> TopScores => 
            _scores.OrderByDescending(s => s.Score).Take(MaxEntries).ToList();
        
        /// <summary>
        /// Add new score and manage list size
        /// Shows: Collection manipulation, Sorting, Size limiting
        /// </summary>
        public bool AddScore(HighScoreEntry entry)
        {
            _scores.Add(entry);
            _scores.Sort();
            
            // Keep only top scores
            if (_scores.Count > MaxEntries)
            {
                _scores = _scores.Take(MaxEntries).ToList();
            }
            
            Save();
            
            // Return true if this score made it to the top list
            return TopScores.Contains(entry);
        }
        
        /// <summary>
        /// Check if score qualifies for high score list
        /// Demonstrates: Conditional logic, Collection queries
        /// </summary>
        public bool IsHighScore(int score)
        {
            return _scores.Count < MaxEntries || score > _scores.LastOrDefault()?.Score;
        }
        
        /// <summary>
        /// Save scores to JSON file
        /// Shows: Serialization, Error handling, File operations
        /// </summary>
        public void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_scores, Formatting.Indented);
                File.WriteAllText(ScoresPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save high scores: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Load scores from JSON file
        /// Demonstrates: Deserialization, Exception handling, Default values
        /// </summary>
        public void Load()
        {
            try
            {
                if (File.Exists(ScoresPath))
                {
                    var json = File.ReadAllText(ScoresPath);
                    _scores = JsonConvert.DeserializeObject<List<HighScoreEntry>>(json) ?? new();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load high scores: {ex.Message}");
                _scores = new List<HighScoreEntry>();
            }
        }
        
        /// <summary>
        /// Clear all scores (for testing or reset)
        /// Shows: Collection operations, File management
        /// </summary>
        public void ClearScores()
        {
            _scores.Clear();
            Save();
        }
    }

    /// <summary>
    /// Game statistics tracking
    /// Demonstrates: State tracking, Statistical calculations
    /// </summary>
    public class GameStats
    {
        public int CurrentScore { get; set; }
        public int EnemiesKilled { get; set; }
        public int ShotsFired { get; set; }
        public DateTime GameStartTime { get; set; } = DateTime.Now;
        public int Lives { get; set; } = 3;
        public int Level { get; set; } = 1;
        public int Combo { get; set; } = 0;
        public int MaxCombo { get; set; } = 0;
        
        /// <summary>
        /// Calculate current play time
        /// Shows: DateTime operations, TimeSpan calculations
        /// </summary>
        public TimeSpan PlayTime => DateTime.Now - GameStartTime;
        
        /// <summary>
        /// Calculate shooting accuracy
        /// Demonstrates: Mathematical operations, Division by zero handling
        /// </summary>
        public double Accuracy => ShotsFired > 0 ? (double)EnemiesKilled / ShotsFired * 100 : 0;
        
        /// <summary>
        /// Add score with combo multiplier
        /// Shows: Mathematical calculations, Bonus systems
        /// </summary>
        public void AddScore(int baseScore)
        {
            var multiplier = 1 + (Combo * 0.1f);
            CurrentScore += (int)(baseScore * multiplier);
        }
        
        /// <summary>
        /// Reset combo on miss or hit
        /// Demonstrates: Game mechanic implementation
        /// </summary>
        public void ResetCombo()
        {
            MaxCombo = Math.Max(MaxCombo, Combo);
            Combo = 0;
        }
        
        /// <summary>
        /// Increment combo on successful hit
        /// Shows: Progressive systems, Statistics tracking
        /// </summary>
        public void IncrementCombo()
        {
            Combo++;
            MaxCombo = Math.Max(MaxCombo, Combo);
        }
        
        /// <summary>
        /// Reset all statistics for new game
        /// Demonstrates: Object state management
        /// </summary>
        public void Reset()
        {
            CurrentScore = 0;
            EnemiesKilled = 0;
            ShotsFired = 0;
            GameStartTime = DateTime.Now;
            Lives = 3;
            Level = 1;
            Combo = 0;
            MaxCombo = 0;
        }
    }
}