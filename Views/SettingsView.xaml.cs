using Microsoft.Win32;
using PokemonCardManager.Services;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace PokemonCardManager.Views
{
    public partial class SettingsView : Page
    {
        private readonly IDataExportService _dataExportService;
        private readonly ILogger _logger;

        public SettingsView(IDataExportService dataExportService, ILogger logger)
        {
            InitializeComponent();

            _dataExportService = dataExportService ?? throw new System.ArgumentNullException(nameof(dataExportService));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));

            _logger.LogDebug("SettingsView initialized");
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