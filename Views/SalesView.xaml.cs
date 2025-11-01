using PokemonCardManager.Models;
using PokemonCardManager.Services;
using PokemonCardManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PokemonCardManager.Views
{
    public partial class SalesView : Page
    {
        private readonly SalesViewModel _viewModel;

        public SalesView(ISaleService saleService, ILogger logger, IUndoRedoService undoRedoService)
        {
            InitializeComponent();

            _viewModel = new SalesViewModel(saleService, logger, undoRedoService);
            DataContext = _viewModel;

            // Subscribe to events
            _viewModel.OnAddSaleRequested += HandleAddSale;
            _viewModel.OnSaleDeleted += HandleSaleDeleted;
        }

        private void HandleAddSale()
        {
            var saleDialog = new SaleDialog();
            if (saleDialog.ShowDialog() == true)
            {
                try
                {
                    _viewModel.AddSaleAsync(saleDialog.SaleData).ContinueWith(task =>
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
                                        $"Errore durante l'aggiunta: {baseException?.Message}",
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
                        $"Errore durante l'aggiunta: {ex.Message}",
                        "Errore",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void HandleSaleDeleted(Sale sale)
        {
            // Sale already deleted, no action needed
        }

        private void BtnDeleteSale_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var sale = button.DataContext as Sale;

            if (sale == null) return;

            var result = MessageBox.Show(
                $"Sei sicuro di voler eliminare questa vendita?",
                "Conferma eliminazione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _viewModel.DeleteSaleAsync(sale).ContinueWith(task =>
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
                                        $"Errore durante l'eliminazione: {baseException?.Message}",
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
                        $"Errore durante l'eliminazione: {ex.Message}",
                        "Errore",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}