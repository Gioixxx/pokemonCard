using Microsoft.EntityFrameworkCore;
using PokemonCardManager.Data;
using PokemonCardManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonCardManager.Services
{
    public class SaleService : ISaleService
    {
        private readonly ApplicationDbContext _context;

        public SaleService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Sale>> GetAllSalesAsync()
        {
            return await _context.Sales
                .Include(s => s.Card)
                .AsNoTracking()
                .OrderByDescending(s => s.SaleDate)
                .ThenByDescending(s => s.Id)
                .ToListAsync();
        }

        public async Task<Sale?> GetSaleByIdAsync(int id)
        {
            return await _context.Sales
                .Include(s => s.Card)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddSaleAsync(Sale sale)
        {
            if (sale == null)
                throw new ArgumentNullException(nameof(sale));

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Load the card with tracking for optimistic concurrency
                var card = await _context.Cards.FindAsync(sale.CardId);
                if (card == null)
                    throw new InvalidOperationException("La carta non è stata trovata nel database.");

                // Check if there's enough quantity
                if (card.Quantity < sale.Quantity)
                    throw new InvalidOperationException(
                        $"Quantità insufficiente. Disponibili: {card.Quantity}, richieste: {sale.Quantity}");

                // Update card quantity (RowVersion will be checked automatically by EF Core)
                card.Quantity -= sale.Quantity;
                if (card.Quantity < 0)
                    card.Quantity = 0;

                // Add the sale
                _context.Sales.Add(sale);
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException(
                    "Impossibile completare la vendita: la carta è stata modificata da un altro utente. Ricarica i dati e riprova.", ex);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateSaleAsync(Sale sale)
        {
            if (sale == null)
                throw new ArgumentNullException(nameof(sale));

            try
            {
                _context.Entry(sale).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException(
                    "La vendita è stata modificata da un altro utente. Ricarica i dati e riprova.", ex);
            }
        }

        public async Task DeleteSaleAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var sale = await _context.Sales.FindAsync(id);
                if (sale == null)
                    throw new InvalidOperationException("La vendita non è stata trovata nel database.");

                // Load the card with tracking for optimistic concurrency
                var card = await _context.Cards.FindAsync(sale.CardId);
                if (card != null)
                {
                    // Restore card quantity (RowVersion will be checked automatically by EF Core)
                    card.Quantity += sale.Quantity;
                }
                
                _context.Sales.Remove(sale);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException(
                    "Impossibile eliminare la vendita: la carta è stata modificata da un altro utente. Ricarica i dati e riprova.", ex);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Sale>> SearchSalesAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllSalesAsync();

            var lowerSearch = searchText.ToLower();
            return await _context.Sales
                .Include(s => s.Card)
                .AsNoTracking()
                .Where(s => 
                    s.Card.Name.ToLower().Contains(lowerSearch) || 
                    s.Card.Set.ToLower().Contains(lowerSearch))
                .OrderByDescending(s => s.SaleDate)
                .ThenByDescending(s => s.Id)
                .ToListAsync();
        }

        public async Task<PagedResult<Sale>> GetSalesPagedAsync(int pageNumber, int pageSize, string? searchText = null, DateTime? dateFilter = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 50;

            var query = _context.Sales
                .Include(s => s.Card)
                .AsNoTracking();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var lowerSearch = searchText.ToLower();
                query = query.Where(s =>
                    s.Card.Name.ToLower().Contains(lowerSearch) ||
                    s.Card.Set.ToLower().Contains(lowerSearch));
            }

            // Apply date filter if provided
            if (dateFilter.HasValue)
            {
                var filterDate = dateFilter.Value.Date;
                query = query.Where(s => s.SaleDate.Date == filterDate);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(s => s.SaleDate)
                .ThenByDescending(s => s.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Sale>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<int> GetTotalSalesCountAsync()
        {
            return await _context.Sales.CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Sales
                .AsNoTracking()
                .SumAsync(s => s.SalePrice);
        }

        public async Task<decimal> GetTotalProfitAsync()
        {
            return await _context.Sales
                .Include(s => s.Card)
                .AsNoTracking()
                .SumAsync(s => s.NetProfit);
        }
    }
}