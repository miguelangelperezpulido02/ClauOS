using System.Windows.Controls;
using System.Windows.Input;
using ClauOS.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClauOS.Views
{
    public partial class HomeTab : UserControl
    {
        public HomeTab()
        {
            InitializeComponent();
            DataContext = App.AppHost!.Services.GetRequiredService<HomeTabViewModel>();
        }

        private void FoodCard_Click(object sender, MouseButtonEventArgs e)
        {
            MainView.FindMain(this)?.ShowFood();
        }

        private void CounterCard_Click(object sender, MouseButtonEventArgs e)
        {
            MainView.FindMain(this)?.ShowCounter();
        }
    }
}