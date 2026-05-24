using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.DB;
using ELVZone.Models;
using ELVZone.Services;
using Microsoft.Win32;

namespace ELVZone.ViewModels
{
    public class ViewZoneSettingsViewModel : ViewModelBase
    {
        private readonly ViewZoneSettingsService _settingsService;
        private ViewZoneSettings _settings;
        private string _statusMessage;

        public ObservableCollection<string> ParameterNames { get; }
        public ObservableCollection<string> FilledRegionTypeNames { get; }
        public ObservableCollection<string> LineStyleNames { get; }
        public ObservableCollection<ZoneStyleRowViewModel> ZoneRows { get; }

        public ICommand SaveCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ResetCommand { get; }

        public ViewZoneSettings Settings
        {
            get => _settings;
            private set
            {
                _settings = value;
                _settings.EnsureShape();
                RebuildZoneRows();
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ViewZoneSettingsViewModel(
            ViewZoneSettings settings,
            ViewZoneSettingsService settingsService,
            RevitOptionsService optionsService,
            Document document,
            IEnumerable<ElementId> preferredElementIds)
        {
            _settingsService = settingsService;
            ParameterNames = new ObservableCollection<string>(optionsService.GetParameterNames(document, preferredElementIds));
            FilledRegionTypeNames = new ObservableCollection<string>(optionsService.GetFilledRegionTypeNames(document));
            LineStyleNames = new ObservableCollection<string>(optionsService.GetLineStyleNames(document));
            ZoneRows = new ObservableCollection<ZoneStyleRowViewModel>();
            _settings = settings;
            _settings.EnsureShape();
            RebuildZoneRows();

            SaveCommand = new RelayCommand(_ => Save());
            ImportCommand = new RelayCommand(_ => Import());
            ExportCommand = new RelayCommand(_ => Export());
            ResetCommand = new RelayCommand(_ => Reset());
        }

        private void Save()
        {
            try
            {
                _settingsService.Save(Settings);
                StatusMessage = $"Настройки сохранены: {_settingsService.SettingsPath}";
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message, "Ошибка сохранения настроек", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Import()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON settings (*.json)|*.json|All files (*.*)|*.*",
                Title = "Импорт настроек зон обзора"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                Settings = _settingsService.LoadFrom(dialog.FileName);
                _settingsService.Save(Settings);
                StatusMessage = "Настройки импортированы.";
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message, "Ошибка импорта настроек", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Export()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON settings (*.json)|*.json|All files (*.*)|*.*",
                FileName = "view-zone-settings.json",
                Title = "Экспорт настроек зон обзора"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                _settingsService.SaveTo(dialog.FileName, Settings);
                StatusMessage = "Настройки экспортированы.";
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message, "Ошибка экспорта настроек", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Reset()
        {
            if (MessageBox.Show(
                    "Сбросить настройки зон обзора?",
                    "ELVZone",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            Settings = ViewZoneSettings.CreateDefault();
            _settingsService.Save(Settings);
            StatusMessage = "Настройки сброшены.";
        }

        private void RebuildZoneRows()
        {
            ZoneRows.Clear();
            for (var i = 0; i < 4; i++)
            {
                ZoneRows.Add(new ZoneStyleRowViewModel($"Зона {i + 1}", _settings.ZoneStyles[i]));
            }
        }
    }
}
