using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ELVZone.Core.Utils;
using ELVZone.SplCoverage.Models;

namespace ELVZone.SplCoverage.Services
{
    public class SplCalculationService
    {
        private const double MinDistanceMeters = 0.25;
        private readonly DirectivityService _directivityService = new DirectivityService();

        public IList<SplCellResult> Calculate(BoundingBoxXYZ bounds, Func<XYZ, bool> containsPoint, IList<SpeakerInstanceData> speakers, SplSettings settings, double elevationFeet)
        {
            var results = new List<SplCellResult>();
            var step = UnitConversion.MetersToFeet(settings.CellSizeMeters);
            var half = step / 2.0;

            for (var x = bounds.Min.X + half; x <= bounds.Max.X; x += step)
            {
                for (var y = bounds.Min.Y + half; y <= bounds.Max.Y; y += step)
                {
                    var point = new XYZ(x, y, elevationFeet);
                    if (!containsPoint(point))
                    {
                        continue;
                    }

                    var spl = CalculateTotal(point, speakers, settings);
                    results.Add(new SplCellResult
                    {
                        Center = point,
                        Spl = spl,
                        Range = GetRange(spl, settings.RequiredMinSpl),
                        TypeName = GetTypeName(spl, settings),
                        SourceSpeakers = string.Join(", ", speakers.Select(s => s.Element.Id.IntegerValue.ToString()))
                    });
                }
            }

            return results;
        }

        private double CalculateTotal(XYZ point, IList<SpeakerInstanceData> speakers, SplSettings settings)
        {
            var sum = 0.0;
            foreach (var speaker in speakers)
            {
                var distanceMeters = Math.Max(MinDistanceMeters, UnitConversion.FeetToMeters(speaker.Location.DistanceTo(point)));
                var correction = settings.UseDirectivity ? GetDirectivityCorrection(speaker, point, settings.CalculationFrequencyHz) : 0;
                var spl = speaker.SensitivityDb + 10.0 * Math.Log10(Math.Max(0.001, speaker.PowerW)) - 20.0 * Math.Log10(distanceMeters) + correction;
                sum += Math.Pow(10.0, spl / 10.0);
            }

            return sum <= 0 ? 0 : 10.0 * Math.Log10(sum);
        }

        private double GetDirectivityCorrection(SpeakerInstanceData speaker, XYZ point, int frequencyHz)
        {
            var toPoint = point - speaker.Location;
            if (toPoint.GetLength() < 0.001)
            {
                return 0;
            }

            var direction = speaker.Direction.Normalize();
            var vector = toPoint.Normalize();
            var dot = Math.Max(-1, Math.Min(1, direction.DotProduct(vector)));
            var angle = Math.Acos(dot) * 180.0 / Math.PI;
            return _directivityService.GetCorrection(speaker.Definition, frequencyHz, angle);
        }

        private static string GetRange(double spl, double requiredMin)
        {
            if (spl < requiredMin) return "Below required";
            if (spl < 80) return "Required-80";
            if (spl < 90) return "80-90";
            return "90+";
        }

        private static string GetTypeName(double spl, SplSettings settings)
        {
            if (spl < settings.RequiredMinSpl) return settings.GreyTypeName;
            if (spl < 80) return settings.YellowTypeName;
            if (spl < 90) return settings.OrangeTypeName;
            return settings.RedTypeName;
        }
    }
}
