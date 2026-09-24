using System.Collections.ObjectModel;
using ClauOS.Models;
using ClauOS.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClauOS.ViewModels
{
    public partial class TicketGroup : ObservableObject
    {
        public string Title { get; set; } = string.Empty;
        public string Emoji { get; set; } = "🎫";
        public string Description { get; set; } = string.Empty;
        public List<TicketItem> Items { get; } = new();

        [ObservableProperty]
        private int _available;
    }

    public partial class TicketsTabViewModel : BaseViewModel
    {
        private static readonly (string Title, string Emoji, string Description, int Copies)[] SeedVarieties =
        {
            ("Vale por un abrazo", "🤗", "Un abrazo fuerte, donde y cuando quieras", 5),
            ("Te invito a comer", "🍽️", "El restaurante lo eliges tú, yo invito", 3),
            ("¿Qué vemos?", "🎬", "Noche de peli: tú eliges, yo pongo las palomitas", 3),
            ("Masaje", "💆", "Masaje relajante de 15 minutos, sin prisas", 3),
            ("Perdón instantáneo", "💌", "Un perdón inmediato, sin preguntas ni reproches", 2),
            ("Mimos sin límites", "🥰", "Tarde entera de mimos, sin límite de dosis", 5),
            ("Paseo juntos", "🌳", "Un paseo sin móvil, solo nosotros dos", 3),
            ("Desayuno en la cama", "🥐", "Desayuno servido en la cama un fin de semana", 2),
        };

        private readonly ISettingsService _settings;

        [ObservableProperty]
        private TicketGroup? _pendingGroup;

        [ObservableProperty]
        private bool _isConfirmVisible;

        public ObservableCollection<TicketGroup> Groups { get; } = new();

        [ObservableProperty]
        private int _availableCount;

        [ObservableProperty]
        private int _redeemedCount;

        [ObservableProperty]
        private string _summary = string.Empty;

        [ObservableProperty]
        private TicketItem? _celebratedTicket;

        [ObservableProperty]
        private bool _isCelebrationVisible;

        public TicketsTabViewModel(ISettingsService settings)
        {
            _settings = settings;
            Load();
        }

        public bool IsDevResetVisible =>
#if DEBUG
            true;
#else
            false;
#endif

        private void Load()
        {
            var saved = _settings.GetSetting<List<TicketItem>>("Tickets", null);
            var items = saved is { Count: > 0 } ? saved : BuildSeed();
            if (saved is not { Count: > 0 })
                Persist(items);

            BuildGroups(items);
            RefreshCounts();
        }

        [RelayCommand]
        private void DevResetTickets()
        {
            BuildGroups(BuildSeed());
            CelebratedTicket = null;
            IsCelebrationVisible = false;
            PendingGroup = null;
            IsConfirmVisible = false;
            RefreshCounts();
            Persist();
        }

        private void BuildGroups(List<TicketItem> items)
        {
            Groups.Clear();
            foreach (var item in items)
            {
                var group = Groups.FirstOrDefault(g => g.Title == item.Title);
                if (group == null)
                {
                    group = new TicketGroup
                    {
                        Title = item.Title,
                        Emoji = item.Emoji,
                        Description = item.Description
                    };
                    Groups.Add(group);
                }
                group.Items.Add(item);
            }
        }

        private static List<TicketItem> BuildSeed()
        {
            var items = new List<TicketItem>();
            foreach (var (title, emoji, description, copies) in SeedVarieties)
            {
                for (var i = 0; i < copies; i++)
                {
                    items.Add(new TicketItem
                    {
                        Title = title,
                        Emoji = emoji,
                        Description = description
                    });
                }
            }
            return items;
        }

        private void Persist(List<TicketItem>? items = null)
        {
            items ??= Groups.SelectMany(g => g.Items).ToList();
            _settings.SetSetting("Tickets", items);
            _settings.Save();
        }

        private void RefreshCounts()
        {
            var available = 0;
            var redeemed = 0;
            foreach (var group in Groups)
            {
                group.Available = group.Items.Count(i => !i.IsRedeemed);
                available += group.Available;
                redeemed += group.Items.Count - group.Available;
            }
            AvailableCount = available;
            RedeemedCount = redeemed;
            Summary = $"Te quedan {available} tickets · {redeemed} canjeados";
        }

        [RelayCommand]
        private void RequestRedeem(TicketGroup? group)
        {
            if (group == null || group.Available <= 0)
                return;

            PendingGroup = group;
            IsConfirmVisible = true;
        }

        [RelayCommand]
        private void ConfirmPendingRedeem()
        {
            if (PendingGroup == null)
                return;

            var group = PendingGroup;
            PendingGroup = null;
            IsConfirmVisible = false;
            ConfirmRedeem(group);
        }

        [RelayCommand]
        private void CancelRedeem()
        {
            PendingGroup = null;
            IsConfirmVisible = false;
        }

        public void ConfirmRedeem(TicketGroup group)
        {
            var item = group.Items.FirstOrDefault(i => !i.IsRedeemed);
            if (item == null)
                return;

            item.IsRedeemed = true;
            item.RedeemedAt = DateTime.Now;
            RefreshCounts();
            Persist();
            CelebratedTicket = item;
            IsCelebrationVisible = true;
        }

        [RelayCommand]
        private void CloseCelebration()
        {
            IsCelebrationVisible = false;
            CelebratedTicket = null;
        }
    }
}
