using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;

namespace ELVZone.SplCoverage.Services
{
    public class SpatialElementService
    {
        public string GetName(Element element)
        {
            if (element is Room room) return room.Name;
            if (element is Space space) return space.Name;
            return element?.Name ?? string.Empty;
        }

        public BoundingBoxXYZ GetBounds(Element element)
        {
            return element?.get_BoundingBox(null);
        }

        public Func<XYZ, bool> CreateContainsPoint(Element element)
        {
            if (element is Room room)
            {
                return point => room.IsPointInRoom(point);
            }

            if (element is Space space)
            {
                return point => space.IsPointInSpace(point);
            }

            return point => false;
        }
    }
}
