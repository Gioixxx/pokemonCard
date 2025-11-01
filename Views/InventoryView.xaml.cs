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
    public partial class InventoryView : Page
    {
        private readonly ICardService _cardService;
        private readonly ILogger _logger;
        private ObservableCollection<Card> Cards { get; set; }
        private Card selectedCard;

        public InventoryView(ICardService cardService, ILogger logger)
        {
            InitializeComponent();

            _cardService = cardService ?? throw new ArgumentNullException(nameof(cardService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogDebug("InventoryView initialized");
            LoadCards();
            UpdateSummary();
        }

        private void LoadCards()
        {
            var cards = _cardService.GetAllCards();
            Cards = new ObservableCollection<Card>(cards);
            dgCards.ItemsSource = Cards;
        }

        private void UpdateSummary()
        {
            if (Cards != null)
            {
                txtTotalCards.Text = Cards.Sum(c => c.Quantity).ToString();
                txtTotalValue.Text = $"€ {Cards.Sum(c => c.TotalValue):N2}";
                txtTotalProfit.Text = $"€ {Cards.Sum(c => c.EstimatedProfit):N2}";
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = txtSearch.Text.ToLower();
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadCards();
            }
            else
            {
                var filteredCards = _cardService.GetAllCards().Where(c => 
                    c.Name.ToLower().Contains(searchText) || 
                    c.Set.ToLower().Contains(searchText) || 
                    c.Number.ToLower().Contains(searchText));
                
                Cards = new ObservableCollection<Card>(filteredCards);
                dgCards.ItemsSource = Cards;
            }
            
            UpdateSummary();
        }

        private void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Implementazione filtri
        }

        private void BtnAddCard_Click(object sender, RoutedEventArgs e)
        {
            var cardDialog = new CardDialog();
            if (cardDialog.ShowDialog() == true)
            {
                Card newCard = cardDialog.CardData;
                _cardService.AddCard(newCard);
                Cards.Add(newCard);
                UpdateSummary();
            }
        }

        private void DgCards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCard = dgCards.SelectedItem as Card;
        }

        private void BtnEditCard_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var card = button.DataContext as Card;
            
            var cardDialog = new CardDialog(card);
            if (cardDialog.ShowDialog() == true)
            {
                _cardService.UpdateCard(cardDialog.CardData);
                LoadCards();
                UpdateSummary();
            }
        }

        private void BtnDeleteCard_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var card = button.DataContext as Card;
            
            var result = MessageBox.Show($"Sei sicuro di voler eliminare la carta {card.Name}?", 
                "Conferma eliminazione", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
            if (result == MessageBoxResult.Yes)
            {
                _cardService.DeleteCard(card.Id);
                Cards.Remove(card);
                UpdateSummary();
            }
        }
    }
}