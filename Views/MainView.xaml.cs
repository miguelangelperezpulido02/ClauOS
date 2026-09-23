using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClauOS.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ClauOS.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        public void ShowFood()
        {
            TabHost.Content = new FoodTab();
        }

        public void ShowHome()
        {
            TabHost.Content = new HomeTab();
        }

        public static MainView? FindMain(DependencyObject child)
        {
            DependencyObject? current = child;
            while (current != null)
            {
                if (current is MainView main)
                    return main;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void Theme_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi && mi.Tag is string theme)
            {
                var settings = App.AppHost?.Services.GetService<ClauOS.Services.ISettingsService>();
                settings?.SetSetting("Theme", theme);
                settings?.Save();
                
                // Aplicar tema inmediatamente
                ApplyTheme(theme);
            }
        }

        private void ApplyTheme(string themeName)
        {
            try
            {
                var dict = new ResourceDictionary
                {
                    Source = new Uri($"pack://application:,,,/Resources/Themes/Theme{themeName}.xaml", UriKind.Absolute)
                };
                if (Application.Current.Resources.MergedDictionaries.Count > 0)
                    Application.Current.Resources.MergedDictionaries[0] = dict;
                else
                    Application.Current.Resources.MergedDictionaries.Insert(0, dict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error aplicando tema {themeName}: {ex.Message}");
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ClauOS v1.0\nHecho con ❤️ para Claudia", "Acerca de", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}