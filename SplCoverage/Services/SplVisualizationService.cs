using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ELVZone.Core.Services;
using ELVZone.SplCoverage.Models;

namespace ELVZone.SplCoverage.Services
{
    public class SplVisualizationService
    {
        private readonly RevitParameterService _parameters = new RevitParameterService();

        public int Place(Document document, ViewPlan view, Element roomOrSpace, string roomName, string calculationId, IList<SplCellResult> cells, SplSettings settings)
        {
            var symbols = GetCellSymbols(document, settings).ToDictionary(symbol => symbol.Name, symbol => symbol);
            var placed = 0;
            foreach (var cell in cells)
            {
                if (!symbols.TryGetValue(cell.TypeName, out var symbol))
                {
                    continue;
                }

                if (!symbol.IsActive)
                {
                    symbol.Activate();
                    document.Regenerate();
                }

                var instance = document.Create.NewFamilyInstance(cell.Center, symbol, view);
                _parameters.TrySet(instance, "SPL_Value", Math.Round(cell.Spl, 1).ToString(System.Globalization.CultureInfo.InvariantCulture));
                _parameters.TrySet(instance, "SPL_Range", cell.Range);
                _parameters.TrySet(instance, "SPL_RoomId", roomOrSpace.Id.IntegerValue.ToString());
                _parameters.TrySet(instance, "SPL_RoomName", roomName);
                _parameters.TrySet(instance, "SPL_SourceSpeakers", cell.SourceSpeakers);
                _parameters.TrySet(instance, "SPL_CalculationId", calculationId);
                placed++;
            }

            return placed;
        }

        private static IEnumerable<FamilySymbol> GetCellSymbols(Document document, SplSettings settings)
        {
            var required = new[] { settings.GreyTypeName, settings.YellowTypeName, settings.OrangeTypeName, settings.RedTypeName };
            return new FilteredElementCollector(document)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .Cast<FamilySymbol>()
                .Where(symbol => symbol.FamilyName == settings.CellFamilyName && required.Contains(symbol.Name));
        }
    }
}
