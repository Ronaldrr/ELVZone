using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace ELVZone.Geometry
{
    public class ViewZoneGeometryBuilder
    {
        private const double MinLength = 0.001;

        public IList<IList<Curve>> BuildZoneBoundaries(CameraViewZoneData data, double planeElevation)
        {
            var result = new List<IList<Curve>>();
            var origin = new XYZ(data.Origin.X, data.Origin.Y, planeElevation);
            var direction = NormalizePlanVector(data.Direction);
            var halfAngle = Math.Max(0.001, data.HorizontalAngleRadians / 2.0);
            var currentRadius = 0.0;
            var totalRadius = Math.Max(0, data.TotalLengthFeet);

            for (var i = 0; i < 4; i++)
            {
                var zoneLength = data.ZoneLengthsFeet[i];
                var nextRadius = Math.Min(totalRadius, currentRadius + Math.Max(0, zoneLength));
                if (nextRadius - currentRadius > MinLength)
                {
                    result.Add(BuildZoneBoundary(origin, direction, halfAngle, currentRadius, nextRadius));
                }
                else
                {
                    result.Add(new List<Curve>());
                }

                currentRadius = nextRadius;
            }

            return result;
        }

        private static IList<Curve> BuildZoneBoundary(
            XYZ origin,
            XYZ direction,
            double halfAngle,
            double innerRadius,
            double outerRadius)
        {
            var points = new List<XYZ>();
            if (innerRadius <= MinLength)
            {
                points.Add(origin);
                points.AddRange(BuildArcPoints(origin, direction, outerRadius, -halfAngle, halfAngle));
            }
            else
            {
                points.Add(PointOnRay(origin, direction, innerRadius, -halfAngle));
                points.Add(PointOnRay(origin, direction, outerRadius, -halfAngle));
                points.AddRange(BuildArcPoints(origin, direction, outerRadius, -halfAngle, halfAngle, skipFirst: true));
                points.AddRange(BuildArcPoints(origin, direction, innerRadius, halfAngle, -halfAngle));
            }

            return CreateLoopCurves(points);
        }

        private static IList<XYZ> BuildArcPoints(
            XYZ origin,
            XYZ direction,
            double radius,
            double startAngle,
            double endAngle,
            bool skipFirst = false)
        {
            var angle = Math.Abs(endAngle - startAngle);
            var segments = Math.Max(8, Math.Min(48, (int)Math.Ceiling(angle / (Math.PI / 48.0))));
            var points = new List<XYZ>();

            for (var i = 0; i <= segments; i++)
            {
                if (skipFirst && i == 0)
                {
                    continue;
                }

                var t = (double)i / segments;
                var currentAngle = startAngle + (endAngle - startAngle) * t;
                points.Add(PointOnRay(origin, direction, radius, currentAngle));
            }

            return points;
        }

        private static XYZ PointOnRay(XYZ origin, XYZ direction, double radius, double angle)
        {
            var left = new XYZ(-direction.Y, direction.X, 0);
            return origin
                + direction.Multiply(Math.Cos(angle) * radius)
                + left.Multiply(Math.Sin(angle) * radius);
        }

        private static IList<Curve> CreateLoopCurves(IList<XYZ> points)
        {
            var curves = new List<Curve>();
            for (var i = 0; i < points.Count; i++)
            {
                var next = i == points.Count - 1 ? points[0] : points[i + 1];
                if (points[i].DistanceTo(next) > MinLength)
                {
                    curves.Add(Line.CreateBound(points[i], next));
                }
            }

            return curves;
        }

        private static XYZ NormalizePlanVector(XYZ vector)
        {
            var plan = new XYZ(vector.X, vector.Y, 0);
            if (plan.GetLength() < MinLength)
            {
                return XYZ.BasisY;
            }

            return plan.Normalize();
        }
    }
}
