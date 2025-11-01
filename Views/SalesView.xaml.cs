using PokemonCardManager.Data;
using PokemonCardManager.Models;
using PokemonCardManager.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace PokemonCardManager.Views
{
    public partial class SalesView : Page
    {
        private readonly ISaleService _saleService;
        private readonly ILogger _logger;
        private ObservableCollection<Sale> Sales { get; set; }

        public SalesView(ISaleService saleService, ILogger logger)
        {
            InitializeComponent();

            _saleService = saleService ?? throw new ArgumentNullException(nameof(saleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogDebug("SalesView initialized");
            LoadSales();
            UpdateSummary();
        }

        private void LoadSales()
        {
            var sales = _saleService.GetAllSales();
            Sales = new ObservableCollection<Sale>(sales);
            dgSales.ItemsSource = Sales;
        }

        private void UpdateSummary()
        {
            if (Sales != null)
            {
                txtTotalSales.Text = Sales.Count.ToString();
                txtTotalRevenue.Text = $"€ {Sales.Sum(s => s.SalePrice):N2}";
                txtTotalProfit.Text = $"€ {Sales.Sum(s => s.NetProfit):N2}";
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterSales();
        }

        private void DpDateFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterSales();
        }

        private void FilterSales()
        {
            string searchText = txtSearch.Text?.ToLower() ?? "";
            DateTime? selectedDate = dpDateFilter.SelectedDate;
            
            var filteredSales = _saleService.GetAllSales();
            
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredSales = filteredSales.Where(s => 
                    s.Card.Name.ToLower().Contains(searchText) || 
                    s.Card.Set.ToLower().Contains(searchText)).ToList();
            }
            
            if (selectedDate.HasValue)
            {
                filteredSales = filteredSales.Where(s => 
                    s.SaleDate.Date == selectedDate.Value.Date).ToList();
            }
            
            Sales = new ObservableCollection<Sale>(filteredSales);
            dgSales.ItemsSource = Sales;
            UpdateSummary();
        }

        private void BtnAddSale_Click(object sender, RoutedEventArgs e)
        {
            var saleDialog = new SaleDialog();
            if (saleDialog.ShowDialog() == true)
            {
                Sale newSale = saleDialog.SaleData;
                _saleService.AddSale(newSale);
                Sales.Add(newSale);
                UpdateSummary();
            }
        }

        private void BtnDeleteSale_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var sale = button.DataContext as Sale;
            
            var result = MessageBox.Show($"Sei sicuro di voler eliminare questa vendita?", 
                "Conferma eliminazione", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
            if (result == MessageBoxResult.Yes)
            {
                _saleService.DeleteSale(sale.Id);
                Sales.Remove(sale);
                UpdateSummary();
            }
        }
    }
}