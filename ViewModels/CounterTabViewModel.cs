using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClauOS.ViewModels
{
    public record ComparisonItem(string Emoji, string Value, string Label);

    public partial class CounterTabViewModel : BaseViewModel
    {
        public event Action? ShakeRequested;
        public event Action? ShakeStopRequested;
        private static readonly DateTime StartDate = new(2020, 6, 6, 0, 0, 0);
        private readonly DispatcherTimer _timer;

        [ObservableProperty]
        private string _days = "0";

        [ObservableProperty]
        private string _hours = "00";

        [ObservableProperty]
        private string _minutes = "00";

        [ObservableProperty]
        private string _totalElapsed = "Calculando...";

        [ObservableProperty]
        private bool _isLoaded;

        [ObservableProperty]
        private bool _isComparisonsVisible;

        public ObservableCollection<ComparisonItem> AllComparisons { get; } = new();
        public ObservableCollection<ComparisonItem> VisibleComparisons { get; } = new();

        private int _nextComparisonIndex;

        public CounterTabViewModel()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
            _timer.Tick += (_, _) => Update();
            _timer.Start();
            Update();
        }

        public void OnLoaded()
        {
            if (IsLoaded)
                return;

            IsLoaded = true;
            _ = AnimateEntryAsync();
        }

        [RelayCommand]
        private void ToggleComparisons()
        {
            if (_nextComparisonIndex < AllComparisons.Count)
            {
                VisibleComparisons.Add(AllComparisons[_nextComparisonIndex]);
                _nextComparisonIndex++;
                IsComparisonsVisible = true;

                if (_nextComparisonIndex < AllComparisons.Count)
                    ShakeRequested?.Invoke();
                else
                    ShakeStopRequested?.Invoke();
            }
            else
            {
                VisibleComparisons.Clear();
                _nextComparisonIndex = 0;
                IsComparisonsVisible = false;
                ShakeStopRequested?.Invoke();
            }
        }

        private void Update()
        {
            var elapsed = DateTime.Now - StartDate;
            Days = elapsed.Days.ToString("N0");
            Hours = elapsed.Hours.ToString("D2");
            Minutes = elapsed.Minutes.ToString("D2");
            TotalElapsed = FormatTotalElapsed(elapsed);
            RefreshComparisons(elapsed);
        }

        private void RefreshComparisons(TimeSpan elapsed)
        {
            var heartbeats = (long)(elapsed.TotalMinutes * 70);
            var worldTrips = elapsed.TotalHours / 264.0;
            var hpTimes = Math.Floor(elapsed.TotalMinutes / 4920.0);
            var loverPlays = Math.Floor(elapsed.TotalMinutes / 3.7);
            var nookMortgages = Math.Floor(elapsed.TotalMinutes * 1500.0 / 5696000.0);

            AllComparisons.Clear();
            AllComparisons.Add(new ComparisonItem("💓", heartbeats.ToString("N0"), "LATIDOS DE CORAZÓN"));
            AllComparisons.Add(new ComparisonItem("🌍", worldTrips.ToString("N1"), "VUELTAS AL MUNDO"));
            AllComparisons.Add(new ComparisonItem("📚", hpTimes.ToString("N0"), "LEER SAGA COMPLETA DE HARRY POTTER"));
            AllComparisons.Add(new ComparisonItem("🎵", loverPlays.ToString("N0"), "REPRODUCCIONES DE LOVER"));
            AllComparisons.Add(new ComparisonItem("🦝", nookMortgages.ToString("N0"), "HIPOTECAS PAGADAS A TOM NOOK"));

            var visibleCount = Math.Min(VisibleComparisons.Count, AllComparisons.Count);
            while (VisibleComparisons.Count > visibleCount)
                VisibleComparisons.RemoveAt(VisibleComparisons.Count - 1);
            for (var i = 0; i < visibleCount; i++)
            {
                if (!VisibleComparisons[i].Equals(AllComparisons[i]))
                    VisibleComparisons[i] = AllComparisons[i];
            }
        }

        private string FormatTotalElapsed(TimeSpan ts)
        {
            int years = ts.Days / 365;
            int remainingDays = ts.Days % 365;
            int months = remainingDays / 30;
            int days = remainingDays % 30;

            var parts = new System.Collections.Generic.List<string>();
            if (years > 0) parts.Add($"{years} año{(years != 1 ? "s" : "")}");
            if (months > 0) parts.Add($"{months} mes{(months != 1 ? "es" : "")}");
            if (days > 0) parts.Add($"{days} día{(days != 1 ? "s" : "")}");
            if (ts.Hours > 0) parts.Add($"{ts.Hours} hora{(ts.Hours != 1 ? "s" : "")}");
            if (ts.Minutes > 0) parts.Add($"{ts.Minutes} minuto{(ts.Minutes != 1 ? "s" : "")}");

            return parts.Count > 0 ? string.Join(", ", parts) : "Menos de un minuto";
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private async Task AnimateEntryAsync()
        {
            if (!int.TryParse(Days, NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var targetDays)
                || !int.TryParse(Hours, out var targetHours)
                || !int.TryParse(Minutes, out var targetMinutes))
                return;

            const int steps = 20;
            const int delayMs = 40;

            for (int i = 1; i <= steps; i++)
            {
                double progress = EaseOutQuadratic(i / (double)steps);

                Days = ((int)(targetDays * progress)).ToString("N0");
                Hours = ((int)(targetHours * progress)).ToString("D2");
                Minutes = ((int)(targetMinutes * progress)).ToString("D2");

                await Task.Delay(delayMs);
            }

            Days = targetDays.ToString("N0");
            Hours = targetHours.ToString("D2");
            Minutes = targetMinutes.ToString("D2");
        }

        private static double EaseOutQuadratic(double t) => 1 - (1 - t) * (1 - t);
    }
}