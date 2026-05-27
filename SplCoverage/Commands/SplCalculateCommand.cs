using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ELVZone.Core.Utils;
using ELVZone.SplCoverage.Services;

namespace ELVZone.SplCoverage.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class SplCalculateCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDocument = commandData.Application.ActiveUIDocument;
            var document = uiDocument.Document;

            try
            {
                if (!(document.ActiveView is ViewPlan viewPlan))
                {
                    TaskDialog.Show("Coverage Tools", "SPL расчет работает только на активном плане.");
                    return Result.Cancelled;
                }

                var roomOrSpace = PickSpatialElement(uiDocument);
                if (roomOrSpace == null)
                {
                    return Result.Cancelled;
                }

                var settings = new SplSettingsService().Load();
                var spatial = new SpatialElementService();
                var bounds = spatial.GetBounds(roomOrSpace);
                if (bounds == null)
                {
                    TaskDialog.Show("Coverage Tools", "Не удалось получить границы выбранного помещения.");
                    return Result.Cancelled;
                }

                var containsPoint = spatial.CreateContainsPoint(roomOrSpace);
                var speakers = new SpeakerCollectorService().Collect(document, containsPoint, settings);
                if (speakers.Count == 0)
                {
                    TaskDialog.Show("Coverage Tools", "В выбранном помещении не найдены привязанные громкоговорители SPL.");
                    return Result.Cancelled;
                }

                var baseElevation = viewPlan.GenLevel != null ? viewPlan.GenLevel.Elevation : bounds.Min.Z;
                var elevation = baseElevation + UnitConversion.MetersToFeet(settings.CalculationPlaneHeightMeters);
                var cells = new SplCalculationService().Calculate(bounds, containsPoint, speakers, settings, elevation);
                if (cells.Count == 0)
                {
                    TaskDialog.Show("Coverage Tools", "Расчетная сетка пуста. Проверьте помещение и размер ячейки.");
                    return Result.Cancelled;
                }

                var calculationId = Guid.NewGuid().ToString("N");
                var roomName = spatial.GetName(roomOrSpace);
                int placed;
                using (var transaction = new Transaction(document, "SPL Calculate"))
                {
                    transaction.Start();
                    placed = new SplVisualizationService().Place(document, viewPlan, roomOrSpace, roomName, calculationId, cells, settings);
                    transaction.Commit();
                }

                if (placed == 0)
                {
                    TaskDialog.Show("Coverage Tools", "SPL рассчитан, но ячейки не размещены. Проверьте семейство SPL_Cell и типы SPL_Grey/SPL_Yellow/SPL_Orange/SPL_Red.");
                    return Result.Cancelled;
                }

                TaskDialog.Show("Coverage Tools", $"SPL расчет завершен. Громкоговорителей: {speakers.Count}. Ячеек: {placed}.");
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception exception)
            {
                message = exception.Message;
                return Result.Failed;
            }
        }

        private static Element PickSpatialElement(UIDocument uiDocument)
        {
            foreach (var id in uiDocument.Selection.GetElementIds())
            {
                var element = uiDocument.Document.GetElement(id);
                if (new SpatialElementSelectionFilter().AllowElement(element))
                {
                    return element;
                }
            }

            var reference = uiDocument.Selection.PickObject(ObjectType.Element, new SpatialElementSelectionFilter(), "Выберите Room или Space для SPL расчета");
            return reference == null ? null : uiDocument.Document.GetElement(reference);
        }
    }
}
