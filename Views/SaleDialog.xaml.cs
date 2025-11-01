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

        private void BtnSave_Click(object? sender, RoutedEventArgs? e)
        {
            // Reset evidenziazione errori
            ResetValidationErrors();

            // Validazione campi
            var errors = new System.Collections.Generic.List<string>();

            // Validazione carta selezionata
            if (cmbCard.SelectedItem == null)
            {
                HighlightError(cmbCard);
                errors.Add("Devi selezionare una carta");
            }

            // Validazione data vendita
            if (!dpSaleDate.SelectedDate.HasValue)
            {
                HighlightError(dpSaleDate);
                errors.Add("La data di vendita è obbligatoria");
            }

            // Validazione prezzo vendita
            if (!decimal.TryParse(txtSalePrice.Text, out decimal salePrice))
            {
                HighlightError(txtSalePrice);
                errors.Add("Il prezzo di vendita deve essere un numero valido");
            }
            else if (salePrice <= 0)
            {
                HighlightError(txtSalePrice);
                errors.Add("Il prezzo di vendita deve essere maggiore di zero");
            }

            // Validazione commissione
            if (!decimal.TryParse(txtFee.Text, out decimal fee))
            {
                HighlightError(txtFee);
                errors.Add("La commissione deve essere un numero valido");
            }
            else if (fee < 0)
            {
                HighlightError(txtFee);
                errors.Add("La commissione non può essere negativa");
            }

            // Validazione costo spedizione
            if (!decimal.TryParse(txtShippingCost.Text, out decimal shippingCost))
            {
                HighlightError(txtShippingCost);
                errors.Add("Il costo di spedizione deve essere un numero valido");
            }
            else if (shippingCost < 0)
            {
                HighlightError(txtShippingCost);
                errors.Add("Il costo di spedizione non può essere negativo");
            }

            // Validazione quantità
            if (!int.TryParse(txtQuantity.Text, out int quantity))
            {
                HighlightError(txtQuantity);
                errors.Add("La quantità deve essere un numero intero valido");
            }
            else if (quantity < 1)
            {
                HighlightError(txtQuantity);
                errors.Add("La quantità deve essere almeno 1");
            }

            if (errors.Count > 0)
            {
                string errorMessage = "Correggere i seguenti errori:\n\n• " + string.Join("\n• ", errors);
                MessageBox.Show(errorMessage, "Errore di Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Aggiorna i dati della vendita (validazione già superata)
            SaleData.Card = cmbCard.SelectedItem as Card;
            if (SaleData.Card != null)
            {
                SaleData.CardId = SaleData.Card.Id;
            }
            SaleData.SaleDate = dpSaleDate.SelectedDate!.Value;
            SaleData.SalePrice = salePrice;
            SaleData.Fee = fee;
            SaleData.ShippingCost = shippingCost;
            SaleData.Quantity = quantity;

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
            var borderBrush = (System.Windows.Media.Brush)FindResource("BorderColor");
            var thickness = new Thickness(1);

            // Reset tutti i controlli
            cmbCard.BorderBrush = borderBrush;
            cmbCard.BorderThickness = thickness;
            dpSaleDate.BorderBrush = borderBrush;
            dpSaleDate.BorderThickness = thickness;
            txtSalePrice.BorderBrush = borderBrush;
            txtSalePrice.BorderThickness = thickness;
            txtFee.BorderBrush = borderBrush;
            txtFee.BorderThickness = thickness;
            txtShippingCost.BorderBrush = borderBrush;
            txtShippingCost.BorderThickness = thickness;
            txtQuantity.BorderBrush = borderBrush;
            txtQuantity.BorderThickness = thickness;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}