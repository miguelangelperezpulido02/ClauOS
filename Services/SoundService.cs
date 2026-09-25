using System.IO;
using System.Windows.Media;

namespace ClauOS.Services
{
    public enum SoundKind
    {
        Tick,
        Pop,
        Success,
        Fanfare
    }

    public interface ISoundService
    {
        bool Enabled { get; set; }
        void Play(SoundKind kind);
    }

    public sealed class SoundService : ISoundService
    {
        private readonly ISettingsService _settings;
        private readonly Dictionary<SoundKind, Uri> _uris;
        private bool _enabled;

        public SoundService(ISettingsService settings)
        {
            _settings = settings;

            var baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds");
            _uris = new Dictionary<SoundKind, Uri>
            {
                [SoundKind.Tick] = new Uri(Path.Combine(baseDir, "tick.wav")),
                [SoundKind.Pop] = new Uri(Path.Combine(baseDir, "pop.wav")),
                [SoundKind.Success] = new Uri(Path.Combine(baseDir, "success.wav")),
                [SoundKind.Fanfare] = new Uri(Path.Combine(baseDir, "fanfare.wav"))
            };

            _enabled = _settings.GetSetting("SoundsEnabled", true);
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                _settings.SetSetting("SoundsEnabled", value);
                _settings.Save();
            }
        }

        public void Play(SoundKind kind)
        {
            if (!_enabled)
                return;

            try
            {
                var player = new MediaPlayer { Volume = 0.4 };
                player.MediaEnded += (_, _) => player.Close();
                player.MediaFailed += (_, _) => player.Close();
                player.Open(_uris[kind]);
                player.Play();
            }
            catch (Exception)
            {
                // El sonido nunca debe romper la app
            }
        }
    }
}
