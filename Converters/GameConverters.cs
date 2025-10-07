using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GalacticCommander.Converters
{
    /// <summary>
    /// Advanced WPF Value Converters for Game-specific Data Binding
    /// Demonstrates: IValueConverter, Custom Binding Logic, UI Data Transformation
    /// </summary>
    
    /// <summary>
    /// Converts health values to progress bar width
    /// Shows mathematical conversion and UI binding optimization
    /// </summary>
    public class HealthToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float health && parameter is string maxHealthStr && float.TryParse(maxHealthStr, out var maxHealth))
            {
                var percentage = Math.Max(0, Math.Min(1, health / maxHealth));
                return percentage * 200; // Assuming 200px width bar
            }
            
            if (value is float healthPercent)
            {
                return Math.Max(0, Math.Min(200, healthPercent * 2)); // Convert 0-100 to 0-200px
            }
            
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts shield values to progress bar width
    /// </summary>
    public class ShieldToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float shield && parameter is string maxShieldStr && float.TryParse(maxShieldStr, out var maxShield))
            {
                var percentage = maxShield > 0 ? Math.Max(0, Math.Min(1, shield / maxShield)) : 0;
                return percentage * 200;
            }
            
            if (value is float shieldPercent)
            {
                return Math.Max(0, Math.Min(200, shieldPercent * 2));
            }
            
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts game status to visibility for game over overlay
    /// Shows conditional UI visibility based on game state
    /// </summary>
    public class GameOverToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.Equals("Game Over", StringComparison.OrdinalIgnoreCase) 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
            }
            
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts loading status to visibility
    /// </summary>
    public class LoadingToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.Equals("Loading", StringComparison.OrdinalIgnoreCase) 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
            }
            
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Advanced multi-value converter for complex calculations
    /// Shows IMultiValueConverter for combining multiple bound values
    /// </summary>
    public class HealthShieldCombinedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 4 && 
                values[0] is float health && 
                values[1] is float maxHealth &&
                values[2] is float shield && 
                values[3] is float maxShield)
            {
                var totalCurrent = health + shield;
                var totalMax = maxHealth + maxShield;
                
                return totalMax > 0 ? (totalCurrent / totalMax) * 200 : 0.0;
            }
            
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts numerical values to formatted display strings
    /// Shows string formatting and localization support
    /// </summary>
    public class ScoreFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int score)
            {
                if (score >= 1000000)
                    return $"{score / 1000000.0:F1}M";
                if (score >= 1000)
                    return $"{score / 1000.0:F1}K";
                
                return score.ToString("N0");
            }
            
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts time spans to formatted time display
    /// Shows advanced time formatting and display logic
    /// </summary>
    public class TimeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                if (timeSpan.TotalHours >= 1)
                    return timeSpan.ToString(@"h\:mm\:ss");
                else
                    return timeSpan.ToString(@"m\:ss");
            }
            
            return "0:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts boolean values to colors for dynamic theming
    /// Shows advanced styling and theme management
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public System.Windows.Media.Brush TrueBrush { get; set; } = System.Windows.Media.Brushes.Green;
        public System.Windows.Media.Brush FalseBrush { get; set; } = System.Windows.Media.Brushes.Red;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueBrush : FalseBrush;
            }
            
            return FalseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts entity types to appropriate colors
    /// Shows enum-based styling and game entity visualization
    /// </summary>
    public class EntityTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string entityType)
            {
                return entityType.ToLower() switch
                {
                    "player" => System.Windows.Media.Brushes.Cyan,
                    "enemy" => System.Windows.Media.Brushes.Red,
                    "projectile" => System.Windows.Media.Brushes.Yellow,
                    "powerup" => System.Windows.Media.Brushes.Green,
                    "explosion" => System.Windows.Media.Brushes.Orange,
                    _ => System.Windows.Media.Brushes.White
                };
            }
            
            return System.Windows.Media.Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Mathematical converter for positioning calculations
    /// Shows advanced mathematical operations in binding
    /// </summary>
    public class PositionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && 
                values[0] is double position && 
                values[1] is double size)
            {
                // Center the object by subtracting half the size
                return position - (size / 2);
            }
            
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Angle converter for rotation transformations
    /// Shows mathematical conversion between radians and degrees
    /// </summary>
    public class RadianToDegreesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float radians)
            {
                return radians * 180.0 / Math.PI;
            }
            
            if (value is double radiansDouble)
            {
                return radiansDouble * 180.0 / Math.PI;
            }
            
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double degrees)
            {
                return degrees * Math.PI / 180.0;
            }
            
            return 0.0;
        }
    }

    /// <summary>
    /// Performance-optimized converter with caching
    /// Shows performance optimization techniques in converters
    /// </summary>
    public class CachedPercentageConverter : IValueConverter
    {
        private static readonly Dictionary<(float, float), double> _cache = new();
        private const int MaxCacheSize = 1000;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float current && parameter is string maxStr && float.TryParse(maxStr, out var max))
            {
                var key = (current, max);
                
                if (_cache.TryGetValue(key, out var cachedResult))
                {
                    return cachedResult;
                }
                
                var result = max > 0 ? Math.Max(0, Math.Min(1, current / max)) * 100 : 0;
                
                // Simple cache eviction
                if (_cache.Count >= MaxCacheSize)
                {
                    _cache.Clear();
                }
                
                _cache[key] = result;
                return result;
            }
            
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}