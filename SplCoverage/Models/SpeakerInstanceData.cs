using Autodesk.Revit.DB;

namespace ELVZone.SplCoverage.Models
{
    public class SpeakerInstanceData
    {
        public Element Element { get; set; }
        public XYZ Location { get; set; }
        public XYZ Direction { get; set; }
        public SpeakerDefinition Definition { get; set; }
        public double PowerW { get; set; }
        public double SensitivityDb { get; set; }
    }
}
