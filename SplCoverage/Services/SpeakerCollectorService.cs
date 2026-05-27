using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using ELVZone.Core.Services;
using ELVZone.SplCoverage.Models;

namespace ELVZone.SplCoverage.Services
{
    public class SpeakerCollectorService
    {
        private readonly RevitParameterService _parameters = new RevitParameterService();
        private readonly SpeakerLibraryService _library = new SpeakerLibraryService();

        public IList<SpeakerInstanceData> Collect(Document document, Func<XYZ, bool> containsPoint, SplSettings settings)
        {
            var speakers = _library.Load(settings.SpeakerLibraryPath).ToList();
            var byFile = speakers.ToDictionary(item => Path.GetFullPath(item.LibraryFile), item => item, StringComparer.OrdinalIgnoreCase);
            var result = new List<SpeakerInstanceData>();
            var elements = new FilteredElementCollector(document)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>();

            foreach (var instance in elements)
            {
                var location = GetSpeakerLocation(instance, settings);
                if (location == null || !containsPoint(location))
                {
                    continue;
                }

                var definition = ResolveDefinition(instance, speakers, byFile);
                if (definition == null)
                {
                    continue;
                }

                var power = _parameters.GetDouble(instance, "SPL_PowerW") ?? definition.RatedPowerW;
                var sensitivity = _parameters.GetDouble(instance, "SPL_SensitivityDb") ?? definition.SensitivityDb1W1M;
                result.Add(new SpeakerInstanceData
                {
                    Element = instance,
                    Location = location,
                    Direction = GetDirection(instance, definition),
                    Definition = definition,
                    PowerW = power,
                    SensitivityDb = sensitivity
                });
            }

            return result;
        }

        private SpeakerDefinition ResolveDefinition(
            FamilyInstance instance,
            IList<SpeakerDefinition> speakers,
            IDictionary<string, SpeakerDefinition> byFile)
        {
            var libraryFile = _parameters.GetString(instance, "SPL_LibraryFile");
            if (!string.IsNullOrWhiteSpace(libraryFile))
            {
                var fullPath = Path.GetFullPath(libraryFile);
                if (byFile.TryGetValue(fullPath, out var byParameter))
                {
                    return byParameter;
                }

                if (File.Exists(fullPath))
                {
                    return new SpeakerLibraryService()
                        .Load(Path.GetDirectoryName(fullPath))
                        .FirstOrDefault(item => string.Equals(Path.GetFullPath(item.LibraryFile), fullPath, StringComparison.OrdinalIgnoreCase));
                }
            }

            return ResolveDefinitionByTypeName(instance, speakers);
        }

        private static SpeakerDefinition ResolveDefinitionByTypeName(FamilyInstance instance, IEnumerable<SpeakerDefinition> speakers)
        {
            var typeText = Normalize(string.Join(" ", new[]
            {
                instance.Name,
                instance.Symbol?.Name,
                instance.Symbol?.FamilyName,
                instance.Symbol?.Family?.Name
            }.Where(value => !string.IsNullOrWhiteSpace(value))));

            if (string.IsNullOrWhiteSpace(typeText))
            {
                return null;
            }

            foreach (var speaker in speakers)
            {
                var candidates = new[]
                {
                    Path.GetFileName(speaker.LibraryFile),
                    Path.GetFileNameWithoutExtension(speaker.LibraryFile),
                    $"{speaker.Manufacturer} {speaker.Model}",
                    $"{speaker.Manufacturer}_{speaker.Model}",
                    speaker.Model
                };

                foreach (var candidate in candidates)
                {
                    var normalizedCandidate = Normalize(candidate);
                    if (!string.IsNullOrWhiteSpace(normalizedCandidate) && typeText.Contains(normalizedCandidate))
                    {
                        return speaker;
                    }
                }
            }

            return null;
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            foreach (var ch in value.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(ch))
                {
                    builder.Append(ch);
                }
            }

            return builder.ToString();
        }

        private XYZ GetSpeakerLocation(FamilyInstance instance, SplSettings settings)
        {
            var location = GetBaseLocation(instance);
            if (location == null)
            {
                return null;
            }

            var mountingHeight = string.IsNullOrWhiteSpace(settings.SpeakerMountingHeightParameterName)
                ? null
                : _parameters.GetLengthFeet(instance, settings.SpeakerMountingHeightParameterName) ??
                  _parameters.GetLengthFeet(instance.Symbol, settings.SpeakerMountingHeightParameterName);

            return mountingHeight.HasValue
                ? new XYZ(location.X, location.Y, location.Z + mountingHeight.Value)
                : location;
        }

        private static XYZ GetBaseLocation(FamilyInstance instance)
        {
            if (instance.Location is LocationPoint locationPoint)
            {
                return locationPoint.Point;
            }

            var box = instance.get_BoundingBox(null);
            return box == null ? null : (box.Min + box.Max).Multiply(0.5);
        }

        private static XYZ GetDirection(FamilyInstance instance, SpeakerDefinition definition)
        {
            if (string.Equals(definition.Type, "ceiling", StringComparison.OrdinalIgnoreCase))
            {
                return -XYZ.BasisZ;
            }

            var facing = instance.FacingOrientation;
            if (facing != null && facing.GetLength() > 0.001)
            {
                return facing.Normalize();
            }

            if (instance.Location is LocationPoint locationPoint)
            {
                return new XYZ(Math.Sin(locationPoint.Rotation), Math.Cos(locationPoint.Rotation), 0).Normalize();
            }

            return XYZ.BasisY;
        }
    }
}
