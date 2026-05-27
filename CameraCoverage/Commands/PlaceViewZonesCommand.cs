using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ELVZone.Services;

namespace ELVZone.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class PlaceViewZonesCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDocument = commandData.Application.ActiveUIDocument;
            var document = uiDocument.Document;

            try
            {
                if (!(document.ActiveView is ViewPlan viewPlan))
                {
                    TaskDialog.Show("ELVZone", "Команда работает только на активном плане.");
                    return Result.Cancelled;
                }

                var camera = GetCameraElement(uiDocument);
                if (camera == null)
                {
                    return Result.Cancelled;
                }

                var settingsService = new ViewZoneSettingsService();
                var settings = settingsService.Load();
                var cameraData = new CameraDataFactory().Create(camera, settings);
                var placementService = new ViewZonePlacementService();

                using (var transaction = new Transaction(document, "Разместить зоны обзора камеры"))
                {
                    transaction.Start();
                    var placed = placementService.PlaceZones(document, viewPlan, cameraData, settings);
                    transaction.Commit();

                    TaskDialog.Show("ELVZone", $"Размещено зон обзора: {placed}.");
                }

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

        private static Element GetCameraElement(UIDocument uiDocument)
        {
            var selectedId = uiDocument.Selection
                .GetElementIds()
                .FirstOrDefault(id => id != ElementId.InvalidElementId);

            if (selectedId != null && selectedId != ElementId.InvalidElementId)
            {
                return uiDocument.Document.GetElement(selectedId);
            }

            var reference = uiDocument.Selection.PickObject(ObjectType.Element, "Выберите элемент камеры");
            return reference == null ? null : uiDocument.Document.GetElement(reference);
        }
    }
}
