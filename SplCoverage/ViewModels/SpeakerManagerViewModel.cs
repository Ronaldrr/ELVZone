using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.DB;
using ELVZone.SplCoverage.Models;
using ELVZone.SplCoverage.Services;
using ELVZone.ViewModels;

namespace ELVZone.SplCoverage.ViewModels
{
    public class SpeakerManagerViewModel : ViewModelBase
    {
        private readonly Document _document;
        private readonly System.Collections.Generic.ICollection<ElementId> _selectedElementIds;
        private readonly SpeakerLibraryService _libraryService = new SpeakerLibraryService();
        private readonly SpeakerBindingService _bindingService = new SpeakerBindingService();
        private string _manufacturerFilter;
        private SpeakerDefinition _selectedSpeaker;
        private double _selectedPowerW;
        private int _selectedFrequencyHz;
        private string _statusMessage;

        public ObservableCollection<SpeakerDefinition> Speakers { get; } = new ObservableCollection<SpeakerDefinition>();
        public ObservableCollection<SpeakerDefinition> FilteredSpeakers { get; } = new ObservableCollection<SpeakerDefinition>();
        public ObservableCollection<double> PowerTaps { get; } = new ObservableCollection<double>();
        public ObservableCollection<int> Frequencies { get; } = new ObservableCollection<int>();
        public ICommand ReloadCommand { get; }
        public ICommand BindCommand { get; }
        public SplSettings Settings { get; }

        public string ManufacturerFilter
        {
            get => _manufacturerFilter;
            set { _manufacturerFilter = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public SpeakerDefinition SelectedSpeaker
        {
            get => _selectedSpeaker;
            set { _selectedSpeaker = value; OnPropertyChanged(); RebuildDetails(); }
        }

        public double SelectedPowerW
        {
            get => _selectedPowerW;
            set { _selectedPowerW = value; OnPropertyChanged(); }
        }

        public int SelectedFrequencyHz
        {
            get => _selectedFrequencyHz;
            set { _selectedFrequencyHz = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public SpeakerManagerViewModel(Document document, System.Collections.Generic.ICollection<ElementId> selectedElementIds, SplSettings settings)
        {
            _document = document;
            _selectedElementIds = selectedElementIds;
            Settings = settings;
            ReloadCommand = new RelayCommand(_ => Reload());
            BindCommand = new RelayCommand(_ => Bind(), _ => SelectedSpeaker != null && _selectedElementIds.Count > 0);
            Reload();
        }

        private void Reload()
        {
            Speakers.Clear();
            foreach (var speaker in _libraryService.Load(Settings.SpeakerLibraryPath)) Speakers.Add(speaker);
            ApplyFilter();
            StatusMessage = $"Загружено моделей: {Speakers.Count}. Выбрано элементов: {_selectedElementIds.Count}. Путь: {Settings.SpeakerLibraryPath}";
        }

        private void ApplyFilter()
        {
            FilteredSpeakers.Clear();
            var filtered = Speakers.Where(s => string.IsNullOrWhiteSpace(ManufacturerFilter) || (s.Manufacturer ?? string.Empty).ToLowerInvariant().Contains(ManufacturerFilter.ToLowerInvariant()));
            foreach (var speaker in filtered) FilteredSpeakers.Add(speaker);
            SelectedSpeaker = FilteredSpeakers.FirstOrDefault();
        }

        private void RebuildDetails()
        {
            PowerTaps.Clear();
            Frequencies.Clear();
            if (SelectedSpeaker == null) return;
            foreach (var power in SelectedSpeaker.PowerTapsW ?? new[] { SelectedSpeaker.RatedPowerW }) PowerTaps.Add(power);
            SelectedPowerW = PowerTaps.FirstOrDefault();
            var frequencyKeys = SelectedSpeaker.PolarPattern?.Frequencies?.Keys;
            if (frequencyKeys != null)
            {
                foreach (var key in frequencyKeys)
                {
                    if (int.TryParse(key, out var frequency)) Frequencies.Add(frequency);
                }
            }
            if (Frequencies.Count == 0) Frequencies.Add(Settings.CalculationFrequencyHz);
            SelectedFrequencyHz = Frequencies.FirstOrDefault();
        }

        private void Bind()
        {
            if (_selectedElementIds.Count == 0)
            {
                MessageBox.Show("Сначала выберите элементы громкоговорителей на плане, затем откройте SPL Speaker Manager.", "Coverage Tools", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            using (var transaction = new Transaction(_document, "Привязать громкоговоритель SPL"))
            {
                transaction.Start();
                var missing = _bindingService.Bind(_document, _selectedElementIds, SelectedSpeaker, SelectedPowerW);
                transaction.Commit();
                if (missing.Count > 0)
                {
                    MessageBox.Show("Не найдены или недоступны параметры:\n" + string.Join("\n", missing), "Coverage Tools", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            StatusMessage = $"Привязано элементов: {_selectedElementIds.Count}";
        }
    }
}
