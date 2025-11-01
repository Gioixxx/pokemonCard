using Microsoft.EntityFrameworkCore;
using PokemonCardManager.Data;
using PokemonCardManager.Models;
using System.Collections.Generic;
using System.Linq;

namespace PokemonCardManager.Services
{
    public class CardService : ICardService
    {
        private readonly ApplicationDbContext _context;

        public CardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Card> GetAllCards()
        {
            return _context.Cards.ToList();
        }

        public Card GetCardById(int id)
        {
            return _context.Cards.Find(id);
        }

        public void AddCard(Card card)
        {
            _context.Cards.Add(card);
            _context.SaveChanges();
        }

        public void UpdateCard(Card card)
        {
            _context.Entry(card).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void DeleteCard(int id)
        {
            var card = _context.Cards.Find(id);
            if (card != null)
            {
                _context.Cards.Remove(card);
                _context.SaveChanges();
            }
        }
    }
}