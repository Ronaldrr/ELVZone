using System;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using System.Linq;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ELVZone.SplCoverage.Services;
using ELVZone.SplCoverage.ViewModels;
using ELVZone.SplCoverage.Views;

namespace ELVZone.SplCoverage.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class SplSpeakerManagerCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var uiDocument = commandData.Application.ActiveUIDocument;
                var settings = new SplSettingsService().Load();
                var selectedIds = uiDocument.Selection.GetElementIds();
                if (selectedIds.Count == 0)
                {
                    try
                    {
                        selectedIds = uiDocument.Selection
                            .PickObjects(ObjectType.Element, new DetailComponentSelectionFilter(), "Выберите элементы громкоговорителей для привязки SPL")
                            .Select(reference => reference.ElementId)
                            .ToList();
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        selectedIds = new System.Collections.Generic.List<ElementId>();
                    }
                }

                var window = new SpeakerManagerWindow
                {
                    DataContext = new SpeakerManagerViewModel(uiDocument.Document, selectedIds, settings)
                };
                new WindowInteropHelper(window) { Owner = commandData.Application.MainWindowHandle };
                window.ShowDialog();
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
