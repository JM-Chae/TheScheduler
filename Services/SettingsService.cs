using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace TheScheduler.Services
{
    public class SettingsService
    {
        private static readonly SettingsService _instance = new SettingsService();
        public static SettingsService Instance => _instance;

        private readonly string _settingsPath;

        private SettingsService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _settingsPath = Path.Combine(appDataPath, "TheScheduler", "settings.json");
            LoadSettings();
        }

        public List<CultureInfo> AvailableCultures { get; } = new List<CultureInfo>
        {
            new CultureInfo("ja-JP"),
            new CultureInfo("ko-KR"),
            new CultureInfo("en-US")
        };

        private CultureInfo _currentCulture = new CultureInfo("ja-JP");
        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (_currentCulture != value)
                {
                    _currentCulture = value;
                    SaveSettings();
                    OnCultureChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void LoadSettings()
        {
            if (File.Exists(_settingsPath))
            {
                try
                {
                    string json = File.ReadAllText(_settingsPath);
                    var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (settings.TryGetValue("Culture", out var cultureName))
                    {
                        _currentCulture = new CultureInfo(cultureName);
                    }
                }
                catch (Exception)
                {
                    // Handle exceptions like file corruption
                }
            }
        }

        private void SaveSettings()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath));
                var settings = new Dictionary<string, string>
                {
                    ["Culture"] = _currentCulture.Name
                };
                string json = JsonSerializer.Serialize(settings);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception)
            {
                // Handle exceptions like permission errors
            }
        }

        public event EventHandler OnCultureChanged;
    }
}
