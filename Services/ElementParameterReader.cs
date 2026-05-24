using System;
using Autodesk.Revit.DB;
using ELVZone.Models;

namespace ELVZone.Services
{
    public class ElementParameterReader
    {
        private const double FeetPerMeter = 3.280839895013123;

        public double ReadAngleRadians(Element element, ParameterSetting setting, double fallbackDegrees)
        {
            var fallback = DegreesToRadians(fallbackDegrees);
            if (setting == null || !setting.IsEnabled || string.IsNullOrWhiteSpace(setting.ParameterName))
            {
                return fallback;
            }

            var parameter = element.LookupParameter(setting.ParameterName);
            if (parameter == null || !parameter.HasValue)
            {
                return fallback;
            }

            if (parameter.StorageType == StorageType.Double)
            {
                return parameter.AsDouble();
            }

            if (double.TryParse(parameter.AsValueString(), out var degrees))
            {
                return DegreesToRadians(degrees);
            }

            return fallback;
        }

        public double ReadLengthFeet(Element element, ParameterSetting setting, double fallbackMeters)
        {
            var fallback = fallbackMeters * FeetPerMeter;
            if (setting == null || !setting.IsEnabled || string.IsNullOrWhiteSpace(setting.ParameterName))
            {
                return fallback;
            }

            var parameter = element.LookupParameter(setting.ParameterName);
            if (parameter == null || !parameter.HasValue)
            {
                return fallback;
            }

            if (parameter.StorageType == StorageType.Double)
            {
                return parameter.AsDouble();
            }

            if (double.TryParse(parameter.AsValueString(), out var meters))
            {
                return meters * FeetPerMeter;
            }

            return fallback;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
