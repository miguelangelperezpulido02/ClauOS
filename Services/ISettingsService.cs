using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace ClauOS.Services
{
    public interface ISettingsService
    {
        T? GetSetting<T>(string key, T? defaultValue = default);
        void SetSetting<T>(string key, T value);
        void Save();
    }

    public class JsonSettingsService : ISettingsService
    {
        private readonly string _settingsPath;
        private readonly string _backupPath;
        private Dictionary<string, object> _settings = new();
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public JsonSettingsService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "ClauOS");
            Directory.CreateDirectory(appFolder);
            _settingsPath = Path.Combine(appFolder, "settings.json");
            _backupPath = Path.Combine(appFolder, "settings.json.bak");
            Load();
        }

        private void Load()
        {
            if (File.Exists(_settingsPath))
            {
                try
                {
                    var json = File.ReadAllText(_settingsPath);
                    _settings = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _jsonOptions) 
                                ?? new Dictionary<string, object>();
                    CreateBackup();
                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[JsonSettingsService] Error loading settings: {ex.Message}");
                }
            }

            if (File.Exists(_backupPath))
            {
                try
                {
                    var json = File.ReadAllText(_backupPath);
                    _settings = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _jsonOptions) 
                                ?? new Dictionary<string, object>();
                    Debug.WriteLine($"[JsonSettingsService] Restored from backup");
                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[JsonSettingsService] Error loading backup: {ex.Message}");
                }
            }

            _settings = new Dictionary<string, object>();
        }

        private void CreateBackup()
        {
            try
            {
                if (File.Exists(_settingsPath))
                    File.Copy(_settingsPath, _backupPath, true);
            }
            catch { }
        }

        public T? GetSetting<T>(string key, T? defaultValue = default)
        {
            if (_settings.TryGetValue(key, out var value))
            {
                try
                {
                    if (value is JsonElement element)
                    {
                        return element.Deserialize<T>(_jsonOptions);
                    }
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[JsonSettingsService] Error getting setting '{key}': {ex.Message}");
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        public void SetSetting<T>(string key, T value)
        {
            _settings[key] = value!;
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings, _jsonOptions);
                var tempPath = _settingsPath + ".tmp";
                File.WriteAllText(tempPath, json);
                File.Move(tempPath, _settingsPath, true);
                CreateBackup();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[JsonSettingsService] Error saving settings: {ex.Message}");
            }
        }
    }
}