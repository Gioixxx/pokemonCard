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

        public DataExportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExportCardsToCSV(string filePath)
        {
            try
            {
                var cards = await _context.Cards.ToListAsync();
                
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
                            $"{card.PurchaseDate:yyyy-MM-dd}," +
                            $"\"{EscapeCsv(card.Source)}\"," +
                            $"{card.CurrentPrice}," +
                            $"{card.Quantity}," +
                            $"{card.TotalValue}," +
                            $"{card.EstimatedProfit}," +
                            $"{card.ROI}"
                        );
                    }
                }
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ExportSalesToCSV(string filePath)
        {
            try
            {
                var sales = await _context.Sales.Include(s => s.Card).ToListAsync();
                
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
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> BackupDatabase(string filePath)
        {
            try
            {
                // Ottiene il percorso del database corrente
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pokemoncards.db");
                
                // Copia il file del database
                File.Copy(dbPath, filePath, true);
                
                return true;
            }
            catch (Exception)
            {
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