using System.Collections.ObjectModel;
using ClauOS.Models;
using ClauOS.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClauOS.ViewModels
{
    public partial class RouletteTabViewModel : BaseViewModel
    {
        public const int MaxSongs = 12;

        private static readonly SongItem[] SeedSongs =
        {
            new() { Title = "Cruel Summer", Artist = "Lover", Emoji = "💗" },
            new() { Title = "Blank Space", Artist = "1989", Emoji = "🕶️" },
            new() { Title = "Shake It Off", Artist = "1989", Emoji = "🕶️" },
            new() { Title = "Lover", Artist = "Lover", Emoji = "💗" },
            new() { Title = "Don't Blame Me", Artist = "reputation", Emoji = "🐍" },
            new() { Title = "Love Story", Artist = "Fearless", Emoji = "✨" },
            new() { Title = "Anti-Hero", Artist = "Midnights", Emoji = "🌙" },
            new() { Title = "Style", Artist = "1989", Emoji = "🕶️" },
            new() { Title = "cardigan", Artist = "folklore", Emoji = "🍂" },
            new() { Title = "Delicate", Artist = "reputation", Emoji = "🐍" },
            new() { Title = "August", Artist = "folklore", Emoji = "🍂" },
            new() { Title = "You Belong With Me", Artist = "Fearless", Emoji = "✨" }
        };

        private static readonly string[] FunPhrases =
        {
            "¡A subir el volumen! 🔊",
            "El azar tiene buen gusto ✨",
            "Temazo elegido 🎶",
            "¡A cantar a pleno pulmón! 🎤",
            "La ruleta nunca falla 💫"
        };

        private readonly ISettingsService _settings;
        private readonly Random _random = new();
        private int _lastWinnerIndex = -1;
        private int _lastPhraseIndex = -1;

        public ObservableCollection<SongItem> Songs { get; } = new();

        [ObservableProperty]
        private string _inputTitle = string.Empty;

        [ObservableProperty]
        private string _inputAlbum = string.Empty;

        [ObservableProperty]
        private string _notice = string.Empty;

        [ObservableProperty]
        private bool _hasResult;

        [ObservableProperty]
        private bool _isSpinning;

        [ObservableProperty]
        private string _winnerEmoji = "🎵";

        [ObservableProperty]
        private string _winnerTitle = "¿Qué escuchamos hoy?";

        [ObservableProperty]
        private string _winnerArtist = string.Empty;

        [ObservableProperty]
        private string _funPhrase = "Pulsa GIRAR y deja que el azar decida";

        public RouletteTabViewModel(ISettingsService settings)
        {
            _settings = settings;
            LoadSongs();
        }

        private void LoadSongs()
        {
            var saved = _settings.GetSetting<List<SongItem>>("Songs", null);
            if (saved is { Count: > 0 })
            {
                foreach (var s in saved.Take(MaxSongs))
                    Songs.Add(s);
            }
            else
            {
                foreach (var s in SeedSongs)
                    Songs.Add(new SongItem { Title = s.Title, Artist = s.Artist, Emoji = s.Emoji });
                Persist();
            }
        }

        private void Persist()
        {
            _settings.SetSetting("Songs", Songs.ToList());
            _settings.Save();
        }

        public SongItem? PickWinner()
        {
            if (Songs.Count == 0)
                return null;

            int index;
            do
            {
                index = _random.Next(Songs.Count);
            } while (index == _lastWinnerIndex && Songs.Count > 1);
            _lastWinnerIndex = index;
            return Songs[index];
        }

        public void ShowWinner(SongItem song)
        {
            WinnerEmoji = song.Emoji;
            WinnerTitle = song.Title;
            WinnerArtist = song.Artist;
            FunPhrase = NextPhrase();
            HasResult = true;
            IsSpinning = false;
        }

        private string NextPhrase()
        {
            int index;
            do
            {
                index = _random.Next(FunPhrases.Length);
            } while (index == _lastPhraseIndex && FunPhrases.Length > 1);
            _lastPhraseIndex = index;
            return FunPhrases[index];
        }

        [RelayCommand]
        private void AddSong()
        {
            var title = InputTitle.Trim();
            if (string.IsNullOrEmpty(title))
                return;

            if (Songs.Count >= MaxSongs)
            {
                Notice = "La ruleta solo admite 12 canciones 🥁";
                return;
            }

            Songs.Add(new SongItem
            {
                Title = title,
                Artist = string.IsNullOrWhiteSpace(InputAlbum) ? "Taylor Swift" : InputAlbum.Trim(),
                Emoji = "🎵"
            });
            InputTitle = string.Empty;
            InputAlbum = string.Empty;
            Notice = string.Empty;
            Persist();
        }

        [RelayCommand]
        private void DeleteSong(SongItem? song)
        {
            if (song == null)
                return;

            if (Songs.IndexOf(song) == _lastWinnerIndex)
                _lastWinnerIndex = -1;

            Songs.Remove(song);
            Persist();
        }
    }
}
