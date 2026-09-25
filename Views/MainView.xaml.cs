using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

            var sounds = App.AppHost?.Services.GetService<ClauOS.Services.ISoundService>();
            UpdateSoundMenu(sounds);
        }

        public void ShowFood()
        {
            TabHost.Content = new FoodTab();
        }

        public void ShowCounter()
        {
            TabHost.Content = new CounterTab();
        }

        public void ShowTickets()
        {
            TabHost.Content = new TicketsTab();
        }

        public void ShowRoulette()
        {
            TabHost.Content = new RouletteTab();
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

        private void Root_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape || TabHost.Content is HomeTab || SettingsContextMenu.IsOpen)
                return;

            // Si el desplegable de un ComboBox está abierto, Escape solo lo cierra
            DependencyObject? current = e.OriginalSource as DependencyObject;
            while (current != null)
            {
                if (current is ComboBox combo && combo.IsDropDownOpen)
                    return;
                current = VisualTreeHelper.GetParent(current);
            }

            if (TabHost.Content is TicketsTab tickets && tickets.TryCloseCelebration())
            {
                e.Handled = true;
                return;
            }

            ShowHome();
            e.Handled = true;
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

        private void Sound_Click(object sender, RoutedEventArgs e)
        {
            var sounds = App.AppHost?.Services.GetService<ClauOS.Services.ISoundService>();
            if (sounds == null)
                return;

            sounds.Enabled = !sounds.Enabled;
            UpdateSoundMenu(sounds);
            if (sounds.Enabled)
                sounds.Play(ClauOS.Services.SoundKind.Pop);
        }

        private void UpdateSoundMenu(ClauOS.Services.ISoundService? sounds)
        {
            if (sounds == null)
                return;

            SoundMenuItem.IsChecked = sounds.Enabled;
            SoundMenuItem.Header = sounds.Enabled ? "🔊 Sonido" : "🔇 Sonido";
        }
    }
}