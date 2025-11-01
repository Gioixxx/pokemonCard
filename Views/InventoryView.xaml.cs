using PokemonCardManager.Models;
using PokemonCardManager.Services;
using PokemonCardManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace PokemonCardManager.Views
{
    public partial class InventoryView : Page
    {
        private readonly InventoryViewModel _viewModel;

        public InventoryView(ICardService cardService, ILogger logger, IUndoRedoService undoRedoService)
        {
            InitializeComponent();

            _viewModel = new InventoryViewModel(cardService, logger, undoRedoService);
            DataContext = _viewModel;

            // Subscribe to events
            _viewModel.OnAddCardRequested += HandleAddCard;
            _viewModel.OnEditCardRequested += HandleEditCard;
            _viewModel.OnSummaryUpdated += UpdateSummaryFromViewModel;

            // Load initial data
            _viewModel.LoadCardsCommand.Execute(null);
        }

        private void HandleAddCard()
        {
            var cardDialog = new CardDialog();
            if (cardDialog.ShowDialog() == true)
            {
                try
                {
                    _viewModel.AddCardAsync(cardDialog.CardData).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show(
                                    $"Errore durante l'aggiunta: {task.Exception?.GetBaseException()?.Message}",
                                    "Errore",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            });
                        }
                    }, System.Threading.Tasks.TaskScheduler.Default);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Errore durante l'aggiunta: {ex.Message}",
                        "Errore",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void HandleEditCard(Card card)
        {
            if (card == null) return;

            // Get fresh copy from database to ensure we have latest RowVersion
            var freshCard = App.ServiceProvider?.GetService<ICardService>()?.GetCardByIdAsync(card.Id)
                .GetAwaiter().GetResult();

            if (freshCard == null)
            {
                MessageBox.Show("Carta non trovata. Potrebbe essere stata eliminata.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cardDialog = new CardDialog(freshCard);
            if (cardDialog.ShowDialog() == true)
            {
                try
                {
                    _viewModel.UpdateCardAsync(cardDialog.CardData).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                var baseException = task.Exception?.GetBaseException();
                                if (baseException is InvalidOperationException)
                                {
                                    MessageBox.Show(
                                        baseException.Message + "\n\nRicarica i dati per vedere le modifiche recenti.",
                                        "Conflitto di Concorrenza",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                                }
                                else
                                {
                                    MessageBox.Show(
                                        $"Errore durante l'aggiornamento: {baseException?.Message}",
                                        "Errore",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                }
                            });
                        }
                    }, System.Threading.Tasks.TaskScheduler.Default);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Errore durante l'aggiornamento: {ex.Message}",
                        "Errore",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void UpdateSummaryFromViewModel()
        {
            Dispatcher.Invoke(() =>
            {
                txtTotalCards.Text = _viewModel.TotalCardsQuantity.ToString();
                txtTotalValue.Text = $"€ {_viewModel.TotalValue:N2}";
                txtTotalProfit.Text = $"€ {_viewModel.TotalProfit:N2}";
            });
        }

        private void BtnAddCard_Click(object sender, RoutedEventArgs e)
        {
            HandleAddCard();
        }

        private void BtnEditCard_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var card = button.DataContext as Card;
            HandleEditCard(card);
        }

        private void BtnDeleteCard_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var card = button.DataContext as Card;

            var result = MessageBox.Show(
                $"Sei sicuro di voler eliminare la carta {card?.Name}?",
                "Conferma eliminazione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes && card != null)
            {
                _viewModel.DeleteCardCommand.Execute(card);
            }
        }

        private void DgCards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.SelectedCard = dgCards.SelectedItem as Card;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.SearchText = txtSearch.Text;
        }

        private void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Filter implementation can be added here
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RefreshCommand.Execute(null);
        }
    }
}