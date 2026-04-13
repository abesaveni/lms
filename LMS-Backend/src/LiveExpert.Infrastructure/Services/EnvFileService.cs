using Microsoft.Extensions.Logging;

namespace LiveExpert.Infrastructure.Services;

/// <summary>
/// Service to update .env file with new configuration values
/// </summary>
public class EnvFileService
{
    private readonly ILogger<EnvFileService> _logger;
    private readonly string _envFilePath;

    public EnvFileService(ILogger<EnvFileService> logger)
    {
        _logger = logger;
        
        // Try multiple paths for .env file
        var currentDir = Directory.GetCurrentDirectory();
        var envPath1 = Path.Combine(currentDir, "..", "..", ".env"); // LMS-Backend/.env
        var envPath2 = Path.Combine(currentDir, "..", "..", "..", ".env"); // LMS _ Application/.env
        var envPath3 = Path.Combine(currentDir, ".env"); // Current directory

        var envPaths = new[] 
        { 
            Path.GetFullPath(envPath1),
            Path.GetFullPath(envPath2),
            Path.GetFullPath(envPath3)
        };

        string? foundPath = null;
        foreach (var envPath in envPaths)
        {
            if (File.Exists(envPath))
            {
                foundPath = envPath;
                break;
            }
        }

        if (foundPath == null)
        {
            // Use the most likely path (LMS-Backend/.env)
            foundPath = Path.GetFullPath(envPath1);
            _logger.LogWarning("Env file not found, will create at: {Path}", foundPath);
        }

        _envFilePath = foundPath;
    }

    /// <summary>
    /// Update or add a key-value pair in the .env file
    /// </summary>
    public async Task<bool> UpdateEnvVariableAsync(string key, string value)
    {
        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_envFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Read existing .env file if it exists
            var lines = new List<string>();
            if (File.Exists(_envFilePath))
            {
                lines = (await File.ReadAllLinesAsync(_envFilePath)).ToList();
            }

            // Find and update existing key or add new one
            var keyFound = false;
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i].Trim();
                
                // Skip comments and empty lines
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                {
                    continue;
                }

                // Check if this line contains the key
                if (line.StartsWith($"{key}=", StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] = $"{key}={value}";
                    keyFound = true;
                    break;
                }
            }

            // If key not found, add it at the end
            if (!keyFound)
            {
                lines.Add($"{key}={value}");
            }

            // Write back to file
            await File.WriteAllLinesAsync(_envFilePath, lines);
            
            _logger.LogInformation("Updated .env file: {Key}", key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update .env file for key: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Update multiple environment variables at once
    /// </summary>
    public async Task<bool> UpdateEnvVariablesAsync(Dictionary<string, string> variables)
    {
        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_envFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Read existing .env file if it exists
            var lines = new List<string>();
            if (File.Exists(_envFilePath))
            {
                lines = (await File.ReadAllLinesAsync(_envFilePath)).ToList();
            }

            // Track which keys we've updated
            var updatedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Update existing keys
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i].Trim();
                
                // Skip comments and empty lines
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                {
                    continue;
                }

                // Check each variable we want to update
                foreach (var (key, value) in variables)
                {
                    if (line.StartsWith($"{key}=", StringComparison.OrdinalIgnoreCase))
                    {
                        lines[i] = $"{key}={value}";
                        updatedKeys.Add(key);
                        break;
                    }
                }
            }

            // Add any keys that weren't found
            foreach (var (key, value) in variables)
            {
                if (!updatedKeys.Contains(key))
                {
                    lines.Add($"{key}={value}");
                }
            }

            // Write back to file
            await File.WriteAllLinesAsync(_envFilePath, lines);
            
            _logger.LogInformation("Updated .env file with {Count} variables", variables.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update .env file with multiple variables");
            return false;
        }
    }

    /// <summary>
    /// Get the current value of an environment variable from .env file
    /// </summary>
    public async Task<string?> GetEnvVariableAsync(string key)
    {
        try
        {
            if (!File.Exists(_envFilePath))
            {
                return null;
            }

            var lines = await File.ReadAllLinesAsync(_envFilePath);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                {
                    continue;
                }

                if (trimmed.StartsWith($"{key}=", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = trimmed.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        return parts[1].Trim('"', '\'');
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read .env file for key: {Key}", key);
            return null;
        }
    }
}
