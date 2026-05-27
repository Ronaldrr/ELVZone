using System.Windows;
using ELVZone.SharedUI;
using ELVZone.SplCoverage.ViewModels;

namespace ELVZone.SplCoverage.Views
{
    public partial class SplSettingsWindow : Window
    {
        public SplSettingsWindow()
        {
            InitializeComponent();
        }

        private SplSettingsViewModel ViewModel => DataContext as SplSettingsViewModel;
        private void SaveButton_Click(object sender, RoutedEventArgs e) { BindingFlush.Flush(this); ViewModel?.SaveCommand.Execute(null); }
        private void ImportButton_Click(object sender, RoutedEventArgs e) { BindingFlush.Flush(this); ViewModel?.ImportCommand.Execute(null); }
        private void ExportButton_Click(object sender, RoutedEventArgs e) { BindingFlush.Flush(this); ViewModel?.ExportCommand.Execute(null); }
        private void ResetButton_Click(object sender, RoutedEventArgs e) { BindingFlush.Flush(this); ViewModel?.ResetCommand.Execute(null); }
    }
}
