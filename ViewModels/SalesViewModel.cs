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

namespace PokemonCardManager.ViewModels
{
    public class SalesViewModel : INotifyPropertyChanged
    {
        private readonly ISaleService _saleService;
        private readonly ILogger _logger;
        private readonly IUndoRedoService _undoRedoService;
        private readonly Dispatcher _dispatcher;

        private ObservableCollection<Sale> _sales;
        private Sale _selectedSale;
        private string _searchText = string.Empty;
        private DateTime? _dateFilter;
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _totalItems;
        private int _totalPages;
        private int _totalSalesCount;
        private decimal _totalRevenue;
        private decimal _totalProfit;

        public SalesViewModel(ISaleService saleService, ILogger logger, IUndoRedoService undoRedoService)
        {
            _saleService = saleService ?? throw new ArgumentNullException(nameof(saleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _undoRedoService = undoRedoService ?? throw new ArgumentNullException(nameof(undoRedoService));
            _dispatcher = Dispatcher.CurrentDispatcher;

            _sales = new ObservableCollection<Sale>();

            // Initialize commands
            LoadSalesCommand = new RelayCommandAsync(async () => await LoadSalesAsync());
            SearchCommand = new RelayCommandAsync(async () => await SearchSalesAsync(), () => !IsLoading);
            AddSaleCommand = new RelayCommand(() => OnAddSaleRequested?.Invoke());
            DeleteSaleCommand = new RelayCommandAsync<Sale>(async sale => await DeleteSaleAsync(sale), sale => sale != null && !IsLoading);
            RefreshCommand = new RelayCommandAsync(async () => await LoadSalesAsync(), () => !IsLoading);
            FirstPageCommand = new RelayCommandAsync(async () => await GoToPageAsync(1), () => CurrentPage > 1 && !IsLoading);
            PreviousPageCommand = new RelayCommandAsync(async () => await GoToPageAsync(CurrentPage - 1), () => CurrentPage > 1 && !IsLoading);
            NextPageCommand = new RelayCommandAsync(async () => await GoToPageAsync(CurrentPage + 1), () => CurrentPage < TotalPages && !IsLoading);
            LastPageCommand = new RelayCommandAsync(async () => await GoToPageAsync(TotalPages), () => CurrentPage < TotalPages && !IsLoading);

            // Load initial data
            _ = LoadSalesAsync();
        }

        public ObservableCollection<Sale> Sales
        {
            get => _sales;
            set
            {
                _sales = value;
                OnPropertyChanged();
            }
        }

        public Sale SelectedSale
        {
            get => _selectedSale;
            set
            {
                _selectedSale = value;
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
                    _ = Task.Delay(500).ContinueWith(_ => _dispatcher.InvokeAsync(async () => await SearchSalesAsync()));
                }
            }
        }

        public DateTime? DateFilter
        {
            get => _dateFilter;
            set
            {
                if (_dateFilter != value)
                {
                    _dateFilter = value;
                    OnPropertyChanged();
                    // Auto-filter when date changes
                    _ = SearchSalesAsync();
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
                    _ = LoadSalesAsync();
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

        public string PageInfo => $"Pagina {CurrentPage} di {TotalPages} ({TotalItems} vendite totali)";

        public int TotalSalesCount
        {
            get => _totalSalesCount;
            set
            {
                _totalSalesCount = value;
                OnPropertyChanged();
            }
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                _totalRevenue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalRevenueFormatted));
            }
        }

        public string TotalRevenueFormatted => $"€ {TotalRevenue:N2}";

        public decimal TotalProfit
        {
            get => _totalProfit;
            set
            {
                _totalProfit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalProfitFormatted));
            }
        }

        public string TotalProfitFormatted => $"€ {TotalProfit:N2}";

        // Commands
        public ICommand LoadSalesCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AddSaleCommand { get; }
        public ICommand DeleteSaleCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand FirstPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LastPageCommand { get; }

        // Events
        public event Action OnAddSaleRequested;
        public event Action<Sale> OnSaleAdded;
        public event Action<Sale> OnSaleDeleted;

        public async Task LoadSalesAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Caricamento vendite...";

                var pagedResult = await _saleService.GetSalesPagedAsync(CurrentPage, PageSize, SearchText, DateFilter);
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

                // Load summary statistics
                var totalRevenueTask = _saleService.GetTotalRevenueAsync();
                var totalProfitTask = _saleService.GetTotalProfitAsync();
                var totalCountTask = _saleService.GetTotalSalesCountAsync();

                var totalRevenue = await totalRevenueTask;
                var totalProfit = await totalProfitTask;
                var totalCount = await totalCountTask;

                await _dispatcher.InvokeAsync(() =>
                {
                    Sales = new ObservableCollection<Sale>(pagedResult.Items);
                    TotalRevenue = totalRevenue;
                    TotalProfit = totalProfit;
                    TotalSalesCount = totalCount;
                });

                StatusMessage = $"Caricate {pagedResult.Items.Count} vendite";
                _logger.LogDebug($"Loaded {pagedResult.Items.Count} sales (page {CurrentPage})");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore durante il caricamento: {ex.Message}";
                _logger.LogError($"Error loading sales: {ex.Message}", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SearchSalesAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Ricerca in corso...";

                CurrentPage = 1; // Reset to first page on search
                await LoadSalesAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore durante la ricerca: {ex.Message}";
                _logger.LogError($"Error searching sales: {ex.Message}", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task AddSaleAsync(Sale sale)
        {
            if (sale == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Aggiunta vendita...";

                // Create a copy for undo
                var saleCopy = new Sale
                {
                    CardId = sale.CardId,
                    SaleDate = sale.SaleDate,
                    SalePrice = sale.SalePrice,
                    Fee = sale.Fee,
                    ShippingCost = sale.ShippingCost,
                    Quantity = sale.Quantity
                };

                await _saleService.AddSaleAsync(sale);
                
                // Record operation for undo
                _undoRedoService.RecordOperation(OperationType.AddSale, saleCopy);
                
                await LoadSalesAsync(); // Reload to get updated data
                StatusMessage = "Vendita aggiunta con successo";
                OnSaleAdded?.Invoke(sale);
                _logger.LogInformation($"Sale added: {sale.Card?.Name}");
            }
            catch (InvalidOperationException ex)
            {
                StatusMessage = ex.Message;
                _logger.LogWarning($"Concurrency conflict adding sale: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore durante l'aggiunta: {ex.Message}";
                _logger.LogError($"Error adding sale: {ex.Message}", ex);
                throw;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task DeleteSaleAsync(Sale sale)
        {
            if (sale == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Eliminazione in corso...";

                // Create a copy for undo
                var saleCopy = new Sale
                {
                    Id = sale.Id,
                    CardId = sale.CardId,
                    SaleDate = sale.SaleDate,
                    SalePrice = sale.SalePrice,
                    Fee = sale.Fee,
                    ShippingCost = sale.ShippingCost,
                    Quantity = sale.Quantity,
                    RowVersion = sale.RowVersion
                };

                await _saleService.DeleteSaleAsync(sale.Id);
                
                // Record operation for undo
                _undoRedoService.RecordOperation(OperationType.DeleteSale, saleCopy);
                
                await LoadSalesAsync(); // Reload to get updated data
                StatusMessage = "Vendita eliminata con successo";
                OnSaleDeleted?.Invoke(sale);
                _logger.LogInformation($"Sale deleted: {sale.Card?.Name}");
            }
            catch (InvalidOperationException ex)
            {
                StatusMessage = ex.Message;
                _logger.LogWarning($"Concurrency conflict deleting sale: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore durante l'eliminazione: {ex.Message}";
                _logger.LogError($"Error deleting sale: {ex.Message}", ex);
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
            await LoadSalesAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

