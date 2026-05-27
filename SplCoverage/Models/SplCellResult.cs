using Autodesk.Revit.DB;

namespace ELVZone.SplCoverage.Models
{
    public class SplCellResult
    {
        public XYZ Center { get; set; }
        public double Spl { get; set; }
        public string Range { get; set; }
        public string TypeName { get; set; }
        public string SourceSpeakers { get; set; }
    }
}
