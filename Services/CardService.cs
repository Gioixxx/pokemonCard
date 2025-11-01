using Microsoft.EntityFrameworkCore;
using PokemonCardManager.Data;
using PokemonCardManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonCardManager.Services
{
    public class CardService : ICardService
    {
        private readonly ApplicationDbContext _context;

        public CardService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Card>> GetAllCardsAsync()
        {
            return await _context.Cards
                .AsNoTracking()
                .OrderByDescending(c => c.Id)
                .ToListAsync();
        }

        public async Task<Card> GetCardByIdAsync(int id)
        {
            return await _context.Cards
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddCardAsync(Card card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            _context.Cards.Add(card);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCardAsync(Card card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            try
            {
                // Track the entity and preserve RowVersion for optimistic concurrency
                var existingCard = await _context.Cards.FindAsync(card.Id);
                if (existingCard == null)
                    throw new InvalidOperationException("La carta non è stata trovata nel database.");

                // Check if RowVersion matches (optimistic concurrency check)
                if (card.RowVersion != null && existingCard.RowVersion != null)
                {
                    if (!existingCard.RowVersion.SequenceEqual(card.RowVersion))
                    {
                        throw new InvalidOperationException(
                            "La carta è stata modificata da un altro utente. Ricarica i dati e riprova.");
                    }
                }

                // Update properties while preserving RowVersion
                existingCard.Name = card.Name;
                existingCard.Set = card.Set;
                existingCard.Number = card.Number;
                existingCard.Rarity = card.Rarity;
                existingCard.Language = card.Language;
                existingCard.Condition = card.Condition;
                existingCard.PurchasePrice = card.PurchasePrice;
                existingCard.PurchaseDate = card.PurchaseDate;
                existingCard.Source = card.Source;
                existingCard.CurrentPrice = card.CurrentPrice;
                existingCard.Quantity = card.Quantity;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException(
                    "La carta è stata modificata da un altro utente. Ricarica i dati e riprova.", ex);
            }
        }

        public async Task DeleteCardAsync(int id)
        {
            var card = await _context.Cards.FindAsync(id);
            if (card != null)
            {
                _context.Cards.Remove(card);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Card>> SearchCardsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetAllCardsAsync();

            var lowerSearch = searchText.ToLower();
            return await _context.Cards
                .AsNoTracking()
                .Where(c => 
                    c.Name.ToLower().Contains(lowerSearch) || 
                    c.Set.ToLower().Contains(lowerSearch) || 
                    c.Number.ToLower().Contains(lowerSearch))
                .OrderByDescending(c => c.Id)
                .ToListAsync();
        }

        public async Task<int> GetTotalCardsCountAsync()
        {
            return await _context.Cards.CountAsync();
        }

        public async Task<PagedResult<Card>> GetCardsPagedAsync(int pageNumber, int pageSize, string? searchText = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 50;

            var query = _context.Cards.AsNoTracking();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var lowerSearch = searchText.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(lowerSearch) ||
                    c.Set.ToLower().Contains(lowerSearch) ||
                    c.Number.ToLower().Contains(lowerSearch));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Card>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<int> SearchCardsCountAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await GetTotalCardsCountAsync();

            var lowerSearch = searchText.ToLower();
            return await _context.Cards
                .AsNoTracking()
                .Where(c =>
                    c.Name.ToLower().Contains(lowerSearch) ||
                    c.Set.ToLower().Contains(lowerSearch) ||
                    c.Number.ToLower().Contains(lowerSearch))
                .CountAsync();
        }
    }
}