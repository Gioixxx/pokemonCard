using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using PokemonCardManager.Data;
using PokemonCardManager.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace PokemonCardManager.Views
{
    public partial class DashboardView : Page
    {
        private readonly ICardService _cardService;
        private readonly ISaleService _saleService;
        private readonly ILogger _logger;

        public ISeries[] PieSeries { get; set; }
        public ISeries[] BarSeries { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }

        public DashboardView(ICardService cardService, ISaleService saleService, ILogger logger)
        {
            InitializeComponent();

            _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
            _saleService = saleService ?? throw new ArgumentNullException(nameof(saleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogDebug("DashboardView initialized");
            LoadDashboardData();
            DataContext = this;
        }

        private void LoadDashboardData()
        {
            var cards = _cardService.GetAllCards();
            var sales = _saleService.GetAllSales();
            
            // Calcola le statistiche principali
            decimal collectionValue = cards.Sum(c => c.TotalValue);
            decimal totalSpent = cards.Sum(c => c.PurchasePrice * c.Quantity);
            decimal totalEarned = sales.Sum(s => s.SalePrice);
            decimal averageROI = totalSpent > 0 ? (totalEarned - totalSpent) / totalSpent * 100 : 0;
            
            // Aggiorna i controlli UI
            txtCollectionValue.Text = $"€ {collectionValue:N2}";
            txtTotalSpent.Text = $"€ {totalSpent:N2}";
            txtTotalEarned.Text = $"€ {totalEarned:N2}";
            txtAverageROI.Text = $"{averageROI:N2}%";
            
            // Prepara i dati per il grafico a torta dei set più redditizi
            var setData = cards
                .GroupBy(c => c.Set)
                .Select(g => new { Set = g.Key, Profit = g.Sum(c => c.EstimatedProfit) })
                .OrderByDescending(x => x.Profit)
                .Take(5)
                .ToList();
            
            PieSeries = new ISeries[]
            {
                new PieSeries<decimal>
                {
                    Values = setData.Select(x => x.Profit).ToArray(),
                    Name = "Profitto per Set",
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"{setData[point.Context.Index].Set}: €{point.PrimaryValue:N2}",
                    Fill = new SolidColorPaint(SKColors.Blue)
                }
            };
            
            // Prepara i dati per il grafico a barre dell'andamento vendite
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
            
            BarSeries = new ISeries[]
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
            
            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = salesByMonth.Select(x => x.Date).ToArray(),
                    LabelsRotation = 45
                }
            };
            
            YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Euro (€)",
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    LabelsPaint = new SolidColorPaint(SKColors.Gray)
                }
            };
        }
    }
}