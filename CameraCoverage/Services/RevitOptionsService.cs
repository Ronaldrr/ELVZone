using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace ELVZone.Services
{
    public class RevitOptionsService
    {
        public IList<string> GetParameterNames(Document document, IEnumerable<ElementId> preferredElementIds = null)
        {
            var names = new SortedSet<string>();
            AddProjectParameterNames(document, names);

            if (preferredElementIds != null)
            {
                foreach (var elementId in preferredElementIds)
                {
                    var element = document.GetElement(elementId);
                    AddElementParameterNames(element, names);
                }
            }

            var instances = new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(FamilyInstance));

            foreach (var element in instances)
            {
                AddElementParameterNames(element, names);
            }

            return names.ToList();
        }

        public IList<string> GetFilledRegionTypeNames(Document document)
        {
            return new FilteredElementCollector(document)
                .OfClass(typeof(FilledRegionType))
                .Cast<FilledRegionType>()
                .Select(type => type.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .OrderBy(name => name)
                .ToList();
        }

        public IList<string> GetLineStyleNames(Document document)
        {
            var names = new SortedSet<string>();
            var lines = document.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);

            foreach (Category category in lines.SubCategories)
            {
                if (!string.IsNullOrWhiteSpace(category.Name))
                {
                    names.Add(category.Name);
                }
            }

            return names.ToList();
        }

        private static void AddProjectParameterNames(Document document, ISet<string> names)
        {
            var iterator = document.ParameterBindings.ForwardIterator();
            iterator.Reset();
            while (iterator.MoveNext())
            {
                var definition = iterator.Key;
                if (!string.IsNullOrWhiteSpace(definition?.Name))
                {
                    names.Add(definition.Name);
                }
            }
        }

        private static void AddElementParameterNames(Element element, ISet<string> names)
        {
            if (element == null)
            {
                return;
            }

            foreach (Parameter parameter in element.Parameters)
            {
                var name = parameter.Definition?.Name;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    names.Add(name);
                }
            }
        }
    }
}
