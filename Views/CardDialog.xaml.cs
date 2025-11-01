using PokemonCardManager.Models;
using PokemonCardManager.Models.Api;
using PokemonCardManager.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PokemonCardManager.Views
{
    public partial class CardDialog : Window
    {
        public Card CardData { get; private set; }
        private bool isEditMode = false;
        private readonly IPokeApiService? _pokeApiService;
        private readonly PokemonSetService? _setService;
        private readonly ILogger? _logger;

        public CardDialog()
        {
            InitializeComponent();
            CardData = new Card();
            dpPurchaseDate.SelectedDate = DateTime.Today;

            // Recupera i servizi dal DI container
            if (App.ServiceProvider != null)
            {
                _pokeApiService = App.ServiceProvider.GetService(typeof(IPokeApiService)) as IPokeApiService;
                _setService = App.ServiceProvider.GetService(typeof(PokemonSetService)) as PokemonSetService;
                _logger = App.ServiceProvider.GetService(typeof(ILogger)) as ILogger;
            }

            // Imposta focus sul primo campo
            Loaded += (s, e) => txtName.Focus();

            // Supporto per shortcut da tastiera
            KeyDown += CardDialog_KeyDown;
        }

        public CardDialog(Card card)
        {
            InitializeComponent();
            CardData = card;
            isEditMode = true;
            txtTitle.Text = "Modifica Carta";

            // Recupera i servizi dal DI container
            if (App.ServiceProvider != null)
            {
                _pokeApiService = App.ServiceProvider.GetService(typeof(IPokeApiService)) as IPokeApiService;
                _setService = App.ServiceProvider.GetService(typeof(PokemonSetService)) as PokemonSetService;
                _logger = App.ServiceProvider.GetService(typeof(ILogger)) as ILogger;
            }

            LoadCardData();

            // Imposta focus sul primo campo
            Loaded += (s, e) => txtName.Focus();

            // Supporto per shortcut da tastiera
            KeyDown += CardDialog_KeyDown;
        }

        private void CardDialog_KeyDown(object sender, KeyEventArgs e)
        {
            // ESC per chiudere
            if (e.Key == Key.Escape)
            {
                BtnCancel_Click(null, null);
                e.Handled = true;
            }
            // Ctrl+Enter per salvare
            else if (e.Key == Key.Enter && 
                     (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                BtnSave_Click(null, null);
                e.Handled = true;
            }
        }

        private void LoadCardData()
        {
            txtName.Text = CardData.Name;
            SetComboBoxValue(cmbSet, CardData.Set);
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
            // Reset evidenziazione errori
            ResetValidationErrors();

            // Validazione campi obbligatori
            bool hasErrors = false;
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                HighlightError(txtName);
                hasErrors = true;
            }
            if (string.IsNullOrWhiteSpace(cmbSet.Text))
            {
                HighlightError(cmbSet);
                hasErrors = true;
            }
            if (string.IsNullOrWhiteSpace(txtNumber.Text))
            {
                HighlightError(txtNumber);
                hasErrors = true;
            }

            if (hasErrors)
            {
                MessageBox.Show("I campi Nome, Set e Numero sono obbligatori.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Aggiorna i dati della carta
            CardData.Name = txtName.Text;
            CardData.Set = cmbSet.Text;
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
        }

        private void ResetValidationErrors()
        {
            // Reset TextBox
            txtName.BorderBrush = (System.Windows.Media.Brush)FindResource("BorderColor");
            txtName.BorderThickness = new Thickness(1);
            txtNumber.BorderBrush = (System.Windows.Media.Brush)FindResource("BorderColor");
            txtNumber.BorderThickness = new Thickness(1);
            
            // Reset ComboBox
            cmbSet.BorderBrush = (System.Windows.Media.Brush)FindResource("BorderColor");
            cmbSet.BorderThickness = new Thickness(1);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void BtnSearchPokemon_Click(object sender, RoutedEventArgs e)
        {
            if (_pokeApiService == null)
            {
                MessageBox.Show("Servizio PokéAPI non disponibile.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string pokemonName = txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(pokemonName))
            {
                MessageBox.Show("Inserisci un nome Pokémon per cercare.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            btnSearchPokemon.IsEnabled = false;
            btnSearchPokemon.Content = "Cercando...";

            try
            {
                var pokemon = await _pokeApiService.GetPokemonByNameAsync(pokemonName);
                
                if (pokemon == null)
                {
                    MessageBox.Show($"Pokémon '{pokemonName}' non trovato su PokéAPI.\n\nProva con il nome in inglese (es: 'pikachu', 'charizard').", 
                        "Pokémon non trovato", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    pokemonImageContainer.Visibility = Visibility.Collapsed;
                    return;
                }

                // Recupera informazioni sulla specie per ottenere la generazione
                PokemonSpeciesDto? species = null;
                if (pokemon.Species != null && !string.IsNullOrEmpty(pokemon.Species.Name))
                {
                    species = await _pokeApiService.GetPokemonSpeciesByNameAsync(pokemon.Species.Name);
                }

                // Popola i campi del form con i dati del Pokémon
                PopulateFormFields(pokemon, species);

                // Mostra informazioni del Pokémon
                string types = string.Join(", ", pokemon.Types.Select(t => CapitalizeFirst(t.Type.Name)));
                string info = $"#{pokemon.Id} - {CapitalizeFirst(pokemon.Name)}\nTipi: {types}";
                
                if (pokemon.Height > 0)
                    info += $"\nAltezza: {pokemon.Height / 10.0}m";
                if (pokemon.Weight > 0)
                    info += $" | Peso: {pokemon.Weight / 10.0}kg";

                txtPokemonInfo.Text = info;
                pokemonImageContainer.Visibility = Visibility.Visible;

                // Carica l'immagine del Pokémon
                if (!string.IsNullOrEmpty(pokemon.Sprites?.FrontDefault))
                {
                    await LoadPokemonImageAsync(pokemon.Sprites.FrontDefault);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError($"Network error while searching Pokemon '{pokemonName}': {ex.Message}", ex);
                MessageBox.Show($"Errore di connessione durante la ricerca.\nVerifica la tua connessione internet.",
                    "Errore di Rete", MessageBoxButton.OK, MessageBoxImage.Error);
                pokemonImageContainer.Visibility = Visibility.Collapsed;
            }
            catch (TaskCanceledException ex)
            {
                _logger?.LogError($"Request timeout while searching Pokemon '{pokemonName}': {ex.Message}", ex);
                MessageBox.Show("La richiesta ha impiegato troppo tempo.\nRiprova tra qualche istante.",
                    "Timeout", MessageBoxButton.OK, MessageBoxImage.Warning);
                pokemonImageContainer.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unexpected error while searching Pokemon '{pokemonName}': {ex.Message}", ex);
                MessageBox.Show($"Errore imprevisto durante la ricerca: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                pokemonImageContainer.Visibility = Visibility.Collapsed;
            }
            finally
            {
                btnSearchPokemon.IsEnabled = true;
                btnSearchPokemon.Content = "🔍 Cerca";
            }
        }

        private async Task LoadPokemonImageAsync(string imageUrl)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetByteArrayAsync(imageUrl);
                    var bitmap = new BitmapImage();

                    using (var stream = new MemoryStream(response))
                    {
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }

                    imgPokemon.Source = bitmap;
                    _logger?.LogDebug($"Successfully loaded Pokemon image from: {imageUrl}");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogWarning($"Failed to load Pokemon image from {imageUrl}: {ex.Message}");
                imgPokemon.Source = null;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Unexpected error loading Pokemon image: {ex.Message}");
                imgPokemon.Source = null;
            }
        }

        private void PopulateFormFields(PokemonDto pokemon, PokemonSpeciesDto? species = null)
        {
            // Popola il campo Nome
            txtName.Text = CapitalizeFirst(pokemon.Name);

            // Popola il campo Numero con l'ID nazionale del Pokémon (solo se vuoto)
            if (string.IsNullOrWhiteSpace(txtNumber.Text))
            {
                txtNumber.Text = pokemon.Id.ToString();
            }

            // Suggerisce una rarità basata su caratteristiche del Pokémon (solo se non già selezionata)
            if (cmbRarity.SelectedItem == null)
            {
                string suggestedRarity = SuggestRarity(pokemon);
                SetComboBoxValue(cmbRarity, suggestedRarity);
            }

            // Popola il ComboBox del Set con i set disponibili per questa generazione
            PopulateSetComboBox(species, pokemon.Id);
        }

        private void PopulateSetComboBox(PokemonSpeciesDto? species, int pokemonId)
        {
            if (_setService == null)
                return;

            List<string> availableSets;
            
            if (species != null && species.Generation != null)
            {
                availableSets = _setService.GetSetsForGeneration(species.Generation.Name);
            }
            else
            {
                availableSets = _setService.GetSetsForPokemonId(pokemonId);
            }

            // Pulisci e popola il ComboBox
            cmbSet.Items.Clear();
            foreach (var set in availableSets)
            {
                cmbSet.Items.Add(set);
            }

            // Se il campo è vuoto, seleziona il primo set suggerito
            if (string.IsNullOrWhiteSpace(cmbSet.Text) && availableSets.Count > 0)
            {
                string suggestedSet = species != null 
                    ? SuggestSetFromGeneration(species, pokemonId) 
                    : SuggestSetFromId(pokemonId);
                
                if (availableSets.Contains(suggestedSet))
                {
                    cmbSet.Text = suggestedSet;
                }
                else if (availableSets.Count > 0)
                {
                    cmbSet.Text = availableSets[0];
                }
            }
        }

        private string SuggestSetFromGeneration(PokemonSpeciesDto species, int pokemonId)
        {
            // Estrae il numero della generazione dall'URL (es: "generation-i" -> 1)
            string? generationName = species.Generation?.Name;
            
            if (string.IsNullOrEmpty(generationName))
            {
                // Fallback: usa l'ID per stimare la generazione
                return SuggestSetFromId(pokemonId);
            }

            // Mappa generazioni a set comuni
            if (generationName.Contains("generation-i") || generationName.Contains("generation-1"))
            {
                return "Base Set";
            }
            else if (generationName.Contains("generation-ii") || generationName.Contains("generation-2"))
            {
                return "Jungle";
            }
            else if (generationName.Contains("generation-iii") || generationName.Contains("generation-3"))
            {
                return "Ruby & Sapphire";
            }
            else if (generationName.Contains("generation-iv") || generationName.Contains("generation-4"))
            {
                return "Diamond & Pearl";
            }
            else if (generationName.Contains("generation-v") || generationName.Contains("generation-5"))
            {
                return "Black & White";
            }
            else if (generationName.Contains("generation-vi") || generationName.Contains("generation-6"))
            {
                return "XY";
            }
            else if (generationName.Contains("generation-vii") || generationName.Contains("generation-7"))
            {
                return "Sun & Moon";
            }
            else if (generationName.Contains("generation-viii") || generationName.Contains("generation-8"))
            {
                return "Sword & Shield";
            }
            else if (generationName.Contains("generation-ix") || generationName.Contains("generation-9"))
            {
                return "Scarlet & Violet";
            }

            return SuggestSetFromId(pokemonId);
        }

        private string SuggestSetFromId(int pokemonId)
        {
            // Stima la generazione basata sull'ID nazionale
            if (pokemonId <= 151)
                return "Base Set";
            else if (pokemonId <= 251)
                return "Jungle";
            else if (pokemonId <= 386)
                return "Ruby & Sapphire";
            else if (pokemonId <= 493)
                return "Diamond & Pearl";
            else if (pokemonId <= 649)
                return "Black & White";
            else if (pokemonId <= 721)
                return "XY";
            else if (pokemonId <= 809)
                return "Sun & Moon";
            else if (pokemonId <= 905)
                return "Sword & Shield";
            else
                return "Scarlet & Violet";
        }

        private string SuggestRarity(PokemonDto pokemon)
        {
            // Logica per suggerire la rarità basata su:
            // - ID nazionale (i primi sono più comuni)
            // - Stats totali (i leggendari hanno stats più alte)
            // - Tipo (alcuni tipi sono più rari)

            int totalStats = pokemon.Stats?.Sum(s => s.BaseStat) ?? 0;

            // Leggendari tipicamente hanno ID sopra 150 e stats molto alte
            if (pokemon.Id > 150 && totalStats > 500)
            {
                return "Ultra Rare";
            }

            // Pokémon con stats molto alte sono probabilmente rari
            if (totalStats > 450)
            {
                return "Holo Rare";
            }

            // Pokémon della prima generazione (1-151) sono spesso più comuni nelle carte base
            if (pokemon.Id <= 151)
            {
                return "Common";
            }

            // Default per Pokémon intermedi
            if (totalStats > 350)
            {
                return "Rare";
            }

            return "Uncommon";
        }

        private string CapitalizeFirst(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            
            return char.ToUpper(text[0]) + text.Substring(1).ToLower();
        }
    }
}