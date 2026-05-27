using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ELVZone.SplCoverage.Models;

namespace ELVZone.SplCoverage.Services
{
    public class DirectivityService
    {
        public double GetCorrection(SpeakerDefinition speaker, int frequencyHz, double angleDegrees)
        {
            var points = GetPoints(speaker, frequencyHz);
            if (points == null || points.Count == 0)
            {
                return 0;
            }

            var angle = Math.Abs(angleDegrees);
            var ordered = points.OrderBy(point => point.Angle).ToList();
            if (angle <= ordered[0].Angle)
            {
                return ordered[0].Db;
            }

            if (angle >= ordered[ordered.Count - 1].Angle)
            {
                return ordered[ordered.Count - 1].Db;
            }

            for (var i = 0; i < ordered.Count - 1; i++)
            {
                var a = ordered[i];
                var b = ordered[i + 1];
                if (angle < a.Angle || angle > b.Angle)
                {
                    continue;
                }

                var t = (angle - a.Angle) / (b.Angle - a.Angle);
                return a.Db + (b.Db - a.Db) * t;
            }

            return 0;
        }

        private static IList<PolarPoint> GetPoints(SpeakerDefinition speaker, int frequencyHz)
        {
            var frequencies = speaker?.PolarPattern?.Frequencies;
            if (frequencies == null || frequencies.Count == 0)
            {
                return null;
            }

            var key = frequencyHz.ToString(CultureInfo.InvariantCulture);
            if (frequencies.TryGetValue(key, out var exact))
            {
                return exact;
            }

            var nearest = frequencies
                .Select(pair => new { pair.Key, Points = pair.Value, Distance = Distance(pair.Key, frequencyHz) })
                .Where(item => item.Distance.HasValue)
                .OrderBy(item => item.Distance.Value)
                .FirstOrDefault();

            return nearest?.Points;
        }

        private static int? Distance(string key, int frequencyHz)
        {
            return int.TryParse(key, out var value) ? Math.Abs(value - frequencyHz) : (int?)null;
        }
    }
}
