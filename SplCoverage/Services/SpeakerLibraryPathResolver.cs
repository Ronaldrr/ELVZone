using System;
using System.IO;
using System.Reflection;

namespace ELVZone.SplCoverage.Services
{
    public static class SpeakerLibraryPathResolver
    {
        public static string GetDefaultLibraryPath()
        {
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrWhiteSpace(assemblyDirectory))
            {
                var outputPath = Path.Combine(assemblyDirectory, "EquipmentLibrary", "Speakers");
                if (Directory.Exists(outputPath))
                {
                    return outputPath;
                }
            }

            var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "EquipmentLibrary", "Speakers"));
            if (Directory.Exists(projectPath))
            {
                return projectPath;
            }

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CoverageTools",
                "EquipmentLibrary",
                "Speakers");
        }
    }
}
