using System;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ELVZone.SplCoverage.Services;
using ELVZone.SplCoverage.ViewModels;
using ELVZone.SplCoverage.Views;

namespace ELVZone.SplCoverage.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class SplSettingsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var service = new SplSettingsService();
                var document = commandData.Application.ActiveUIDocument.Document;
                var window = new SplSettingsWindow
                {
                    DataContext = new SplSettingsViewModel(service.Load(), service, document)
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
