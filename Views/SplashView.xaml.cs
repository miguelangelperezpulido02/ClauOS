using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ClauOS.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClauOS.Views
{
    public partial class SplashView : UserControl
    {
        public SplashView()
        {
            InitializeComponent();
            DataContext = App.AppHost!.Services.GetRequiredService<SplashViewModel>();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            
            if (DataContext is SplashViewModel vm)
            {
                vm.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(vm.Progress))
                    {
                        AnimateProgress(vm.Progress);
                    }
                    else if (args.PropertyName == nameof(vm.StatusMessage) && vm.StatusMessage == "¡Bienvenida, Claudia! 💖")
                    {
                        ShowWelcomeOverlay();
                    }
                };
            }
        }

        private void AnimateProgress(double progress)
        {
            var anim = new DoubleAnimation
            {
                To = progress / 100.0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            ProgressScale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
        }

        private void ShowWelcomeOverlay()
        {
            WelcomeOverlay.Visibility = Visibility.Visible;
            
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            WelcomeOverlay.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            var scaleAnim = new DoubleAnimation(0.9, 1, TimeSpan.FromMilliseconds(200)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            var scaleTransform = new ScaleTransform(0.9, 0.9);
            WelcomeOverlay.RenderTransform = scaleTransform;
            WelcomeOverlay.RenderTransformOrigin = new Point(0.5, 0.5);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }
    }
}