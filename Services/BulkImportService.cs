using Microsoft.EntityFrameworkCore;
using PokemonCardManager.Data;
using PokemonCardManager.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonCardManager.Services
{
    public interface IBulkImportService
    {
        Task<BulkImportResult> ImportCardsFromCSVAsync(string filePath);
    }

    public class BulkImportResult
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool IsSuccess => ErrorCount == 0;
    }

    public class BulkImportService : IBulkImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public BulkImportService(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<BulkImportResult> ImportCardsFromCSVAsync(string filePath)
        {
            var result = new BulkImportResult();

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                result.Errors.Add("File non trovato o percorso non valido");
                result.ErrorCount = 1;
                return result;
            }

            try
            {
                _logger.LogInformation($"Starting bulk import from CSV: {filePath}");

                var lines = await File.ReadAllLinesAsync(filePath);
                if (lines.Length < 2)
                {
                    result.Errors.Add("File CSV vuoto o contiene solo intestazioni");
                    result.ErrorCount = 1;
                    return result;
                }

                // Skip header row
                var dataLines = lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
                result.TotalRows = dataLines.Count;

                var cardsToAdd = new List<Card>();
                var rowNumber = 1; // Start from 1 because we skip header

                foreach (var line in dataLines)
                {
                    rowNumber++;
                    try
                    {
                        var card = ParseCardFromCSVLine(line, rowNumber);
                        if (card != null)
                        {
                            cardsToAdd.Add(card);
                        }
                        else
                        {
                            result.ErrorCount++;
                            result.Errors.Add($"Riga {rowNumber}: Dati non validi");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.ErrorCount++;
                        result.Errors.Add($"Riga {rowNumber}: {ex.Message}");
                        _logger.LogWarning($"Error parsing row {rowNumber}: {ex.Message}");
                    }
                }

                // Bulk insert
                if (cardsToAdd.Count > 0)
                {
                    await _context.Cards.AddRangeAsync(cardsToAdd);
                    await _context.SaveChangesAsync();
                    result.SuccessCount = cardsToAdd.Count;
                    _logger.LogInformation($"Successfully imported {cardsToAdd.Count} cards");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during bulk import: {ex.Message}", ex);
                result.Errors.Add($"Errore durante l'importazione: {ex.Message}");
                result.ErrorCount++;
                return result;
            }
        }

        private Card? ParseCardFromCSVLine(string line, int rowNumber)
        {
            var values = ParseCSVLine(line);
            
            if (values.Length < 11) // Minimum required fields
            {
                throw new Exception("Numero insufficiente di colonne");
            }

            try
            {
                var card = new Card
                {
                    Name = values[1]?.Trim() ?? string.Empty,
                    Set = values[2]?.Trim() ?? string.Empty,
                    Number = values[3]?.Trim() ?? string.Empty,
                    Rarity = values[4]?.Trim() ?? string.Empty,
                    Language = values[5]?.Trim() ?? string.Empty,
                    Condition = values[6]?.Trim() ?? string.Empty,
                    Source = values[9]?.Trim() ?? string.Empty
                };

                // Parse PurchasePrice
                if (decimal.TryParse(values[7]?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var purchasePrice))
                {
                    card.PurchasePrice = purchasePrice;
                }

                // Parse PurchaseDate
                if (!string.IsNullOrWhiteSpace(values[8]) && DateTime.TryParse(values[8]?.Trim(), out var purchaseDate))
                {
                    card.PurchaseDate = purchaseDate;
                }

                // Parse CurrentPrice
                if (decimal.TryParse(values[10]?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var currentPrice))
                {
                    card.CurrentPrice = currentPrice;
                }

                // Parse Quantity
                if (int.TryParse(values[11]?.Trim(), out var quantity) && quantity > 0)
                {
                    card.Quantity = quantity;
                }
                else
                {
                    card.Quantity = 1;
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(card.Name) || 
                    string.IsNullOrWhiteSpace(card.Set) || 
                    string.IsNullOrWhiteSpace(card.Number))
                {
                    throw new Exception("Campi obbligatori mancanti (Nome, Set, Numero)");
                }

                return card;
            }
            catch (Exception ex)
            {
                throw new Exception($"Errore parsing: {ex.Message}");
            }
        }

        private string[] ParseCSVLine(string line)
        {
            var values = new List<string>();
            var currentValue = new System.Text.StringBuilder();
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Escaped quote
                        currentValue.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        // Toggle quote state
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // End of field
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            // Add last field
            values.Add(currentValue.ToString());

            return values.ToArray();
        }
    }
}

