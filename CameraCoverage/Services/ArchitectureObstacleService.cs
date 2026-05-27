using System.Collections.Generic;
using Autodesk.Revit.DB;
using ELVZone.Geometry;

namespace ELVZone.Services
{
    public class ArchitectureObstacleService
    {
        private const double MinSegmentLength = 0.01;

        private static readonly BuiltInCategory[] BlockingCategories =
        {
            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_Columns,
            BuiltInCategory.OST_StructuralColumns
        };

        public IList<PlanObstacleSegment> Collect(Document document, ViewPlan view)
        {
            var result = new List<PlanObstacleSegment>();
            CollectHostObstacles(document, view, result);
            CollectLinkedObstacles(document, view, result);
            return result;
        }

        private static void CollectHostObstacles(Document document, ViewPlan view, IList<PlanObstacleSegment> result)
        {
            foreach (var category in BlockingCategories)
            {
                var elements = new FilteredElementCollector(document, view.Id)
                    .OfCategory(category)
                    .WhereElementIsNotElementType();

                foreach (var element in elements)
                {
                    AddElementSegments(element, Transform.Identity, result);
                }
            }
        }

        private static void CollectLinkedObstacles(Document document, ViewPlan view, IList<PlanObstacleSegment> result)
        {
            var links = new FilteredElementCollector(document, view.Id)
                .OfClass(typeof(RevitLinkInstance))
                .WhereElementIsNotElementType();

            foreach (RevitLinkInstance link in links)
            {
                var linkDocument = link.GetLinkDocument();
                if (linkDocument == null)
                {
                    continue;
                }

                var transform = link.GetTotalTransform();
                foreach (var category in BlockingCategories)
                {
                    var elements = new FilteredElementCollector(linkDocument)
                        .OfCategory(category)
                        .WhereElementIsNotElementType();

                    foreach (var element in elements)
                    {
                        AddElementSegments(element, transform, result);
                    }
                }
            }
        }

        private static void AddElementSegments(Element element, Transform transform, IList<PlanObstacleSegment> result)
        {
            var box = element.get_BoundingBox(null);
            if (box == null)
            {
                return;
            }

            GetTransformedZRange(box, transform, out var minZ, out var maxZ);
            if (element.Location is LocationCurve locationCurve)
            {
                AddCurveSegments(locationCurve.Curve, transform, minZ, maxZ, result);
                return;
            }

            AddBoxSegments(box, transform, minZ, maxZ, result);
        }

        private static void AddCurveSegments(
            Curve curve,
            Transform transform,
            double minZ,
            double maxZ,
            IList<PlanObstacleSegment> result)
        {
            if (curve == null)
            {
                return;
            }

            var points = curve.Tessellate();
            for (var i = 0; i < points.Count - 1; i++)
            {
                AddSegment(transform.OfPoint(points[i]), transform.OfPoint(points[i + 1]), minZ, maxZ, result);
            }
        }

        private static void AddBoxSegments(
            BoundingBoxXYZ box,
            Transform transform,
            double minZ,
            double maxZ,
            IList<PlanObstacleSegment> result)
        {
            var min = box.Min;
            var max = box.Max;
            var p1 = transform.OfPoint(new XYZ(min.X, min.Y, min.Z));
            var p2 = transform.OfPoint(new XYZ(max.X, min.Y, min.Z));
            var p3 = transform.OfPoint(new XYZ(max.X, max.Y, min.Z));
            var p4 = transform.OfPoint(new XYZ(min.X, max.Y, min.Z));

            AddSegment(p1, p2, minZ, maxZ, result);
            AddSegment(p2, p3, minZ, maxZ, result);
            AddSegment(p3, p4, minZ, maxZ, result);
            AddSegment(p4, p1, minZ, maxZ, result);
        }

        private static void AddSegment(XYZ start, XYZ end, double minZ, double maxZ, IList<PlanObstacleSegment> result)
        {
            var planStart = new XYZ(start.X, start.Y, 0);
            var planEnd = new XYZ(end.X, end.Y, 0);
            if (planStart.DistanceTo(planEnd) < MinSegmentLength)
            {
                return;
            }

            result.Add(new PlanObstacleSegment(planStart, planEnd, minZ, maxZ));
        }

        private static void GetTransformedZRange(BoundingBoxXYZ box, Transform transform, out double minZ, out double maxZ)
        {
            minZ = double.MaxValue;
            maxZ = double.MinValue;

            var min = box.Min;
            var max = box.Max;
            var points = new[]
            {
                new XYZ(min.X, min.Y, min.Z),
                new XYZ(min.X, min.Y, max.Z),
                new XYZ(min.X, max.Y, min.Z),
                new XYZ(min.X, max.Y, max.Z),
                new XYZ(max.X, min.Y, min.Z),
                new XYZ(max.X, min.Y, max.Z),
                new XYZ(max.X, max.Y, min.Z),
                new XYZ(max.X, max.Y, max.Z)
            };

            foreach (var point in points)
            {
                var transformed = transform.OfPoint(point);
                if (transformed.Z < minZ)
                {
                    minZ = transformed.Z;
                }

                if (transformed.Z > maxZ)
                {
                    maxZ = transformed.Z;
                }
            }
        }
    }
}
