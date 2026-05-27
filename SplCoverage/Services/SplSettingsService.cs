using System;
using System.IO;
using System.Text;
using ELVZone.SplCoverage.Models;
using Newtonsoft.Json;

namespace ELVZone.SplCoverage.Services
{
    public class SplSettingsService
    {
        public string SettingsDirectory { get; }
        public string SettingsPath { get; }

        public SplSettingsService()
        {
            SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoverageTools");
            SettingsPath = Path.Combine(SettingsDirectory, "settings.json");
        }

        public SplSettings Load()
        {
            if (!File.Exists(SettingsPath))
            {
                return SplSettings.CreateDefault();
            }

            return LoadFrom(SettingsPath);
        }

        public SplSettings LoadFrom(string path)
        {
            if (!File.Exists(path))
            {
                return SplSettings.CreateDefault();
            }

            var settings = JsonConvert.DeserializeObject<SplSettings>(File.ReadAllText(path, Encoding.UTF8));
            if (settings == null)
            {
                throw new InvalidOperationException("Файл настроек SPL пустой или имеет неверный формат.");
            }

            settings.EnsureShape();
            if (!Directory.Exists(settings.SpeakerLibraryPath))
            {
                settings.SpeakerLibraryPath = SpeakerLibraryPathResolver.GetDefaultLibraryPath();
            }

            return settings;
        }

        public void Save(SplSettings settings)
        {
            SaveTo(SettingsPath, settings);
        }

        public void SaveTo(string path, SplSettings settings)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            settings.EnsureShape();
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(path, json, Encoding.UTF8);
        }
    }
}
