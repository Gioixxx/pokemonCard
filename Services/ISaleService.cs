using PokemonCardManager.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonCardManager.Services
{
    public interface ISaleService
    {
        Task<List<Sale>> GetAllSalesAsync();
        Task<Sale?> GetSaleByIdAsync(int id);
        Task AddSaleAsync(Sale sale);
        Task UpdateSaleAsync(Sale sale);
        Task DeleteSaleAsync(int id);
        Task<List<Sale>> SearchSalesAsync(string searchText);
        Task<PagedResult<Sale>> GetSalesPagedAsync(int pageNumber, int pageSize, string? searchText = null, DateTime? dateFilter = null);
        Task<int> GetTotalSalesCountAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetTotalProfitAsync();
    }
}