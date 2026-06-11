using Autodesk.Revit.DB;

namespace ELVZone.Geometry
{
    public class CameraViewZoneData
    {
        public XYZ Origin { get; set; }
        public XYZ Direction { get; set; }
        public double HorizontalAngleRadians { get; set; }
        public double VerticalAngleRadians { get; set; }
        public double MountingHeightFeet { get; set; }
        public double DeadZoneLengthFeet { get; set; }
        public double[] ZoneLengthsFeet { get; set; }
        public double TotalLengthFeet { get; set; }
        public double AnalysisBottomHeightFeet { get; set; }
        public double AnalysisTopHeightFeet { get; set; }
    }
}
