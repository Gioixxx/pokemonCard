using PokemonCardManager.Models;
using System.Collections.Generic;

namespace PokemonCardManager.Services
{
    public interface ISaleService
    {
        List<Sale> GetAllSales();
        Sale GetSaleById(int id);
        void AddSale(Sale sale);
        void DeleteSale(int id);
    }
}