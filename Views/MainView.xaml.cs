using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClauOS.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ClauOS.Views
{
    public partial class MainView : UserControl
    {
        private readonly HomeTab _homeTab;

        public MainView()
        {
            InitializeComponent();
            // Reutilizar la instancia creada en XAML: evita un HomeTabViewModel
            // (y su DispatcherTimer) nuevo en cada vuelta al inicio.
            _homeTab = (HomeTab)TabHost.Content;
        }

        public void ShowFood()
        {
            TabHost.Content = new FoodTab();
        }

        public void ShowHome()
        {
            TabHost.Content = _homeTab;
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
                
                // Aplicar tema inmediatamente (implementación única en App)
                App.ApplyTheme(theme);
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ClauOS v1.0\nHecho con ❤️ para Claudia", "Acerca de", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}