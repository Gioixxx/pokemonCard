using PokemonCardManager.Models;
using PokemonCardManager.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows;

namespace PokemonCardManager.ViewModels
{
    public class InventoryViewModel : INotifyPropertyChanged
    {
        private readonly ICardService _cardService;
        private readonly ILogger _logger;
        private readonly Dispatcher _dispatcher;

        private ObservableCollection<Card> _cards;
        private Card _selectedCard;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _totalItems;
        private int _totalPages;


        public InventoryViewModel(ICardService cardService, ILogger logger)
        {
            _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dispatcher = Dispatcher.CurrentDispatcher;

            _cards = new ObservableCollection<Card>();

            // Initialize commands
            LoadCardsCommand = new RelayCommandAsync(async () => await LoadCardsAsync());
            SearchCommand = new RelayCommandAsync(async () => await SearchCardsAsync(), () => !IsLoading);
            AddCardCommand = new RelayCommand(() => OnAddCardRequested?.Invoke());
            EditCardCommand = new RelayCommand<Card>(card => OnEditCardRequested?.Invoke(card), card => card != null);
            DeleteCardCommand = new RelayCommandAsync<Card>(async card => await DeleteCardAsync(card), card => card != null && !IsLoading);
            RefreshCommand = new RelayCommandAsync(async () => await LoadCardsAsync(), () => !IsLoading);
            FirstPageCommand = new RelayCommandAsync(async () => await GoToPageAsync(1), () => CurrentPage > 1 && !IsLoading);
            PreviousPageCommand = new RelayCommandAsync(async () => await GoToPageAsync(CurrentPage - 1), () => CurrentPage > 1 && !IsLoading);
            NextPageCommand = new RelayCommandAsync(async () => await GoToPageAsync(CurrentPage + 1), () => CurrentPage < TotalPages && !IsLoading);
            LastPageCommand = new RelayCommandAsync(async () => await GoToPageAsync(TotalPages), () => CurrentPage < TotalPages && !IsLoading);

            // Load initial data
            _ = LoadCardsAsync();
        }

        public ObservableCollection<Card> Cards
        {
            get => _cards;
            set
            {
                _cards = value;
                OnPropertyChanged();
            }
        }

        public Card SelectedCard
        {
            get => _selectedCard;
            set
            {
                _selectedCard = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    // Auto-search after delay (debounce)
                    _ = Task.Delay(500).ContinueWith(_ => _dispatcher.InvokeAsync(async () => await SearchCardsAsync()));
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                // Update command states
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PageInfo));
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (_pageSize != value && value > 0)
                {
                    _pageSize = value;
                    OnPropertyChanged();
                    CurrentPage = 1; // Reset to first page when page size changes
                    _ = LoadCardsAsync();
                }
            }
        }

        public int TotalItems
        {
            get => _totalItems;
            set
            {
                _totalItems = value;
                OnPropertyChanged();
                TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PageInfo));
            }
        }

        public string PageInfo => $"Pagina {CurrentPage} di {TotalPages} ({TotalItems} carte totali)";

        // Summary properties
        public int TotalCardsQuantity => Cards?.Sum(c => c.Quantity) ?? 0;
        public decimal TotalValue => Cards?.Sum(c => c.TotalValue) ?? 0;
        public decimal TotalProfit => Cards?.Sum(c => c.EstimatedProfit) ?? 0;

        // Commands
        public ICommand LoadCardsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AddCardCommand { get; }
        public ICommand EditCardCommand { get; }
        public ICommand DeleteCardCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand FirstPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LastPageCommand { get; }

        // Events
        public event Action OnAddCardRequested;
        public event Action<Card> OnEditCardRequested;
        public event Action OnSummaryUpdated;

        public async Task LoadCardsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Caricamento carte...";

                // Use server-side pagination for better performance
                var pagedResult = await _cardService.GetCardsPagedAsync(CurrentPage, PageSize, SearchText);
                TotalItems = pagedResult.TotalCount;

                // Ensure current page is valid
                if (CurrentPage > TotalPages && TotalPages > 0)
                {
                    CurrentPage = TotalPages;
                    return; // Will reload with correct page
                }
                if (CurrentPage < 1)
                {
                    CurrentPage = 1;
                    return; // Will reload with correct page
                }

                await _dispatcher.InvokeAsync(() =>
                {
                    Cards = new ObservableCollection<Card>(pagedResult.Items);
                    UpdateSummary();
                });

                StatusMessage = $"Caricate {pagedResult.Items.Count} carte";
                _logger.LogDebug($"Loaded {pagedResult.Items.Count} cards (page {CurrentPage})");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore durante il caricamento: {ex.Message}";
                _logger.LogError($"Error loading cards: {ex.Message}", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SearchCardsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Ricerca in corso...";

                // Reset to first page on search and use server-side pagination
                CurrentPage = 1;
                await LoadCardsAsync(); // LoadCardsAsync now handles search via SearchText parameter
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore durante la ricerca: {ex.Message}";
                _logger.LogError($"Error searching cards: {ex.Message}", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task DeleteCardAsync(Card card)
        {
            if (card == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Eliminazione in corso...";

                await _cardService.DeleteCardAsync(card.Id);
                Cards.Remove(card);
                TotalItems--;
                UpdateSummary();
                StatusMessage = "Carta eliminata con successo";
                _logger.LogInformation($"Card deleted: {card.Name}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore durante l'eliminazione: {ex.Message}";
                _logger.LogError($"Error deleting card: {ex.Message}", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RefreshCardAsync(Card card)
        {
            if (card == null) return;

            try
            {
                var updatedCard = await _cardService.GetCardByIdAsync(card.Id);
                if (updatedCard != null)
                {
                    var index = Cards.IndexOf(card);
                    if (index >= 0)
                    {
                        Cards[index] = updatedCard;
                        UpdateSummary();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error refreshing card: {ex.Message}", ex);
            }
        }

        public async Task AddCardAsync(Card card)
        {
            if (card == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Aggiunta carta...";

                await _cardService.AddCardAsync(card);
                await LoadCardsAsync(); // Reload to get updated data
                StatusMessage = "Carta aggiunta con successo";
                _logger.LogInformation($"Card added: {card.Name}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore durante l'aggiunta: {ex.Message}";
                _logger.LogError($"Error adding card: {ex.Message}", ex);
                throw;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task UpdateCardAsync(Card card)
        {
            if (card == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Aggiornamento carta...";

                await _cardService.UpdateCardAsync(card);
                await RefreshCardAsync(card);
                UpdateSummary();
                StatusMessage = "Carta aggiornata con successo";
                _logger.LogInformation($"Card updated: {card.Name}");
            }
            catch (InvalidOperationException ex)
            {
                StatusMessage = ex.Message;
                _logger.LogWarning($"Concurrency conflict updating card: {ex.Message}");
                // Reload the card to get latest version
                await RefreshCardAsync(card);
                throw;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore durante l'aggiornamento: {ex.Message}";
                _logger.LogError($"Error updating card: {ex.Message}", ex);
                throw;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task GoToPageAsync(int page)
        {
            if (page < 1 || page > TotalPages) return;

            CurrentPage = page;
            await LoadCardsAsync();
        }

        private void UpdateSummary()
        {
            OnPropertyChanged(nameof(TotalCardsQuantity));
            OnPropertyChanged(nameof(TotalValue));
            OnPropertyChanged(nameof(TotalProfit));
            OnSummaryUpdated?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

