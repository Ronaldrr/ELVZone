using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace ELVZone.Geometry
{
    public class ViewZoneBoundarySet
    {
        public int ZoneIndex { get; }
        public IList<Curve> Curves { get; }

        public ViewZoneBoundarySet(int zoneIndex, IList<Curve> curves)
        {
            ZoneIndex = zoneIndex;
            Curves = curves;
        }
    }
}
