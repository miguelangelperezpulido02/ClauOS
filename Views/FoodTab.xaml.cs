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
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainView.FindMain(this)?.ShowHome();
        }
    }
}
