using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using ClauOS.Views;

namespace ClauOS.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private UserControl? _currentView;

        public UserControl? CurrentView => _currentView;

        public event Action? CurrentViewChanged;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void NavigateToLogin()
        {
            var view = _serviceProvider.GetRequiredService<LoginView>();
            SetCurrentView(view);
        }

        public void NavigateToSplash()
        {
            var view = _serviceProvider.GetRequiredService<SplashView>();
            SetCurrentView(view);
        }

        public void NavigateToMain()
        {
            var view = _serviceProvider.GetRequiredService<MainView>();
            SetCurrentView(view);
        }

        private void SetCurrentView(UserControl view)
        {
            _currentView = view;
            CurrentViewChanged?.Invoke();
        }
    }
}