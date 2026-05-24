using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ELVZone.Geometry;
using ELVZone.Models;

namespace ELVZone.Services
{
    public class ViewZonePlacementService
    {
        private readonly ViewZoneGeometryBuilder _geometryBuilder = new ViewZoneGeometryBuilder();

        public int PlaceZones(Document document, ViewPlan view, CameraViewZoneData data, ViewZoneSettings settings)
        {
            var elevation = view.GenLevel != null ? view.GenLevel.Elevation : data.Origin.Z;
            var boundaries = _geometryBuilder.BuildZoneBoundaries(data, elevation);
            var placed = 0;

            for (var i = 0; i < boundaries.Count; i++)
            {
                var curves = boundaries[i];
                if (curves.Count == 0)
                {
                    continue;
                }

                var style = settings.ZoneStyles[i];
                if (style.FillEnabled)
                {
                    var filledRegionType = FindFilledRegionType(document, style.FilledRegionTypeName);
                    if (filledRegionType != null)
                    {
                        var loop = new CurveLoop();
                        foreach (var curve in curves)
                        {
                            loop.Append(curve);
                        }

                        FilledRegion.Create(
                            document,
                            filledRegionType.Id,
                            view.Id,
                            new List<CurveLoop> { loop });
                    }
                }

                if (style.LineEnabled)
                {
                    var lineStyle = FindLineStyle(document, style.LineStyleName);
                    foreach (var curve in curves)
                    {
                        var detailCurve = document.Create.NewDetailCurve(view, curve);
                        if (lineStyle != null)
                        {
                            detailCurve.LineStyle = lineStyle;
                        }
                    }
                }

                placed++;
            }

            return placed;
        }

        private static FilledRegionType FindFilledRegionType(Document document, string name)
        {
            var types = new FilteredElementCollector(document)
                .OfClass(typeof(FilledRegionType))
                .Cast<FilledRegionType>()
                .ToList();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var selected = types.FirstOrDefault(type => type.Name == name);
                if (selected != null)
                {
                    return selected;
                }
            }

            return types.FirstOrDefault();
        }

        private static GraphicsStyle FindLineStyle(Document document, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var lines = document.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);
            foreach (Category category in lines.SubCategories)
            {
                if (category.Name == name)
                {
                    return category.GetGraphicsStyle(GraphicsStyleType.Projection);
                }
            }

            return null;
        }
    }
}
