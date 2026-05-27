using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ELVZone.SplCoverage.Services;

namespace ELVZone.SplCoverage.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class SplClearCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDocument = commandData.Application.ActiveUIDocument;
            var document = uiDocument.Document;

            try
            {
                var filter = new SpatialElementSelectionFilter();
                var roomOrSpace = uiDocument.Selection.GetElementIds()
                    .Select(id => document.GetElement(id))
                    .FirstOrDefault(filter.AllowElement);
                var settings = new SplSettingsService().Load();
                int deleted;
                using (var transaction = new Transaction(document, "SPL Clear"))
                {
                    transaction.Start();
                    deleted = new SplClearService().Clear(document, settings, roomOrSpace);
                    transaction.Commit();
                }

                TaskDialog.Show("Coverage Tools", roomOrSpace == null ? $"Удалено SPL-ячеек: {deleted}." : $"Удалено SPL-ячеек выбранного помещения: {deleted}.");
                return Result.Succeeded;
            }
            catch (Exception exception)
            {
                message = exception.Message;
                return Result.Failed;
            }
        }
    }
}
