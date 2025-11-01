using Microsoft.Win32;
using PokemonCardManager.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace PokemonCardManager.Views
{
    public partial class SettingsView : Page
    {
        private readonly IDataExportService _dataExportService;
        private readonly IBulkImportService _bulkImportService;
        private readonly IThemeService _themeService;
        private readonly ILogger _logger;

        public SettingsView(IDataExportService dataExportService, IBulkImportService bulkImportService, IThemeService themeService, ILogger logger)
        {
            InitializeComponent();

            _dataExportService = dataExportService ?? throw new ArgumentNullException(nameof(dataExportService));
            _bulkImportService = bulkImportService ?? throw new ArgumentNullException(nameof(bulkImportService));
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Initialize dark mode toggle
            toggleDarkMode.IsChecked = _themeService.CurrentTheme == Theme.Dark;

            _logger.LogDebug("SettingsView initialized");
        }

        private void ToggleDarkMode_Checked(object sender, RoutedEventArgs e)
        {
            _themeService.SetTheme(Theme.Dark);
        }

        private void ToggleDarkMode_Unchecked(object sender, RoutedEventArgs e)
        {
            _themeService.SetTheme(Theme.Light);
        }

        private async void ImportCardsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "File CSV (*.csv)|*.csv",
                Title = "Importa carte da CSV",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var result = await _bulkImportService.ImportCardsFromCSVAsync(dialog.FileName);

                    if (result.IsSuccess)
                    {
                        MessageBox.Show(
                            $"Importazione completata con successo!\n\n" +
                            $"Totale righe: {result.TotalRows}\n" +
                            $"Importate: {result.SuccessCount}\n" +
                            $"Errori: {result.ErrorCount}",
                            "Importazione",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        var errorMessage = $"Si sono verificati errori durante l'importazione.\n\n" +
                                          $"Totale righe: {result.TotalRows}\n" +
                                          $"Importate: {result.SuccessCount}\n" +
                                          $"Errori: {result.ErrorCount}";

                        if (result.Errors.Any())
                        {
                            errorMessage += "\n\nDettagli errori:\n" + string.Join("\n", result.Errors.Take(10));
                            if (result.Errors.Count > 10)
                            {
                                errorMessage += $"\n... e altri {result.Errors.Count - 10} errori";
                            }
                        }

                        MessageBox.Show(
                            errorMessage,
                            "Importazione",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Errore durante l'importazione: {ex.Message}",
                        "Errore",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    _logger.LogError($"Error importing cards: {ex.Message}", ex);
                }
            }
        }

        private async void ExportCardsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "File CSV (*.csv)|*.csv",
                Title = "Esporta inventario carte",
                FileName = "PokemonCards_Export.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                var result = await _dataExportService.ExportCardsToCSV(dialog.FileName);
                
                if (result)
                    MessageBox.Show("Esportazione completata con successo!", "Esportazione", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Si è verificato un errore durante l'esportazione.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExportSalesButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "File CSV (*.csv)|*.csv",
                Title = "Esporta registro vendite",
                FileName = "PokemonSales_Export.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                var result = await _dataExportService.ExportSalesToCSV(dialog.FileName);
                
                if (result)
                    MessageBox.Show("Esportazione completata con successo!", "Esportazione", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Si è verificato un errore durante l'esportazione.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BackupDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "File database SQLite (*.db)|*.db",
                Title = "Backup database",
                FileName = "PokemonCardManager_Backup.db"
            };

            if (dialog.ShowDialog() == true)
            {
                var result = await _dataExportService.BackupDatabase(dialog.FileName);
                
                if (result)
                    MessageBox.Show("Backup completato con successo!", "Backup", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Si è verificato un errore durante il backup.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}