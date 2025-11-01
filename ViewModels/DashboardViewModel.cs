using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using PokemonCardManager.Models;
using PokemonCardManager.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace PokemonCardManager.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly ICardService _cardService;
        private readonly ISaleService _saleService;
        private readonly ILogger _logger;
        private readonly Dispatcher _dispatcher;

        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private decimal _collectionValue;
        private decimal _totalSpent;
        private decimal _totalEarned;
        private decimal _averageROI;
        private ISeries[] _pieSeries = Array.Empty<ISeries>();
        private ISeries[] _barSeries = Array.Empty<ISeries>();
        private Axis[] _xAxes = Array.Empty<Axis>();
        private Axis[] _yAxes = Array.Empty<Axis>();

        public DashboardViewModel(ICardService cardService, ISaleService saleService, ILogger logger)
        {
            _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
            _saleService = saleService ?? throw new ArgumentNullException(nameof(saleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dispatcher = Dispatcher.CurrentDispatcher;

            LoadDashboardCommand = new RelayCommandAsync(async () => await LoadDashboardDataAsync());

            // Load initial data
            _ = LoadDashboardDataAsync();
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
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

        public decimal CollectionValue
        {
            get => _collectionValue;
            set
            {
                _collectionValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CollectionValueFormatted));
            }
        }

        public string CollectionValueFormatted => $"€ {CollectionValue:N2}";

        public decimal TotalSpent
        {
            get => _totalSpent;
            set
            {
                _totalSpent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalSpentFormatted));
            }
        }

        public string TotalSpentFormatted => $"€ {TotalSpent:N2}";

        public decimal TotalEarned
        {
            get => _totalEarned;
            set
            {
                _totalEarned = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalEarnedFormatted));
            }
        }

        public string TotalEarnedFormatted => $"€ {TotalEarned:N2}";

        public decimal AverageROI
        {
            get => _averageROI;
            set
            {
                _averageROI = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AverageROIFormatted));
            }
        }

        public string AverageROIFormatted => $"{AverageROI:N2}%";

        public ISeries[] PieSeries
        {
            get => _pieSeries;
            set
            {
                _pieSeries = value;
                OnPropertyChanged();
            }
        }

        public ISeries[] BarSeries
        {
            get => _barSeries;
            set
            {
                _barSeries = value;
                OnPropertyChanged();
            }
        }

        public Axis[] XAxes
        {
            get => _xAxes;
            set
            {
                _xAxes = value;
                OnPropertyChanged();
            }
        }

        public Axis[] YAxes
        {
            get => _yAxes;
            set
            {
                _yAxes = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadDashboardCommand { get; }

        public async Task LoadDashboardDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Caricamento dati dashboard...";

                // Load data asynchronously
                var cardsTask = _cardService.GetAllCardsAsync();
                var salesTask = _saleService.GetAllSalesAsync();

                await Task.WhenAll(cardsTask, salesTask);

                var cards = await cardsTask;
                var sales = await salesTask;

                // Calculate statistics
                decimal collectionValue = cards.Sum(c => c.TotalValue);
                decimal totalSpent = cards.Sum(c => c.PurchasePrice * c.Quantity);
                decimal totalEarned = sales.Sum(s => s.SalePrice);
                decimal averageROI = totalSpent > 0 ? (totalEarned - totalSpent) / totalSpent * 100 : 0;

                // Update properties on UI thread
                await _dispatcher.InvokeAsync(() =>
                {
                    CollectionValue = collectionValue;
                    TotalSpent = totalSpent;
                    TotalEarned = totalEarned;
                    AverageROI = averageROI;
                });

                // Prepare pie chart data for most profitable sets
                var setData = cards
                    .GroupBy(c => c.Set)
                    .Select(g => new { Set = g.Key, Profit = g.Sum(c => c.EstimatedProfit) })
                    .OrderByDescending(x => x.Profit)
                    .Take(5)
                    .ToList();

                var pieSeries = new ISeries[]
                {
                    new PieSeries<decimal>
                    {
                        Values = setData.Select(x => x.Profit).ToArray(),
                        Name = "Profitto per Set",
                        DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                        DataLabelsFormatter = point => $"{setData[point.Context.Entity.EntityIndex].Set}: €{point.PrimaryValue:N2}",
                        Fill = new SolidColorPaint(SKColors.Blue)
                    }
                };

                // Prepare bar chart data for sales trend
                var salesByMonth = sales
                    .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
                    .Select(g => new
                    {
                        Date = $"{g.Key.Month}/{g.Key.Year}",
                        Revenue = g.Sum(s => s.SalePrice),
                        Profit = g.Sum(s => s.NetProfit)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                var barSeries = new ISeries[]
                {
                    new ColumnSeries<decimal>
                    {
                        Values = salesByMonth.Select(x => x.Revenue).ToArray(),
                        Name = "Ricavi",
                        Fill = new SolidColorPaint(SKColors.Green)
                    },
                    new ColumnSeries<decimal>
                    {
                        Values = salesByMonth.Select(x => x.Profit).ToArray(),
                        Name = "Profitti",
                        Fill = new SolidColorPaint(SKColors.Blue)
                    }
                };

                var xAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = salesByMonth.Select(x => x.Date).ToArray(),
                        LabelsRotation = 45
                    }
                };

                var yAxes = new Axis[]
                {
                    new Axis
                    {
                        Name = "Euro (€)",
                        NamePaint = new SolidColorPaint(SKColors.Black),
                        LabelsPaint = new SolidColorPaint(SKColors.Gray)
                    }
                };

                // Update chart properties on UI thread
                await _dispatcher.InvokeAsync(() =>
                {
                    PieSeries = pieSeries;
                    BarSeries = barSeries;
                    XAxes = xAxes;
                    YAxes = yAxes;
                });

                StatusMessage = "Dashboard aggiornato";
                _logger.LogDebug("Dashboard data loaded successfully");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Errore durante il caricamento: {ex.Message}";
                _logger.LogError($"Error loading dashboard data: {ex.Message}", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

