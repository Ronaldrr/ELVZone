using System;
using System.IO;
using System.Text;
using ELVZone.Models;
using Newtonsoft.Json;

namespace ELVZone.Services
{
    public class ViewZoneSettingsService
    {
        public string SettingsDirectory { get; }
        public string SettingsPath { get; }

        public ViewZoneSettingsService()
        {
            SettingsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ViewZonePlugin");
            SettingsPath = Path.Combine(SettingsDirectory, "settings.json");
        }

        public ViewZoneSettings Load()
        {
            if (!File.Exists(SettingsPath))
            {
                return ViewZoneSettings.CreateDefault();
            }

            return LoadFrom(SettingsPath);
        }

        public ViewZoneSettings LoadFrom(string path)
        {
            if (!File.Exists(path))
            {
                return ViewZoneSettings.CreateDefault();
            }

            var json = File.ReadAllText(path, Encoding.UTF8);
            var settings = JsonConvert.DeserializeObject<ViewZoneSettings>(json);
            if (settings == null)
            {
                throw new InvalidOperationException($"Файл настроек пустой или имеет неверный формат: {path}");
            }

            settings.EnsureShape();
            return settings;
        }

        public void Save(ViewZoneSettings settings)
        {
            SaveTo(SettingsPath, settings);
        }

        public void SaveTo(string path, ViewZoneSettings settings)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            settings.EnsureShape();
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(path, json, Encoding.UTF8);
        }
    }
}
