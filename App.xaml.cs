using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ClauOS.Services;
using ClauOS.ViewModels;
using ClauOS.Views;

namespace ClauOS
{
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            try
            {
                AppHost = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        services.AddSingleton<INavigationService, NavigationService>();
                        services.AddSingleton<ISettingsService, JsonSettingsService>();
                        services.AddTransient<LoginViewModel>();
                        services.AddTransient<SplashViewModel>();
                        services.AddTransient<MainViewModel>();
                        services.AddTransient<HomeTabViewModel>();
                        services.AddTransient<FoodTabViewModel>();
                        services.AddTransient<CounterTabViewModel>();
                        services.AddTransient<TicketsTabViewModel>();
                        services.AddTransient<RouletteTabViewModel>();
                        services.AddTransient<LoginView>();
                        services.AddTransient<SplashView>();
                        services.AddTransient<MainView>();
                        services.AddTransient<FoodTab>();
                        services.AddTransient<CounterTab>();
                        services.AddTransient<TicketsTab>();
                        services.AddTransient<RouletteTab>();
                        services.AddTransient<MainWindow>();
                    })
                    .Build();

                InitializeDefaultSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error crítico al iniciar la aplicación:\n{ex.Message}", "ClauOS - Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void InitializeDefaultSettings()
        {
            if (AppHost == null) return;
            
            var settings = AppHost.Services.GetRequiredService<ISettingsService>();
            if (string.IsNullOrEmpty(settings.GetSetting("ValidUsername", "")))
            {
                settings.SetSetting("ValidUsername", "Claudia");
                settings.SetSetting("ValidPassword", "claudia");
                settings.Save();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (AppHost == null)
            {
                MessageBox.Show("No se pudo inicializar la aplicación.", "ClauOS - Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            // Cargar tema guardado
            var settings = AppHost.Services.GetRequiredService<ISettingsService>();
            var savedTheme = settings.GetSetting("Theme", "Default");
            ApplyTheme(savedTheme);

            AppHost.Start();

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            System.Diagnostics.Debug.WriteLine("MainWindow created, showing...");
            mainWindow.Show();
            System.Diagnostics.Debug.WriteLine("MainWindow shown");
        }

        public static void ApplyTheme(string themeName)
        {
            try
            {
                var dict = new ResourceDictionary
                {
                    Source = new Uri($"pack://application:,,,/Resources/Themes/Theme{themeName}.xaml", UriKind.Absolute)
                };
                // Reemplazar el diccionario de colores (índice 0) con el tema
                if (Application.Current.Resources.MergedDictionaries.Count > 0)
                {
                    Application.Current.Resources.MergedDictionaries[0] = dict;
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries.Insert(0, dict);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error aplicando tema {themeName}: {ex.Message}");
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (AppHost != null)
                await AppHost.StopAsync();
            base.OnExit(e);
        }
    }
}