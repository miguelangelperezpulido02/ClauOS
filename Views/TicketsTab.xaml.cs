using System.Windows;
using System.Windows.Controls;
using ClauOS.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClauOS.Views
{
    public partial class TicketsTab : UserControl
    {
        public TicketsTab()
        {
            InitializeComponent();
            DataContext = App.AppHost!.Services.GetRequiredService<TicketsTabViewModel>();
        }

        public bool TryCloseCelebration()
        {
            if (DataContext is not TicketsTabViewModel vm)
                return false;

            if (vm.IsCelebrationVisible)
            {
                vm.CloseCelebrationCommand.Execute(null);
                return true;
            }

            if (vm.IsConfirmVisible)
            {
                vm.CancelRedeemCommand.Execute(null);
                return true;
            }

            return false;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainView.FindMain(this) is MainView main)
                main.ShowHome();
        }

        private void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) => e.Handled = true;
    }
}
