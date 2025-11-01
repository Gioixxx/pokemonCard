using PokemonCardManager.Data;
using PokemonCardManager.Models;
using PokemonCardManager.Services;
using System;
using System.Windows;
using System.Windows.Input;
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

            // Inizializza il colore del profitto
            txtNetProfit.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#64748B"));

            // Aggiungi handler per calcolare il profitto in tempo reale
            txtSalePrice.TextChanged += UpdateNetProfit;
            txtFee.TextChanged += UpdateNetProfit;
            txtShippingCost.TextChanged += UpdateNetProfit;
            txtQuantity.TextChanged += UpdateNetProfit;

            // Imposta focus sul primo campo
            Loaded += (s, e) => cmbCard.Focus();

            // Supporto per shortcut da tastiera
            KeyDown += SaleDialog_KeyDown;
        }

        private void SaleDialog_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // ESC per chiudere
            if (e.Key == Key.Escape)
            {
                BtnCancel_Click(null, null);
                e.Handled = true;
            }
            // Enter per salvare (se siamo in un TextBox, solo se Ctrl+Enter)
            else if (e.Key == Key.Enter && 
                     (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                BtnSave_Click(null, null);
                e.Handled = true;
            }
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
            {
                txtNetProfit.Text = "€ 0,00";
                txtNetProfit.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#64748B"));
                return;
            }

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
            
            // Cambia colore in base al profitto
            if (netProfit > 0)
            {
                txtNetProfit.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#10B981")); // SuccessColor
            }
            else if (netProfit < 0)
            {
                txtNetProfit.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EF4444")); // DangerColor
            }
            else
            {
                txtNetProfit.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#64748B")); // TextSecondaryColor
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Reset evidenziazione errori
            ResetValidationErrors();

            // Validazione campi obbligatori
            bool hasErrors = false;
            if (cmbCard.SelectedItem == null)
            {
                HighlightError(cmbCard);
                hasErrors = true;
            }
            if (!dpSaleDate.SelectedDate.HasValue)
            {
                HighlightError(dpSaleDate);
                hasErrors = true;
            }
            if (string.IsNullOrWhiteSpace(txtSalePrice.Text) || !decimal.TryParse(txtSalePrice.Text, out _))
            {
                HighlightError(txtSalePrice);
                hasErrors = true;
            }
            if (string.IsNullOrWhiteSpace(txtQuantity.Text) || !int.TryParse(txtQuantity.Text, out _) || int.Parse(txtQuantity.Text) < 1)
            {
                HighlightError(txtQuantity);
                hasErrors = true;
            }

            if (hasErrors)
            {
                MessageBox.Show("I campi Carta, Data Vendita, Prezzo di Vendita e Quantità sono obbligatori.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void HighlightError(System.Windows.Controls.Control control)
        {
            // Evidenzia il controllo con bordo rosso
            if (control is System.Windows.Controls.TextBox textBox)
            {
                textBox.BorderBrush = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EF4444"));
                textBox.BorderThickness = new Thickness(2);
            }
            else if (control is System.Windows.Controls.ComboBox comboBox)
            {
                comboBox.BorderBrush = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EF4444"));
                comboBox.BorderThickness = new Thickness(2);
            }
            else if (control is System.Windows.Controls.DatePicker datePicker)
            {
                datePicker.BorderBrush = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EF4444"));
                datePicker.BorderThickness = new Thickness(2);
            }
        }

        private void ResetValidationErrors()
        {
            // Reset tutti i controlli
            cmbCard.BorderBrush = (System.Windows.Media.Brush)FindResource("BorderColor");
            cmbCard.BorderThickness = new Thickness(1);
            dpSaleDate.BorderBrush = (System.Windows.Media.Brush)FindResource("BorderColor");
            dpSaleDate.BorderThickness = new Thickness(1);
            txtSalePrice.BorderBrush = (System.Windows.Media.Brush)FindResource("BorderColor");
            txtSalePrice.BorderThickness = new Thickness(1);
            txtQuantity.BorderBrush = (System.Windows.Media.Brush)FindResource("BorderColor");
            txtQuantity.BorderThickness = new Thickness(1);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}