using System.Windows;
using System.Windows.Controls;
using ClauOS.Services;
using ClauOS.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClauOS
{
    public partial class MainWindow : Window
    {
        private readonly INavigationService _navigation;

        public MainWindow()
        {
            InitializeComponent();
            
            _navigation = App.AppHost!.Services.GetRequiredService<INavigationService>();
            _navigation.CurrentViewChanged += OnViewChanged;
            
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            
            _navigation.NavigateToLogin();
        }

        private void OnViewChanged()
        {
            Dispatcher.Invoke(() =>
            {
                ViewContainer.Content = _navigation.CurrentView;
            });
        }
    }
}