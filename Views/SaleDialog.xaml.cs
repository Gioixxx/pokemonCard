using PokemonCardManager.Data;
using PokemonCardManager.Models;
using PokemonCardManager.Services;
using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace PokemonCardManager.Views
{
    public partial class SaleDialog : Window
    {
        private readonly ICardService _cardService;
        public Sale SaleData { get; private set; }

        public SaleDialog()
        {
            InitializeComponent();

            // In un'applicazione reale, questo verrebbe iniettato
            var dbContext = App.ServiceProvider?.GetService<ApplicationDbContext>();
            _cardService = new CardService(dbContext!);

            SaleData = new Sale();
            dpSaleDate.SelectedDate = DateTime.Today;

            LoadCards();

            // Aggiungi handler per calcolare il profitto in tempo reale
            txtSalePrice.TextChanged += UpdateNetProfit;
            txtFee.TextChanged += UpdateNetProfit;
            txtShippingCost.TextChanged += UpdateNetProfit;
            txtQuantity.TextChanged += UpdateNetProfit;
        }

        private void LoadCards()
        {
            var cards = _cardService.GetAllCards();
            cmbCard.ItemsSource = cards;
        }

        private void CmbCard_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateNetProfit(null, null);
        }

        private void UpdateNetProfit(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var selectedCard = cmbCard.SelectedItem as Card;
            if (selectedCard == null)
                return;

            decimal salePrice = 0;
            decimal.TryParse(txtSalePrice.Text, out salePrice);
            
            decimal fee = 0;
            decimal.TryParse(txtFee.Text, out fee);
            
            decimal shippingCost = 0;
            decimal.TryParse(txtShippingCost.Text, out shippingCost);
            
            int quantity = 1;
            int.TryParse(txtQuantity.Text, out quantity);
            if (quantity < 1) quantity = 1;
            
            decimal netProfit = salePrice - fee - shippingCost - (selectedCard.PurchasePrice * quantity);
            txtNetProfit.Text = $"€ {netProfit:N2}";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validazione campi obbligatori
            if (cmbCard.SelectedItem == null || !dpSaleDate.SelectedDate.HasValue || string.IsNullOrWhiteSpace(txtSalePrice.Text))
            {
                MessageBox.Show("I campi Carta, Data Vendita e Prezzo di Vendita sono obbligatori.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Aggiorna i dati della vendita
            SaleData.Card = cmbCard.SelectedItem as Card;
            SaleData.CardId = SaleData.Card.Id;
            SaleData.SaleDate = dpSaleDate.SelectedDate.Value;
            
            decimal salePrice;
            if (decimal.TryParse(txtSalePrice.Text, out salePrice))
                SaleData.SalePrice = salePrice;
            
            decimal fee;
            if (decimal.TryParse(txtFee.Text, out fee))
                SaleData.Fee = fee;
            
            decimal shippingCost;
            if (decimal.TryParse(txtShippingCost.Text, out shippingCost))
                SaleData.ShippingCost = shippingCost;
            
            int quantity;
            if (int.TryParse(txtQuantity.Text, out quantity) && quantity > 0)
                SaleData.Quantity = quantity;
            else
                SaleData.Quantity = 1;

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}