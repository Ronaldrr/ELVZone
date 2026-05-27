using System.Collections.Generic;
using Autodesk.Revit.DB;
using ELVZone.Core.Services;
using ELVZone.SplCoverage.Models;

namespace ELVZone.SplCoverage.Services
{
    public class SpeakerBindingService
    {
        private readonly RevitParameterService _parameters = new RevitParameterService();

        public IList<string> Bind(Document document, IEnumerable<ElementId> elementIds, SpeakerDefinition speaker, double powerW)
        {
            var missing = new SortedSet<string>();
            foreach (var id in elementIds)
            {
                var element = document.GetElement(id);
                if (element == null)
                {
                    continue;
                }

                Set(element, "SPL_Manufacturer", speaker.Manufacturer, missing);
                Set(element, "SPL_Model", speaker.Model, missing);
                Set(element, "SPL_Type", speaker.Type, missing);
                Set(element, "SPL_PowerW", powerW, missing);
                Set(element, "SPL_SensitivityDb", speaker.SensitivityDb1W1M, missing);
                Set(element, "SPL_LibraryFile", speaker.LibraryFile, missing);
            }

            return new List<string>(missing);
        }

        private void Set(Element element, string name, string value, ISet<string> missing)
        {
            if (!_parameters.TrySet(element, name, value)) missing.Add(name);
        }

        private void Set(Element element, string name, double value, ISet<string> missing)
        {
            if (!_parameters.TrySet(element, name, value)) missing.Add(name);
        }
    }
}
