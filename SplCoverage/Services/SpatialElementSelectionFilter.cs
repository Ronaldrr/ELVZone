using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI.Selection;

namespace ELVZone.SplCoverage.Services
{
    public class SpatialElementSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is Room || elem is Space;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
