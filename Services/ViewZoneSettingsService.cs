using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using ELVZone.Models;

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
            return LoadFrom(SettingsPath);
        }

        public ViewZoneSettings LoadFrom(string path)
        {
            if (!File.Exists(path))
            {
                return ViewZoneSettings.CreateDefault();
            }

            try
            {
                using (var stream = File.OpenRead(path))
                {
                    var serializer = new DataContractJsonSerializer(typeof(ViewZoneSettings));
                    var settings = serializer.ReadObject(stream) as ViewZoneSettings;
                    if (settings == null)
                    {
                        return ViewZoneSettings.CreateDefault();
                    }

                    settings.EnsureShape();
                    return settings;
                }
            }
            catch
            {
                return ViewZoneSettings.CreateDefault();
            }
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
            using (var stream = File.Create(path))
            {
                var serializer = new DataContractJsonSerializer(typeof(ViewZoneSettings));
                serializer.WriteObject(stream, settings);
            }

            FormatJsonFile(path);
        }

        private static void FormatJsonFile(string path)
        {
            var json = File.ReadAllText(path, Encoding.UTF8);
            var indent = 0;
            var quoted = false;
            var builder = new StringBuilder();

            for (var i = 0; i < json.Length; i++)
            {
                var ch = json[i];
                if (ch == '"' && (i == 0 || json[i - 1] != '\\'))
                {
                    quoted = !quoted;
                }

                if (quoted)
                {
                    builder.Append(ch);
                    continue;
                }

                switch (ch)
                {
                    case '{':
                    case '[':
                        builder.Append(ch);
                        builder.AppendLine();
                        indent++;
                        builder.Append(new string(' ', indent * 2));
                        break;
                    case '}':
                    case ']':
                        builder.AppendLine();
                        indent--;
                        builder.Append(new string(' ', indent * 2));
                        builder.Append(ch);
                        break;
                    case ',':
                        builder.Append(ch);
                        builder.AppendLine();
                        builder.Append(new string(' ', indent * 2));
                        break;
                    case ':':
                        builder.Append(": ");
                        break;
                    default:
                        if (!char.IsWhiteSpace(ch))
                        {
                            builder.Append(ch);
                        }
                        break;
                }
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }
    }
}
