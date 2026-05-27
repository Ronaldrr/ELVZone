using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace ELVZone.SplCoverage.Services
{
    public class DetailComponentSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is FamilyInstance;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
