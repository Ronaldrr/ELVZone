using System;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ELVZone.Services;
using ELVZone.ViewModels;
using ELVZone.Views;

namespace ELVZone.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class OpenViewZoneSettingsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var uiApplication = commandData.Application;
                var document = uiApplication.ActiveUIDocument.Document;
                var settingsService = new ViewZoneSettingsService();
                var settings = settingsService.Load();
                var optionsService = new RevitOptionsService();
                var viewModel = new ViewZoneSettingsViewModel(
                    settings,
                    settingsService,
                    optionsService,
                    document,
                    uiApplication.ActiveUIDocument.Selection.GetElementIds());
                var window = new ViewZoneSettingsWindow
                {
                    DataContext = viewModel
                };

                new WindowInteropHelper(window)
                {
                    Owner = uiApplication.MainWindowHandle
                };

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
