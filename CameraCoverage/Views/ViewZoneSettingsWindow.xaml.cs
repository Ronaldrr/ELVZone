using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using ELVZone.ViewModels;

namespace ELVZone.Views
{
    public partial class ViewZoneSettingsWindow : Window
    {
        public ViewZoneSettingsWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            FlushBindings();
            ViewModel?.SaveCommand.Execute(null);
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            FlushBindings();
            ViewModel?.ImportCommand.Execute(null);
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            FlushBindings();
            ViewModel?.ExportCommand.Execute(null);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            FlushBindings();
            ViewModel?.ResetCommand.Execute(null);
        }

        private ViewZoneSettingsViewModel ViewModel => DataContext as ViewZoneSettingsViewModel;

        private void FlushBindings()
        {
            FocusManager.SetFocusedElement(this, this);
            Keyboard.ClearFocus();
            UpdateBindings(this);
        }

        private static void UpdateBindings(DependencyObject element)
        {
            if (element == null)
            {
                return;
            }

            UpdateBinding(element, TextBox.TextProperty);
            UpdateBinding(element, ComboBox.TextProperty);
            UpdateBinding(element, ComboBox.SelectedItemProperty);
            UpdateBinding(element, CheckBox.IsCheckedProperty);

            var childrenCount = VisualTreeHelper.GetChildrenCount(element);
            for (var i = 0; i < childrenCount; i++)
            {
                UpdateBindings(VisualTreeHelper.GetChild(element, i));
            }
        }

        private static void UpdateBinding(DependencyObject element, DependencyProperty property)
        {
            var expression = BindingOperations.GetBindingExpression(element, property);
            expression?.UpdateSource();
        }
    }
}
