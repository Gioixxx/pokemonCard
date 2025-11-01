using PokemonCardManager.Models;
using System;
using System.Windows;

namespace PokemonCardManager.Views
{
    public partial class CardDialog : Window
    {
        public Card CardData { get; private set; }
        private bool isEditMode = false;

        public CardDialog()
        {
            InitializeComponent();
            CardData = new Card();
            dpPurchaseDate.SelectedDate = DateTime.Today;
        }

        public CardDialog(Card card)
        {
            InitializeComponent();
            CardData = card;
            isEditMode = true;
            txtTitle.Text = "Modifica Carta";
            LoadCardData();
        }

        private void LoadCardData()
        {
            txtName.Text = CardData.Name;
            txtSet.Text = CardData.Set;
            txtNumber.Text = CardData.Number;
            
            // Imposta i valori delle ComboBox
            SetComboBoxValue(cmbRarity, CardData.Rarity);
            SetComboBoxValue(cmbLanguage, CardData.Language);
            SetComboBoxValue(cmbCondition, CardData.Condition);
            
            txtPurchasePrice.Text = CardData.PurchasePrice.ToString();
            dpPurchaseDate.SelectedDate = CardData.PurchaseDate;
            txtSource.Text = CardData.Source;
            txtCurrentPrice.Text = CardData.CurrentPrice.ToString();
            txtQuantity.Text = CardData.Quantity.ToString();
        }

        private void SetComboBoxValue(System.Windows.Controls.ComboBox comboBox, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            foreach (System.Windows.Controls.ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == value)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validazione campi obbligatori
            if (string.IsNullOrWhiteSpace(txtName.Text) || 
                string.IsNullOrWhiteSpace(txtSet.Text) || 
                string.IsNullOrWhiteSpace(txtNumber.Text))
            {
                MessageBox.Show("I campi Nome, Set e Numero sono obbligatori.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Aggiorna i dati della carta
            CardData.Name = txtName.Text;
            CardData.Set = txtSet.Text;
            CardData.Number = txtNumber.Text;
            CardData.Rarity = (cmbRarity.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();
            CardData.Language = (cmbLanguage.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();
            CardData.Condition = (cmbCondition.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();
            
            decimal purchasePrice;
            if (decimal.TryParse(txtPurchasePrice.Text, out purchasePrice))
                CardData.PurchasePrice = purchasePrice;
            
            CardData.PurchaseDate = dpPurchaseDate.SelectedDate;
            CardData.Source = txtSource.Text;
            
            decimal currentPrice;
            if (decimal.TryParse(txtCurrentPrice.Text, out currentPrice))
                CardData.CurrentPrice = currentPrice;
            
            int quantity;
            if (int.TryParse(txtQuantity.Text, out quantity) && quantity > 0)
                CardData.Quantity = quantity;
            else
                CardData.Quantity = 1;

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