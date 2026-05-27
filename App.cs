using System;
using System.Reflection;
using Autodesk.Revit.UI;

namespace ELVZone
{
    public class App : IExternalApplication
    {
        private const string TabName = "Coverage Tools";
        private const string PanelName = "Coverage Tools";

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                try
                {
                    application.CreateRibbonTab(TabName);
                }
                catch
                {
                    // Ribbon tab already exists.
                }

                var panel = application.CreateRibbonPanel(TabName, PanelName);
                var assemblyPath = Assembly.GetExecutingAssembly().Location;

                AddButton(panel, "CameraCoverage", "Camera\nCoverage", assemblyPath, "ELVZone.Commands.PlaceViewZonesCommand", "Построить зоны обзора камеры");
                AddButton(panel, "CameraSettings", "Camera\nSettings", assemblyPath, "ELVZone.Commands.OpenViewZoneSettingsCommand", "Настройки зон обзора камер");
                AddButton(panel, "SplCalculate", "SPL\nCalculate", assemblyPath, "ELVZone.SplCoverage.Commands.SplCalculateCommand", "Рассчитать SPL и разместить карту покрытия");
                AddButton(panel, "SplSpeakerManager", "SPL Speaker\nManager", assemblyPath, "ELVZone.SplCoverage.Commands.SplSpeakerManagerCommand", "Библиотека и привязка громкоговорителей");
                AddButton(panel, "SplSettings", "SPL\nSettings", assemblyPath, "ELVZone.SplCoverage.Commands.SplSettingsCommand", "Настройки SPL расчета");
                AddButton(panel, "SplClear", "SPL\nClear", assemblyPath, "ELVZone.SplCoverage.Commands.SplClearCommand", "Очистить визуализацию SPL");

                return Result.Succeeded;
            }
            catch (Exception exception)
            {
                TaskDialog.Show("Coverage Tools", exception.Message);
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private static void AddButton(RibbonPanel panel, string name, string text, string assemblyPath, string className, string tooltip)
        {
            var data = new PushButtonData(name, text, assemblyPath, className)
            {
                ToolTip = tooltip
            };
            panel.AddItem(data);
        }
    }
}
