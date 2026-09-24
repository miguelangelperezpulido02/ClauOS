using System.Windows;
using System.Windows.Controls;
using ClauOS.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClauOS.Views
{
    public partial class FoodTab : UserControl
    {
        public FoodTab()
        {
            InitializeComponent();
            DataContext = App.AppHost!.Services.GetRequiredService<FoodTabViewModel>();
            Unloaded += (_, _) => (DataContext as FoodTabViewModel)?.Stop();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainView.FindMain(this)?.ShowHome();
        }

        private void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) => e.Handled = true;
    }
}
