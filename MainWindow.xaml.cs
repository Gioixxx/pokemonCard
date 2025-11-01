using PokemonCardManager.Views;
using System.Windows;
using System.Windows.Controls;

namespace PokemonCardManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Carica la vista inventario all'avvio
            NavigateToInventory(null, null);
        }

        private void NavigateToInventory(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new InventoryView());
            SetActiveButton(btnInventory);
        }

        private void NavigateToSales(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SalesView());
            SetActiveButton(btnSales);
        }

        private void NavigateToDashboard(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardView());
            SetActiveButton(btnDashboard);
        }

        private void NavigateToSettings(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SettingsView());
            SetActiveButton(btnSettings);
        }

        private void SetActiveButton(Button activeButton)
        {
            // Resetta tutti i pulsanti
            btnInventory.Background = System.Windows.Media.Brushes.Transparent;
            btnSales.Background = System.Windows.Media.Brushes.Transparent;
            btnDashboard.Background = System.Windows.Media.Brushes.Transparent;
            btnSettings.Background = System.Windows.Media.Brushes.Transparent;

            // Imposta il pulsante attivo con sfondo indigo/blu scuro (NON giallo!)
            // Usa un indigo scuro semi-trasparente per evidenziare la selezione
            var activeColor = System.Windows.Media.Color.FromArgb(0x70, 0x4F, 0x46, 0xE5); // Indigo scuro #4F46E5 con 44% opacità
            var activeBrush = new System.Windows.Media.SolidColorBrush(activeColor);
            activeButton.Background = activeBrush;
            activeButton.FontWeight = FontWeights.Bold;
            
            // Resetta il font weight degli altri pulsanti
            if (activeButton != btnInventory) btnInventory.FontWeight = FontWeights.Medium;
            if (activeButton != btnSales) btnSales.FontWeight = FontWeights.Medium;
            if (activeButton != btnDashboard) btnDashboard.FontWeight = FontWeights.Medium;
            if (activeButton != btnSettings) btnSettings.FontWeight = FontWeights.Medium;
        }
    }
}