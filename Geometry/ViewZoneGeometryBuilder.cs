using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace ELVZone.Geometry
{
    public class ViewZoneGeometryBuilder
    {
        private const double MinLength = 0.001;
        private const double RayTolerance = 0.01;

        public IList<ViewZoneBoundarySet> BuildZoneBoundaries(
            CameraViewZoneData data,
            double planeElevation,
            IList<PlanObstacleSegment> obstacles)
        {
            var result = new List<ViewZoneBoundarySet>();
            var origin = new XYZ(data.Origin.X, data.Origin.Y, planeElevation);
            var direction = NormalizePlanVector(data.Direction);
            var halfAngle = Math.Max(0.001, data.HorizontalAngleRadians / 2.0);
            var totalRadius = Math.Max(0, data.TotalLengthFeet);
            var rays = BuildVisibilityRays(
                origin,
                direction,
                halfAngle,
                totalRadius,
                data,
                planeElevation,
                obstacles ?? new List<PlanObstacleSegment>());
            var innerRadius = 0.0;

            for (var i = 0; i < 4; i++)
            {
                var zoneLength = data.ZoneLengthsFeet[i];
                var outerRadius = Math.Min(totalRadius, innerRadius + Math.Max(0, zoneLength));
                if (outerRadius - innerRadius > MinLength)
                {
                    foreach (var boundary in BuildClippedZoneBoundaries(rays, innerRadius, outerRadius))
                    {
                        result.Add(new ViewZoneBoundarySet(i, boundary));
                    }
                }

                innerRadius = outerRadius;
            }

            return result;
        }

        private static IList<VisibilityRay> BuildVisibilityRays(
            XYZ origin,
            XYZ direction,
            double halfAngle,
            double totalRadius,
            CameraViewZoneData data,
            double planeElevation,
            IList<PlanObstacleSegment> obstacles)
        {
            var segments = Math.Max(24, Math.Min(144, (int)Math.Ceiling((halfAngle * 2.0) / (Math.PI / 90.0))));
            var rays = new List<VisibilityRay>();

            for (var i = 0; i <= segments; i++)
            {
                var t = (double)i / segments;
                var angle = -halfAngle + halfAngle * 2.0 * t;
                var rayDirection = RotatePlanVector(direction, angle);
                var distance = FindNearestObstacleDistance(
                    origin,
                    rayDirection,
                    totalRadius,
                    data,
                    planeElevation,
                    obstacles);
                rays.Add(new VisibilityRay(origin, rayDirection, angle, distance));
            }

            return rays;
        }

        private static IList<IList<Curve>> BuildClippedZoneBoundaries(
            IList<VisibilityRay> rays,
            double innerRadius,
            double outerRadius)
        {
            var boundaries = new List<IList<Curve>>();
            var current = new List<VisibilityRay>();

            foreach (var ray in rays)
            {
                if (ray.Distance > innerRadius + MinLength)
                {
                    current.Add(ray);
                    continue;
                }

                AddBoundaryIfValid(current, innerRadius, outerRadius, boundaries);
                current.Clear();
            }

            AddBoundaryIfValid(current, innerRadius, outerRadius, boundaries);
            return boundaries;
        }

        private static void AddBoundaryIfValid(
            IList<VisibilityRay> rays,
            double innerRadius,
            double outerRadius,
            IList<IList<Curve>> boundaries)
        {
            if (rays.Count < 2)
            {
                return;
            }

            var points = new List<XYZ>();
            if (innerRadius <= MinLength)
            {
                points.Add(PointAt(rays[0], 0));
            }
            else
            {
                points.Add(PointAt(rays[0], innerRadius));
            }

            foreach (var ray in rays)
            {
                var visibleOuterRadius = Math.Min(outerRadius, ray.Distance);
                if (visibleOuterRadius > innerRadius + MinLength)
                {
                    points.Add(PointAt(ray, visibleOuterRadius));
                }
            }

            if (innerRadius > MinLength)
            {
                for (var i = rays.Count - 1; i >= 0; i--)
                {
                    points.Add(PointAt(rays[i], innerRadius));
                }
            }

            var curves = CreateLoopCurves(points);
            if (curves.Count >= 3)
            {
                boundaries.Add(curves);
            }
        }

        private static double FindNearestObstacleDistance(
            XYZ origin,
            XYZ rayDirection,
            double maxDistance,
            CameraViewZoneData data,
            double planeElevation,
            IList<PlanObstacleSegment> obstacles)
        {
            var nearest = maxDistance;
            foreach (var obstacle in obstacles)
            {
                if (TryIntersectRaySegment(origin, rayDirection, obstacle.Start, obstacle.End, out var distance) &&
                    distance > RayTolerance &&
                    distance < nearest &&
                    IntersectsVerticalAnalysisWindow(data, planeElevation, distance, obstacle))
                {
                    nearest = distance;
                }
            }

            return nearest;
        }

        private static bool IntersectsVerticalAnalysisWindow(
            CameraViewZoneData data,
            double planeElevation,
            double distance,
            PlanObstacleSegment obstacle)
        {
            var halfVerticalAngle = Math.Max(0.001, data.VerticalAngleRadians / 2.0);
            var cameraZ = planeElevation + data.MountingHeightFeet;
            var verticalOffset = distance * Math.Tan(halfVerticalAngle);
            var viewBottom = cameraZ - verticalOffset;
            var viewTop = cameraZ + verticalOffset;
            var analysisBottom = planeElevation + data.AnalysisBottomHeightFeet;
            var analysisTop = planeElevation + data.AnalysisTopHeightFeet;

            if (analysisTop < analysisBottom)
            {
                var temp = analysisBottom;
                analysisBottom = analysisTop;
                analysisTop = temp;
            }

            var effectiveBottom = Math.Max(viewBottom, analysisBottom);
            var effectiveTop = Math.Min(viewTop, analysisTop);
            if (effectiveTop < effectiveBottom)
            {
                return false;
            }

            return obstacle.MaxZ >= effectiveBottom && obstacle.MinZ <= effectiveTop;
        }

        private static bool TryIntersectRaySegment(
            XYZ rayOrigin,
            XYZ rayDirection,
            XYZ segmentStart,
            XYZ segmentEnd,
            out double distance)
        {
            distance = 0;
            var segment = segmentEnd - segmentStart;
            var denominator = Cross2D(rayDirection, segment);
            if (Math.Abs(denominator) < 1e-9)
            {
                return false;
            }

            var diff = segmentStart - rayOrigin;
            var rayParameter = Cross2D(diff, segment) / denominator;
            var segmentParameter = Cross2D(diff, rayDirection) / denominator;
            if (rayParameter < 0 || segmentParameter < 0 || segmentParameter > 1)
            {
                return false;
            }

            distance = rayParameter;
            return true;
        }

        private static XYZ PointAt(VisibilityRay ray, double radius)
        {
            return ray.Origin + ray.Direction.Multiply(radius);
        }

        private static XYZ RotatePlanVector(XYZ direction, double angle)
        {
            var left = new XYZ(-direction.Y, direction.X, 0);
            return (direction.Multiply(Math.Cos(angle)) + left.Multiply(Math.Sin(angle))).Normalize();
        }

        private static double Cross2D(XYZ first, XYZ second)
        {
            return first.X * second.Y - first.Y * second.X;
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

        private class VisibilityRay
        {
            public double Angle { get; }
            public double Distance { get; }
            public XYZ Origin { get; }
            public XYZ Direction { get; }

            public VisibilityRay(XYZ origin, XYZ direction, double angle, double distance)
            {
                Origin = origin;
                Direction = direction;
                Angle = angle;
                Distance = distance;
            }
        }
    }
}
