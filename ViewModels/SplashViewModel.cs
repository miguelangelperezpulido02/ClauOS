using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using ClauOS.Services;

namespace ClauOS.ViewModels
{
    public partial class SplashViewModel : BaseViewModel
    {
        private readonly INavigationService _navigation;

        [ObservableProperty]
        private double _progress = 0;

        [ObservableProperty]
        private string _statusMessage = "Iniciando ClauOS...";

        [ObservableProperty]
        private string _subMessage = "Cargando amor y paciencia 💕";

        private readonly string[] _loadingMessages = new[]
        {
            "Cargando amor y paciencia 💕",
            "Preparando besitos virtuales 😘",
            "Inflando corazones 🎈",
            "Endulzando el día 🍯",
            "Casi listo... ✨",
            "Te echaba de menos"
        };

        public SplashViewModel(INavigationService navigation)
        {
            _navigation = navigation;
            _ = StartLoadingAsync();
        }

        private async Task StartLoadingAsync()
        {
            try
            {
                IsBusy = true;

                var random = new Random();
                var totalSteps = 100;
                var stepDelay = 20;

                for (int i = 0; i <= totalSteps; i++)
                {
                    Progress = i;

                    if (i % 20 == 0 && i < totalSteps)
                    {
                        StatusMessage = _loadingMessages[random.Next(_loadingMessages.Length)];
                    }

                    await Task.Delay(stepDelay);
                }

                StatusMessage = "¡Bienvenida, Claudia! 💖";
                SubMessage = "Abriendo ClauOS...";
                await Task.Delay(2500);

                App.Current.Dispatcher.Invoke(() =>
                {
                    _navigation.NavigateToMain();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SplashViewModel] Error during loading: {ex.Message}");
            }
        }
    }
}