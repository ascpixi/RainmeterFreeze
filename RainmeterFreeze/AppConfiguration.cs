using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using RainmeterFreeze.Enumerations;

namespace RainmeterFreeze;

/// <summary>
/// Represents the configuration file of RainmeterFreeze.
/// </summary>
[Serializable]
public class AppConfiguration
{
    public static readonly string ConfigPath = $"{Program.DataFolderPath}\\config.json";

    private static readonly JsonSerializerOptions jsonOptions = new() {
        WriteIndented = true,
        IgnoreReadOnlyProperties = true,
        Converters = {
            new JsonStringEnumConverter()
        },
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// The freezing algorithm to use.
    /// </summary>
    public FreezeAlgorithm FreezeAlgorithm { get; set; } = FreezeAlgorithm.Maximized;

    /// <summary>
    /// The freeze mode to use.
    /// </summary>
    public FreezeMode FreezeMode { get; set; } = FreezeMode.Suspend;

    /// <summary>
    /// Saves the configuration to the <see cref="ConfigPath"/>.
    /// </summary>
    public void Save()
    {
        string json = JsonSerializer.Serialize(this, jsonOptions);
        File.WriteAllText(ConfigPath, json);
    }

    /// <summary>
    /// Loads the application configuration from the <see cref="ConfigPath"/>.
    /// If no file exists under the expected location, a new instance of
    /// the <see cref="AppConfiguration"/> class is returned.
    /// </summary>
    /// <returns>The loaded <see cref="AppConfiguration"/>, or a new instance of it if no file has been found.</returns>
    public static AppConfiguration Load()
    {
        if (File.Exists(ConfigPath)) {
            string json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<AppConfiguration>(json, jsonOptions);
        }

        return new();
    }
}
