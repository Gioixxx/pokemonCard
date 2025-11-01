using PokemonCardManager.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace PokemonCardManager
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            // Carica la vista inventario all'avvio
            NavigateToInventory(null, null);

            // Supporto per shortcut da tastiera
            KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Ignora se siamo in un controllo che gestisce testo (TextBox, ComboBox, etc.)
            if (e.OriginalSource is System.Windows.Controls.TextBox || 
                e.OriginalSource is System.Windows.Controls.ComboBox ||
                e.OriginalSource is System.Windows.Controls.DatePicker)
                return;

            // Ctrl+1 - Inventario
            if (e.Key == Key.D1 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                NavigateToInventory(null, null);
                e.Handled = true;
            }
            // Ctrl+2 - Vendite
            else if (e.Key == Key.D2 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                NavigateToSales(null, null);
                e.Handled = true;
            }
            // Ctrl+3 - Dashboard
            else if (e.Key == Key.D3 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                NavigateToDashboard(null, null);
                e.Handled = true;
            }
            // Ctrl+4 - Impostazioni
            else if (e.Key == Key.D4 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                NavigateToSettings(null, null);
                e.Handled = true;
            }
        }

        private void NavigateToInventory(object? sender, RoutedEventArgs? e)
        {
            var view = _serviceProvider.GetRequiredService<InventoryView>();
            MainFrame.Navigate(view);
            SetActiveButton(btnInventory);
        }

        private void NavigateToSales(object? sender, RoutedEventArgs? e)
        {
            var view = _serviceProvider.GetRequiredService<SalesView>();
            MainFrame.Navigate(view);
            SetActiveButton(btnSales);
        }

        private void NavigateToDashboard(object? sender, RoutedEventArgs? e)
        {
            var view = _serviceProvider.GetRequiredService<DashboardView>();
            MainFrame.Navigate(view);
            SetActiveButton(btnDashboard);
        }

        private void NavigateToSettings(object? sender, RoutedEventArgs? e)
        {
            var view = _serviceProvider.GetRequiredService<SettingsView>();
            MainFrame.Navigate(view);
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