using System;
using System.IO;
using System.Text.Json;

namespace TrueStretchedValorant
{
    public sealed class UserSettings
    {
        private static readonly string SettingsPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public string? IniFilePath { get; set; }
        public string? QResPath { get; set; }
        public int StretchedWidth { get; set; } = 1440;
        public int StretchedHeight { get; set; } = 1080;

        public static UserSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return new UserSettings();
                string json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
            }
            catch { return new UserSettings(); }
        }

        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, options));
            }
            catch { }
        }
    }
}
