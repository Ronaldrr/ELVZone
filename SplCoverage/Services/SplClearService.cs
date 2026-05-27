using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ELVZone.Core.Services;
using ELVZone.SplCoverage.Models;

namespace ELVZone.SplCoverage.Services
{
    public class SplClearService
    {
        private readonly RevitParameterService _parameters = new RevitParameterService();

        public int Clear(Document document, SplSettings settings, Element roomOrSpace = null)
        {
            var ids = new List<ElementId>();
            var roomId = roomOrSpace?.Id.IntegerValue.ToString();
            var instances = new FilteredElementCollector(document)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>();

            foreach (var instance in instances)
            {
                if (instance.Symbol?.FamilyName != settings.CellFamilyName)
                {
                    continue;
                }

                var calculationId = _parameters.GetString(instance, "SPL_CalculationId");
                if (string.IsNullOrWhiteSpace(calculationId))
                {
                    continue;
                }

                if (roomId != null && _parameters.GetString(instance, "SPL_RoomId") != roomId)
                {
                    continue;
                }

                ids.Add(instance.Id);
            }

            if (ids.Count > 0)
            {
                document.Delete(ids);
            }

            return ids.Count;
        }
    }
}
