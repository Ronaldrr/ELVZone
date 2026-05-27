using Autodesk.Revit.DB;

namespace ELVZone.Geometry
{
    public class PlanObstacleSegment
    {
        public XYZ Start { get; }
        public XYZ End { get; }
        public double MinZ { get; }
        public double MaxZ { get; }

        public PlanObstacleSegment(XYZ start, XYZ end, double minZ, double maxZ)
        {
            Start = new XYZ(start.X, start.Y, 0);
            End = new XYZ(end.X, end.Y, 0);
            MinZ = minZ;
            MaxZ = maxZ;
        }
    }
}
