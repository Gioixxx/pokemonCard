using Microsoft.EntityFrameworkCore;
using PokemonCardManager.Data;
using PokemonCardManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonCardManager.Services
{
    public class DataExportService : IDataExportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public DataExportService(ApplicationDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> ExportCardsToCSV(string filePath)
        {
            try
            {
                _logger.LogInformation($"Starting CSV export of cards to: {filePath}");

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    _logger.LogError("Export failed: file path is empty");
                    return false;
                }

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogDebug($"Created directory: {directory}");
                }

                var cards = await _context.Cards.ToListAsync();
                _logger.LogDebug($"Fetched {cards.Count} cards from database");

                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Intestazioni
                    await writer.WriteLineAsync("Id,Nome,Set,Numero,Rarità,Lingua,Condizione,PrezzoAcquisto,DataAcquisto,Fonte,PrezzoAttuale,Quantità,ValoreTotale,ProfittoStimato,ROI");

                    // Dati
                    foreach (var card in cards)
                    {
                        await writer.WriteLineAsync(
                            $"{card.Id}," +
                            $"\"{EscapeCsv(card.Name)}\"," +
                            $"\"{EscapeCsv(card.Set)}\"," +
                            $"\"{EscapeCsv(card.Number)}\"," +
                            $"\"{EscapeCsv(card.Rarity)}\"," +
                            $"\"{EscapeCsv(card.Language)}\"," +
                            $"\"{EscapeCsv(card.Condition)}\"," +
                            $"{card.PurchasePrice}," +
                            $"{(card.PurchaseDate.HasValue ? card.PurchaseDate.Value.ToString("yyyy-MM-dd") : "")}," +
                            $"\"{EscapeCsv(card.Source)}\"," +
                            $"{card.CurrentPrice}," +
                            $"{card.Quantity}," +
                            $"{card.TotalValue}," +
                            $"{card.EstimatedProfit}," +
                            $"{card.ROI}"
                        );
                    }
                }

                _logger.LogInformation($"Successfully exported {cards.Count} cards to CSV: {filePath}");
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError($"Access denied when exporting cards to CSV: {ex.Message}", ex);
                return false;
            }
            catch (IOException ex)
            {
                _logger.LogError($"IO error during CSV export: {ex.Message}", ex);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error during CSV export: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> ExportSalesToCSV(string filePath)
        {
            try
            {
                _logger.LogInformation($"Starting CSV export of sales to: {filePath}");

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    _logger.LogError("Export failed: file path is empty");
                    return false;
                }

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogDebug($"Created directory: {directory}");
                }

                var sales = await _context.Sales.Include(s => s.Card).ToListAsync();
                _logger.LogDebug($"Fetched {sales.Count} sales from database");

                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Intestazioni
                    await writer.WriteLineAsync("Id,CardId,NomeCarta,Set,DataVendita,PrezzoVendita,Fee,CostoSpedizione,Quantità,ProfittoNetto");

                    // Dati
                    foreach (var sale in sales)
                    {
                        await writer.WriteLineAsync(
                            $"{sale.Id}," +
                            $"{sale.CardId}," +
                            $"\"{EscapeCsv(sale.Card?.Name)}\"," +
                            $"\"{EscapeCsv(sale.Card?.Set)}\"," +
                            $"{sale.SaleDate:yyyy-MM-dd}," +
                            $"{sale.SalePrice}," +
                            $"{sale.Fee}," +
                            $"{sale.ShippingCost}," +
                            $"{sale.Quantity}," +
                            $"{sale.NetProfit}"
                        );
                    }
                }

                _logger.LogInformation($"Successfully exported {sales.Count} sales to CSV: {filePath}");
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError($"Access denied when exporting sales to CSV: {ex.Message}", ex);
                return false;
            }
            catch (IOException ex)
            {
                _logger.LogError($"IO error during CSV sales export: {ex.Message}", ex);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error during CSV sales export: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> BackupDatabase(string filePath)
        {
            try
            {
                _logger.LogInformation($"Starting database backup to: {filePath}");

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    _logger.LogError("Backup failed: file path is empty");
                    return false;
                }

                // Get correct database path (same as in App.xaml.cs)
                string dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PokemonCardManager",
                    "pokemoncards.db");

                if (!File.Exists(dbPath))
                {
                    _logger.LogError($"Database file not found at: {dbPath}");
                    return false;
                }

                // Ensure backup directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogDebug($"Created backup directory: {directory}");
                }

                // Close any active database connections before backup
                await _context.Database.CloseConnectionAsync();

                // Copy database file
                File.Copy(dbPath, filePath, true);

                _logger.LogInformation($"Successfully backed up database from {dbPath} to {filePath}");
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError($"Access denied during database backup: {ex.Message}", ex);
                return false;
            }
            catch (IOException ex)
            {
                _logger.LogError($"IO error during database backup: {ex.Message}", ex);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error during database backup: {ex.Message}", ex);
                return false;
            }
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
                
            // Sostituisce le virgolette con doppie virgolette per l'escape in CSV
            return value.Replace("\"", "\"\"");
        }
    }
}