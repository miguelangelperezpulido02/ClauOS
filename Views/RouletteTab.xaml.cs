using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ClauOS.Models;
using ClauOS.Services;
using ClauOS.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClauOS.Views
{
    public partial class RouletteTab : UserControl
    {
        private const double WheelSize = 320.0;
        private const double Radius = 150.0;
        private const double SpinSeconds = 4.0;
        private const int FullTurns = 5;

        private static readonly Color[] SliceColors =
        {
            Color.FromRgb(0xEC, 0x40, 0x7A),
            Color.FromRgb(0xAB, 0x47, 0xBC),
            Color.FromRgb(0xFF, 0xCA, 0x28),
            Color.FromRgb(0x7E, 0x57, 0xC2)
        };

        private static readonly Color DarkLabel = Color.FromRgb(0x3D, 0x2C, 0x3F);

        private static readonly Dictionary<string, (Color Slice, Color Text)> EraColors = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Lover"] = (Color.FromRgb(0xF7, 0xA8, 0xC4), DarkLabel),
            ["1989"] = (Color.FromRgb(0xA8, 0xD8, 0xEA), DarkLabel),
            ["reputation"] = (Color.FromRgb(0x23, 0x23, 0x23), Colors.White),
            ["Fearless"] = (Color.FromRgb(0xD4, 0xAF, 0x37), DarkLabel),
            ["Midnights"] = (Color.FromRgb(0x2E, 0x1A, 0x47), Colors.White),
            ["folklore"] = (Color.FromRgb(0xC9, 0xC5, 0xB8), DarkLabel)
        };

        private static (Color Slice, Color Text) ColorsFor(SongItem song, int index) =>
            EraColors.TryGetValue(song.Artist ?? string.Empty, out var c)
                ? c
                : (SliceColors[index % SliceColors.Length], Colors.White);

        private static readonly SolidColorBrush WinnerBrush =
            new(Color.FromRgb(0xFF, 0xEB, 0x3B));

        private static readonly SolidColorBrush SliceStroke =
            new(Color.FromArgb(0xBF, 0xFF, 0xFF, 0xFF));

        private readonly List<Shape> _slicePaths = new();
        private readonly ISoundService _sounds;

        private CancellationTokenSource? _tickCts;

        private RouletteTabViewModel ViewModel => (RouletteTabViewModel)DataContext;

        private double _currentAngle;
        private double _spinTo;
        private SongItem? _pendingWinner;
        private int _pendingIndex = -1;

        public RouletteTab()
        {
            InitializeComponent();
            DataContext = App.AppHost!.Services.GetRequiredService<RouletteTabViewModel>();
            _sounds = App.AppHost!.Services.GetRequiredService<ISoundService>();

            ViewModel.Songs.CollectionChanged += (_, _) =>
            {
                _tickCts?.Cancel();
                WheelRotation.BeginAnimation(RotateTransform.AngleProperty, null);
                ViewModel.IsSpinning = false;
                BuildWheel();
            };
            BuildWheel();

            Unloaded += (_, _) =>
            {
                _tickCts?.Cancel();
                WheelRotation.BeginAnimation(RotateTransform.AngleProperty, null);
            };
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainView.FindMain(this)?.ShowHome();
        }

        private void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) => e.Handled = true;

        private static Point Polar(double angleDegClockwiseFromTop, double r)
        {
            var rad = (angleDegClockwiseFromTop - 90.0) * Math.PI / 180.0;
            return new Point(160.0 + r * Math.Cos(rad), 160.0 + r * Math.Sin(rad));
        }

        private void BuildWheel()
        {
            WheelCanvas.Children.Clear();
            _slicePaths.Clear();
            _currentAngle = 0.0;
            WheelRotation.Angle = 0.0;

            var songs = ViewModel.Songs;
            if (songs.Count == 0)
            {
                var placeholder = new TextBlock
                {
                    Text = "Añade canciones 🎵",
                    FontSize = 16,
                    Width = 200,
                    TextAlignment = TextAlignment.Center
                };
                Canvas.SetLeft(placeholder, 60.0);
                Canvas.SetTop(placeholder, 150.0);
                WheelCanvas.Children.Add(placeholder);
                return;
            }

            double center = WheelSize / 2.0;
            double per = 360.0 / songs.Count;

            if (songs.Count == 1)
            {
                var circle = new Ellipse
                {
                    Width = Radius * 2.0,
                    Height = Radius * 2.0,
                    Fill = new SolidColorBrush(ColorsFor(songs[0], 0).Slice),
                    Stroke = SliceStroke,
                    StrokeThickness = 1.0
                };
                Canvas.SetLeft(circle, center - Radius);
                Canvas.SetTop(circle, center - Radius);
                WheelCanvas.Children.Add(circle);
                _slicePaths.Add(circle);
                AddLabel(songs[0].Title, 0.0, ColorsFor(songs[0], 0).Text);
                return;
            }

            for (int i = 0; i < songs.Count; i++)
            {
                double a0 = i * per;
                double a1 = (i + 1) * per;
                var p0 = Polar(a0, Radius);
                var p1 = Polar(a1, Radius);

                var figure = new PathFigure
                {
                    StartPoint = new Point(center, center),
                    IsClosed = true
                };
                figure.Segments.Add(new LineSegment(p0, true));
                figure.Segments.Add(new ArcSegment(p1, new Size(Radius, Radius), 0.0,
                    per > 180.0, SweepDirection.Clockwise, true));
                var geometry = new PathGeometry();
                geometry.Figures.Add(figure);

                var slice = new Path
                {
                    Data = geometry,
                    Fill = new SolidColorBrush(ColorsFor(songs[i], i).Slice),
                    Stroke = SliceStroke,
                    StrokeThickness = 1.0
                };
                WheelCanvas.Children.Add(slice);
                _slicePaths.Add(slice);

                AddLabel(songs[i].Title, a0 + per / 2.0, ColorsFor(songs[i], i).Text);
            }
        }

        private void AddLabel(string text, double midAngle, Color textColor)
        {
            var label = new TextBlock
            {
                Text = text,
                FontSize = 11.0,
                Width = 84.0,
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Foreground = new SolidColorBrush(textColor),
                FontWeight = FontWeights.SemiBold,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(midAngle > 90.0 && midAngle < 270.0
                    ? midAngle + 180.0
                    : midAngle)
            };
            var pos = Polar(midAngle, Radius * 0.64);
            Canvas.SetLeft(label, pos.X - 42.0);
            Canvas.SetTop(label, pos.Y - 9.0);
            WheelCanvas.Children.Add(label);
        }

        private void SpinButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsSpinning || ViewModel.Songs.Count == 0)
                return;

            var winner = ViewModel.PickWinner();
            if (winner is null)
                return;

            _pendingWinner = winner;
            _pendingIndex = ViewModel.Songs.IndexOf(winner);
            ViewModel.IsSpinning = true;
            ResetHighlight();

            double per = 360.0 / ViewModel.Songs.Count;
            double winnerCenter = _pendingIndex * per + per / 2.0;
            double target = (360.0 - winnerCenter) % 360.0;
            double current = ((_currentAngle % 360.0) + 360.0) % 360.0;
            double delta = (target - current + 360.0) % 360.0;

            _spinTo = _currentAngle + 360.0 * FullTurns + delta;

            var animation = new DoubleAnimation(_currentAngle, _spinTo,
                new Duration(TimeSpan.FromSeconds(SpinSeconds)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            animation.Completed += OnSpinCompleted;
            WheelRotation.BeginAnimation(RotateTransform.AngleProperty, animation);

            _tickCts?.Cancel();
            _tickCts = new CancellationTokenSource();
            _ = PlayTicksAsync(SpinSeconds, _tickCts.Token);
        }

        // Ticks a intervalos de ángulo iguales: con el ease-out cúbico
        // suenan rápidos al principio y se espacian al frenar.
        private async Task PlayTicksAsync(double seconds, CancellationToken token)
        {
            const int ticks = 26;
            double previous = 0.0;
            for (int i = 1; i <= ticks; i++)
            {
                double at = seconds * (1.0 - Math.Cbrt(1.0 - (double)i / ticks));
                int delay = Math.Max(0, (int)((at - previous) * 1000.0));
                previous = at;
                try
                {
                    await Task.Delay(delay, token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                _sounds.Play(SoundKind.Tick);
            }
        }

        private void OnSpinCompleted(object? sender, EventArgs e)
        {
            _currentAngle = _spinTo;
            WheelRotation.BeginAnimation(RotateTransform.AngleProperty, null);
            WheelRotation.Angle = _spinTo;

            if (_pendingWinner != null && _pendingIndex >= 0 && _pendingIndex < _slicePaths.Count)
            {
                _slicePaths[_pendingIndex].Fill = WinnerBrush;
                ViewModel.ShowWinner(_pendingWinner);
            }
            else
            {
                ViewModel.IsSpinning = false;
            }
        }

        private void ResetHighlight()
        {
            var songs = ViewModel.Songs;
            for (int i = 0; i < _slicePaths.Count && i < songs.Count; i++)
                _slicePaths[i].Fill = new SolidColorBrush(ColorsFor(songs[i], i).Slice);
        }
    }
}
