using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ClauOS.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClauOS.Views
{
    public partial class CounterTab : UserControl
    {
        private CounterTabViewModel? _vm;
        private Storyboard? _shakeStoryboard;
        private Storyboard? _breatheStoryboard;

        public CounterTab()
        {
            InitializeComponent();
            DataContext = App.AppHost!.Services.GetRequiredService<CounterTabViewModel>();
            Loaded += (_, _) => Subscribe();
            Unloaded += (_, _) => Unsubscribe();
        }

        private void Subscribe()
        {
            if (_vm != null)
                return;

            _vm = DataContext as CounterTabViewModel;
            if (_vm != null)
            {
                _vm.ShakeRequested += StartShakeHintButton;
                _vm.ShakeStopRequested += StopShakeHintButton;
                if (_vm.VisibleComparisons.Count == 0)
                    StartBreathing();
            }
        }

        private void Unsubscribe()
        {
            StopShakeHintButton();
            StopBreathing();
            if (_vm != null)
            {
                _vm.ShakeRequested -= StartShakeHintButton;
                _vm.ShakeStopRequested -= StopShakeHintButton;
            }
            _vm = null;
        }

        private TransformGroup EnsureTransformGroup()
        {
            if (HintButton.RenderTransform is not TransformGroup group)
            {
                group = new TransformGroup();
                group.Children.Add(new ScaleTransform(1, 1));
                group.Children.Add(new TranslateTransform());
                HintButton.RenderTransform = group;
            }
            HintButton.RenderTransformOrigin = new Point(0.5, 0.5);
            return (TransformGroup)HintButton.RenderTransform;
        }

        private void StartBreathing()
        {
            if (_breatheStoryboard != null)
                return;

            EnsureTransformGroup();
            var breathe = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

            var scaleX = new DoubleAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };
            scaleX.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))));
            scaleX.KeyFrames.Add(new LinearDoubleKeyFrame(1.08, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(800))));
            scaleX.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1600))));
            Storyboard.SetTarget(scaleX, HintButton);
            Storyboard.SetTargetProperty(scaleX, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));

            var scaleY = new DoubleAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };
            scaleY.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0))));
            scaleY.KeyFrames.Add(new LinearDoubleKeyFrame(1.08, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(800))));
            scaleY.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1600))));
            Storyboard.SetTarget(scaleY, HintButton);
            Storyboard.SetTargetProperty(scaleY, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));

            breathe.Children.Add(scaleX);
            breathe.Children.Add(scaleY);
            _breatheStoryboard = breathe;
            _breatheStoryboard.Begin();
        }

        private void StopBreathing()
        {
            _breatheStoryboard?.Stop();
            _breatheStoryboard = null;
            if (HintButton?.RenderTransform is TransformGroup group && group.Children.Count > 0 && group.Children[0] is ScaleTransform s)
            {
                s.ScaleX = 1.0;
                s.ScaleY = 1.0;
            }
        }

        private void StartShakeHintButton()
        {
            StopBreathing();
            StopShakeHintButton();

            EnsureTransformGroup();
            var shake = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            shake.KeyFrames.Add(new LinearDoubleKeyFrame(-7, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(90))));
            shake.KeyFrames.Add(new LinearDoubleKeyFrame(7, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(180))));
            shake.KeyFrames.Add(new LinearDoubleKeyFrame(-5, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(270))));
            shake.KeyFrames.Add(new LinearDoubleKeyFrame(5, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(360))));
            shake.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(450))));
            Storyboard.SetTarget(shake, HintButton);
            Storyboard.SetTargetProperty(shake, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.X)"));
            _shakeStoryboard = new Storyboard();
            _shakeStoryboard.Children.Add(shake);
            _shakeStoryboard.Begin();
        }

        private void StopShakeHintButton()
        {
            _shakeStoryboard?.Stop();
            _shakeStoryboard = null;
            if (HintButton?.RenderTransform is TransformGroup group && group.Children.Count > 1 && group.Children[1] is TranslateTransform t)
                t.X = 0;

            if (_vm != null && _vm.VisibleComparisons.Count == 0 && _breatheStoryboard == null)
                StartBreathing();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CounterTabViewModel vm)
            {
                vm.OnLoaded();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainView.FindMain(this) is MainView main)
            {
                main.ShowHome();
            }
        }
    }
}