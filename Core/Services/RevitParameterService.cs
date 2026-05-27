using System.Globalization;
using Autodesk.Revit.DB;
using ELVZone.Core.Utils;

namespace ELVZone.Core.Services
{
    public class RevitParameterService
    {
        public string GetString(Element element, string parameterName)
        {
            var parameter = element?.LookupParameter(parameterName);
            if (parameter == null || !parameter.HasValue)
            {
                return null;
            }

            if (parameter.StorageType == StorageType.String)
            {
                return parameter.AsString();
            }

            return parameter.AsValueString();
        }

        public double? GetDouble(Element element, string parameterName)
        {
            var parameter = element?.LookupParameter(parameterName);
            if (parameter == null || !parameter.HasValue)
            {
                return null;
            }

            if (parameter.StorageType == StorageType.Double)
            {
                return parameter.AsDouble();
            }

            var text = parameter.StorageType == StorageType.String ? parameter.AsString() : parameter.AsValueString();
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariant))
            {
                return invariant;
            }

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var local))
            {
                return local;
            }

            return null;
        }

        public double? GetLengthFeet(Element element, string parameterName)
        {
            var parameter = element?.LookupParameter(parameterName);
            if (parameter == null || !parameter.HasValue)
            {
                return null;
            }

            if (parameter.StorageType == StorageType.Double)
            {
                return parameter.AsDouble();
            }

            var text = parameter.StorageType == StorageType.String ? parameter.AsString() : parameter.AsValueString();
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariantMeters))
            {
                return UnitConversion.MetersToFeet(invariantMeters);
            }

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var localMeters))
            {
                return UnitConversion.MetersToFeet(localMeters);
            }

            return null;
        }

        public bool TrySet(Element element, string parameterName, string value)
        {
            var parameter = element?.LookupParameter(parameterName);
            if (parameter == null || parameter.IsReadOnly)
            {
                return false;
            }

            if (parameter.StorageType == StorageType.String)
            {
                parameter.Set(value ?? string.Empty);
                return true;
            }

            return false;
        }

        public bool TrySet(Element element, string parameterName, double value)
        {
            var parameter = element?.LookupParameter(parameterName);
            if (parameter == null || parameter.IsReadOnly)
            {
                return false;
            }

            if (parameter.StorageType == StorageType.Double)
            {
                parameter.Set(value);
                return true;
            }

            if (parameter.StorageType == StorageType.String)
            {
                parameter.Set(value.ToString(CultureInfo.InvariantCulture));
                return true;
            }

            return false;
        }
    }
}
