using Microsoft.EntityFrameworkCore;
using PokemonCardManager.Data;
using PokemonCardManager.Models;
using System.Collections.Generic;
using System.Linq;

namespace PokemonCardManager.Services
{
    public class SaleService : ISaleService
    {
        private readonly ApplicationDbContext _context;

        public SaleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Sale> GetAllSales()
        {
            return _context.Sales.Include(s => s.Card).ToList();
        }

        public Sale GetSaleById(int id)
        {
            return _context.Sales.Include(s => s.Card).FirstOrDefault(s => s.Id == id);
        }

        public void AddSale(Sale sale)
        {
            _context.Sales.Add(sale);
            
            // Aggiorna la quantità della carta
            var card = _context.Cards.Find(sale.CardId);
            if (card != null)
            {
                card.Quantity -= sale.Quantity;
                if (card.Quantity < 0)
                    card.Quantity = 0;
            }
            
            _context.SaveChanges();
        }

        public void DeleteSale(int id)
        {
            var sale = _context.Sales.Find(id);
            if (sale != null)
            {
                // Ripristina la quantità della carta
                var card = _context.Cards.Find(sale.CardId);
                if (card != null)
                {
                    card.Quantity += sale.Quantity;
                }
                
                _context.Sales.Remove(sale);
                _context.SaveChanges();
            }
        }
    }
}