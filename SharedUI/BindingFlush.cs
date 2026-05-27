using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ELVZone.SharedUI
{
    public static class BindingFlush
    {
        public static void Flush(DependencyObject root)
        {
            FocusManager.SetFocusedElement(root, root as IInputElement);
            Keyboard.ClearFocus();
            UpdateBindings(root);
        }

        private static void UpdateBindings(DependencyObject element)
        {
            if (element == null) return;
            UpdateBinding(element, TextBox.TextProperty);
            UpdateBinding(element, ComboBox.TextProperty);
            UpdateBinding(element, ComboBox.SelectedItemProperty);
            UpdateBinding(element, CheckBox.IsCheckedProperty);
            var childrenCount = VisualTreeHelper.GetChildrenCount(element);
            for (var i = 0; i < childrenCount; i++) UpdateBindings(VisualTreeHelper.GetChild(element, i));
        }

        private static void UpdateBinding(DependencyObject element, DependencyProperty property)
        {
            BindingOperations.GetBindingExpression(element, property)?.UpdateSource();
        }
    }
}
