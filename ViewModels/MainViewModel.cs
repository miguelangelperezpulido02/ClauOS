using CommunityToolkit.Mvvm.ComponentModel;

namespace ClauOS.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string _welcomeMessage = "¡Bienvenida a ClauOS, Claudia! 💕";

        [ObservableProperty]
        private int _selectedTabIndex = 0;

        public MainViewModel() { }
    }
}