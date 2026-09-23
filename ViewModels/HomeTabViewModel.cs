using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClauOS.ViewModels
{
    public partial class HomeTabViewModel : BaseViewModel
    {
        private readonly DispatcherTimer _timer;
        private readonly Random _random = new();
        private int _currentIndex = -1;

        [ObservableProperty]
        private string _dailyQuote = "Cargando frase...";

        private readonly string[] _quotes = new[]
        {
            "'Cada día a tu lado es un regalo que atesoro en el corazón'",
            "'El amor no se mira, se siente, y aún más cuando estás lejos'",
            "'Eres mi hoy y todos mis mañanas'",
            "'No hay distancia que pueda separar dos corazones que se aman'",
            "'Contigo, hasta los días grises tienen color de rosa'",
            "'Te quiero no solo por como eres, sino por como soy yo cuando estoy contigo'",
            "'El mejor lugar del mundo es junto a ti'",
            "'Un beso tuyo vale más que mil palabras'",
            "'Eres la razón por la que sonrío sin motivo'",
            "'Gracias por existir y hacer mi vida más bonita'",
            "'Te echaba de menos'"
        };

        public HomeTabViewModel()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _timer.Tick += (_, _) => ChangeQuote();
            _timer.Start();
            ChangeQuote();
        }

        private void ChangeQuote()
        {
            int newIndex;
            do
            {
                newIndex = _random.Next(_quotes.Length);
            } while (newIndex == _currentIndex && _quotes.Length > 1);

            _currentIndex = newIndex;
            DailyQuote = _quotes[_currentIndex];
        }
    }
}