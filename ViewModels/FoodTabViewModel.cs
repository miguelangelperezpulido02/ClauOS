using System.Collections.ObjectModel;
using System.Windows.Threading;
using ClauOS.Models;
using ClauOS.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClauOS.ViewModels
{
    public partial class FoodTabViewModel : BaseViewModel
    {
        public const string AllCategories = "Todo";

        public static readonly string[] Categories =
        {
            "Desayuno", "Comida", "Cena", "Postre", "Snack"
        };

        public static readonly string[] EmojiChoices =
        {
            "🍕", "🍔", "🥗", "🍣", "🌮", "🍝", "🥘", "🍜", "🍗", "🥑",
            "🍎", "🥐", "☕", "🥞", "🧁", "🍩", "🍰", "🍦", "🍪", "🌯",
            "🥙", "🍛", "🍲", "🥟", "🍱", "🍤", "🥪", "🍳", "🧀", "🍿", "🍽️"
        };

        private static readonly string[] FunPhrases =
        {
            "¡Buena elección! 😋",
            "El azar ha hablado 🍀",
            "¡Ñam ñam! 😍",
            "Nada de arrepentimientos 😎",
            "El destino lo quiso así ✨",
            "¡A disfrutar! 🎉"
        };

        private static readonly FoodItem[] SeedFoods =
        {
            new() { Name = "Tortitas con miel", Emoji = "🥞", Category = "Desayuno" },
            new() { Name = "Tostada con aguacate", Emoji = "🥑", Category = "Desayuno" },
            new() { Name = "Café con croissant", Emoji = "🥐", Category = "Desayuno" },
            new() { Name = "Pizza", Emoji = "🍕", Category = "Comida" },
            new() { Name = "Sushi", Emoji = "🍣", Category = "Comida" },
            new() { Name = "Tacos", Emoji = "🌮", Category = "Cena" },
            new() { Name = "Pasta", Emoji = "🍝", Category = "Comida" },
            new() { Name = "Ensalada", Emoji = "🥗", Category = "Cena" },
            new() { Name = "Pollo asado", Emoji = "🍗", Category = "Cena" },
            new() { Name = "Tarta de queso", Emoji = "🍰", Category = "Postre" },
            new() { Name = "Helado", Emoji = "🍦", Category = "Postre" },
            new() { Name = "Palomitas", Emoji = "🍿", Category = "Snack" }
        };

        private const int SlotTicks = 12;

        private readonly ISettingsService _settings;
        private readonly DispatcherTimer _slotTimer;
        private readonly Random _random = new();
        private readonly List<FoodItem> _foods = new();
        private int _slotTick;
        private List<FoodItem> _pool = new();
        private int _lastPickIndex = -1;
        private int _lastPhraseIndex = -1;

        [ObservableProperty]
        private string _selectedCategory = AllCategories;

        [ObservableProperty]
        private string _displayedEmoji = "🤔";

        [ObservableProperty]
        private string _displayedName = "¿Qué comemos hoy?";

        [ObservableProperty]
        private string _displayedCategory = string.Empty;

        [ObservableProperty]
        private string _funPhrase = "Pulsa el botón y deja que el azar decida";

        [ObservableProperty]
        private bool _hasResult;

        [ObservableProperty]
        private bool _isPicking;

        [ObservableProperty]
        private bool _isManagementVisible;

        [ObservableProperty]
        private string _inputName = string.Empty;

        [ObservableProperty]
        private string _inputEmoji = "🍕";

        [ObservableProperty]
        private string _inputCategory = "Comida";

        public ObservableCollection<string> Filters { get; } = new();
        public ObservableCollection<FoodItem> FilteredFoods { get; } = new();

        public bool IsEditing => EditingItem != null;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEditing))]
        private FoodItem? _editingItem;

        public FoodTabViewModel(ISettingsService settings)
        {
            _settings = settings;

            Filters.Add(AllCategories);
            foreach (var c in Categories)
                Filters.Add(c);

            LoadFoods();
            RefreshFiltered();

            _slotTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(80)
            };
            _slotTimer.Tick += OnSlotTick;
        }

        private void LoadFoods()
        {
            var saved = _settings.GetSetting<List<FoodItem>>("Foods", null);
            if (saved is { Count: > 0 })
            {
                _foods.AddRange(saved);
            }
            else
            {
                _foods.AddRange(SeedFoods.Select(f => new FoodItem
                {
                    Name = f.Name,
                    Emoji = f.Emoji,
                    Category = f.Category
                }));
                Persist();
            }
        }

        private void Persist()
        {
            _settings.SetSetting("Foods", _foods);
            _settings.Save();
        }

        partial void OnSelectedCategoryChanged(string value) => RefreshFiltered();

        private void RefreshFiltered()
        {
            FilteredFoods.Clear();
            foreach (var f in _foods.Where(f =>
                         SelectedCategory == AllCategories || f.Category == SelectedCategory))
            {
                FilteredFoods.Add(f);
            }
        }

        [RelayCommand]
        private void ToggleManagement() => IsManagementVisible = !IsManagementVisible;

        [RelayCommand]
        private void SelectCategory(string? category)
        {
            if (!string.IsNullOrEmpty(category))
                SelectedCategory = category;
        }

        [RelayCommand]
        private void Pick()
        {
            if (IsPicking)
                return;

            _pool = _foods.Where(f =>
                SelectedCategory == AllCategories || f.Category == SelectedCategory).ToList();

            if (_pool.Count == 0)
            {
                DisplayedEmoji = "💡";
                DisplayedName = SelectedCategory == AllCategories
                    ? "No hay comidas todavía"
                    : $"No hay comidas en {SelectedCategory}";
                DisplayedCategory = string.Empty;
                FunPhrase = "¡Añade alguna abajo! 👇";
                HasResult = false;
                return;
            }

            _lastPickIndex = -1;
            _slotTick = 0;
            HasResult = true;
            IsPicking = true;
            _slotTimer.Start();
        }

        private void OnSlotTick(object? sender, EventArgs e)
        {
            if (_pool.Count == 0)
            {
                _slotTimer.Stop();
                IsPicking = false;
                return;
            }

            _slotTick++;
            var preview = _pool[_random.Next(_pool.Count)];
            DisplayedEmoji = preview.Emoji;
            DisplayedName = preview.Name;
            DisplayedCategory = preview.Category;

            if (_slotTick < SlotTicks)
                return;

            _slotTimer.Stop();

            int finalIndex;
            do
            {
                finalIndex = _random.Next(_pool.Count);
            } while (finalIndex == _lastPickIndex && _pool.Count > 1);
            _lastPickIndex = finalIndex;

            var winner = _pool[finalIndex];
            DisplayedEmoji = winner.Emoji;
            DisplayedName = winner.Name;
            DisplayedCategory = winner.Category;
            FunPhrase = NextPhrase();
            IsPicking = false;
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
        private void SaveFood()
        {
            var name = InputName.Trim();
            if (string.IsNullOrEmpty(name))
                return;

            if (EditingItem == null)
            {
                _foods.Add(new FoodItem
                {
                    Name = name,
                    Emoji = string.IsNullOrEmpty(InputEmoji) ? "🍽️" : InputEmoji,
                    Category = InputCategory
                });
            }
            else
            {
                EditingItem.Name = name;
                EditingItem.Emoji = string.IsNullOrEmpty(InputEmoji) ? "🍽️" : InputEmoji;
                EditingItem.Category = InputCategory;
                EditingItem = null;
            }

            InputName = string.Empty;
            Persist();
            RefreshFiltered();
        }

        [RelayCommand]
        private void EditFood(FoodItem? item)
        {
            if (item == null)
                return;

            EditingItem = item;
            InputName = item.Name;
            InputEmoji = item.Emoji;
            InputCategory = item.Category;
        }

        [RelayCommand]
        private void CancelEdit()
        {
            EditingItem = null;
            InputName = string.Empty;
        }

        [RelayCommand]
        private void DeleteFood(FoodItem? item)
        {
            if (item == null)
                return;

            if (EditingItem == item)
            {
                EditingItem = null;
                InputName = string.Empty;
            }

            _foods.Remove(item);
            Persist();
            RefreshFiltered();
        }
    }
}
