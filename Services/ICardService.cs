using PokemonCardManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonCardManager.Services
{
    public interface ICardService
    {
        Task<List<Card>> GetAllCardsAsync();
        Task<Card> GetCardByIdAsync(int id);
        Task AddCardAsync(Card card);
        Task UpdateCardAsync(Card card);
        Task DeleteCardAsync(int id);
        Task<List<Card>> SearchCardsAsync(string searchText);
        Task<int> GetTotalCardsCountAsync();
        Task<PagedResult<Card>> GetCardsPagedAsync(int pageNumber, int pageSize, string? searchText = null);
        Task<int> SearchCardsCountAsync(string searchText);
    }
}