using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClauOS.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClauOS.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            DataContext = App.AppHost!.Services.GetRequiredService<LoginViewModel>();
            if (DataContext is LoginViewModel vm)
                vm.PropertyChanged += Vm_PropertyChanged;
        }

        // El PasswordBox no admite binding: refleja aquí los cambios
        // que el ViewModel haga sobre Password (p. ej. limpiarla tras un error).
        private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LoginViewModel.Password)
                && sender is LoginViewModel vm
                && PasswordBox.Password != vm.Password)
            {
                PasswordBox.Password = vm.Password;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm && sender is PasswordBox pb)
            {
                vm.Password = pb.Password;
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is LoginViewModel vm)
            {
                if (vm.LoginCommand.CanExecute(null))
                {
                    vm.LoginCommand.Execute(null);
                }
            }
        }
    }
}