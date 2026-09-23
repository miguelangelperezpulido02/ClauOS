using System.Windows.Controls;

namespace ClauOS.Services
{
    public interface INavigationService
    {
        void NavigateToLogin();
        void NavigateToSplash();
        void NavigateToMain();
        UserControl? CurrentView { get; }
        event Action? CurrentViewChanged;
    }
}