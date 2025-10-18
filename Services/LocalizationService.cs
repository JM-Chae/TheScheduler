using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace TheScheduler.Services
{
    public class LocalizationService
    {
        private static readonly LocalizationService _instance = new LocalizationService();
        public static LocalizationService Instance => _instance;

        private Dictionary<string, string> _resources = new Dictionary<string, string>();

        private LocalizationService() { }

        public void Initialize(CultureInfo culture)
        {
            string json;
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Languages", $"{culture.Name}.json");

            if (File.Exists(filePath))
            {
                json = File.ReadAllText(filePath);
            }
            else
            {
                // Fallback to default language if the file for the selected culture doesn't exist
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "Languages", "ja-JP.json");
                json = File.ReadAllText(filePath);
            }

            _resources = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        public string GetString(string key)
        {
            return _resources.TryGetValue(key, out var value) ? value : key;
        }
    }
}
