using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.DB;
using ELVZone.SplCoverage.Models;
using ELVZone.SplCoverage.Services;
using ELVZone.ViewModels;
using Microsoft.Win32;

namespace ELVZone.SplCoverage.ViewModels
{
    public class SplSettingsViewModel : ViewModelBase
    {
        private readonly SplSettingsService _settingsService;
        private SplSettings _settings;
        private string _statusMessage;

        public ObservableCollection<string> ParameterNames { get; }

        public SplSettings Settings
        {
            get => _settings;
            set { _settings = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ResetCommand { get; }

        public SplSettingsViewModel(SplSettings settings, SplSettingsService settingsService, Document document)
        {
            _settings = settings;
            _settingsService = settingsService;
            ParameterNames = new ObservableCollection<string>(GetParameterNames(document));
            SaveCommand = new RelayCommand(_ => Save());
            ImportCommand = new RelayCommand(_ => Import());
            ExportCommand = new RelayCommand(_ => Export());
            ResetCommand = new RelayCommand(_ => Reset());
        }

        private static IEnumerable<string> GetParameterNames(Document document)
        {
            var names = new SortedSet<string>();
            if (document == null)
            {
                return names;
            }

            var iterator = document.ParameterBindings.ForwardIterator();
            iterator.Reset();
            while (iterator.MoveNext())
            {
                var name = iterator.Key?.Name;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    names.Add(name);
                }
            }

            var instances = new FilteredElementCollector(document)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType();

            foreach (var element in instances)
            {
                foreach (Parameter parameter in element.Parameters)
                {
                    var name = parameter.Definition?.Name;
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        names.Add(name);
                    }
                }
            }

            return names;
        }

        private void Save()
        {
            _settingsService.Save(Settings);
            StatusMessage = $"Настройки сохранены: {_settingsService.SettingsPath}";
        }

        private void Import()
        {
            var dialog = new OpenFileDialog { Filter = "JSON settings (*.json)|*.json|All files (*.*)|*.*", Title = "Импорт настроек SPL" };
            if (dialog.ShowDialog() != true) return;
            Settings = _settingsService.LoadFrom(dialog.FileName);
            _settingsService.Save(Settings);
            StatusMessage = "Настройки импортированы.";
        }

        private void Export()
        {
            var dialog = new SaveFileDialog { Filter = "JSON settings (*.json)|*.json|All files (*.*)|*.*", FileName = "spl-settings.json", Title = "Экспорт настроек SPL" };
            if (dialog.ShowDialog() != true) return;
            _settingsService.SaveTo(dialog.FileName, Settings);
            StatusMessage = "Настройки экспортированы.";
        }

        private void Reset()
        {
            if (MessageBox.Show("Сбросить настройки SPL?", "Coverage Tools", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            Settings = SplSettings.CreateDefault();
            _settingsService.Save(Settings);
            StatusMessage = "Настройки сброшены.";
        }
    }
}
