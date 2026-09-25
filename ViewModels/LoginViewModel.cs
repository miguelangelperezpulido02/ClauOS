using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClauOS.Services;

namespace ClauOS.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly INavigationService _navigation;
        private readonly ISettingsService _settings;
        private readonly ISoundService _sounds;

        [ObservableProperty]
        private string _username = "Claudia";

        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private bool _hasError;

        public bool IsDevSkipVisible
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public LoginViewModel(INavigationService navigation, ISettingsService settings, ISoundService sounds)
        {
            _navigation = navigation;
            _settings = settings;
            _sounds = sounds;
            
            var savedUser = _settings.GetSetting("LastUser", "");
            if (!string.IsNullOrEmpty(savedUser))
                Username = savedUser;
        }

        [RelayCommand]
        private async Task Login()
        {
            ErrorMessage = null;
            HasError = false;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Por favor, introduce tanto el usuario como la contraseña 💕";
                HasError = true;
                return;
            }

            var validUser = _settings.GetSetting("ValidUsername", "Claudia");
            var validPass = _settings.GetSetting("ValidPassword", "claudia");

            if (Username == validUser && Password == validPass)
            {
                _settings.SetSetting("LastUser", Username);
                _settings.Save();
                _sounds.Play(SoundKind.Success);

                IsBusy = true;
                BusyMessage = "Verificando credenciales...";

                try
                {
                    await Task.Delay(500);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _navigation.NavigateToSplash();
                    });
                }
                catch (Exception)
                {
                    ErrorMessage = "Error al iniciar sesión. Inténtalo de nuevo ✨";
                    HasError = true;
                    IsBusy = false;
                }
            }
            else
            {
                ErrorMessage = "Usuario o contraseña incorrectos. Inténtalo de nuevo ✨";
                HasError = true;
                Password = "";
            }
        }

        [RelayCommand]
        private void ClearError()
        {
            HasError = false;
            ErrorMessage = null;
        }

        [RelayCommand]
        private void DevSkip()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _navigation.NavigateToMain();
            });
        }
    }
}