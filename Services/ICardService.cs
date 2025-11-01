using PokemonCardManager.Models;
using System.Collections.Generic;

namespace PokemonCardManager.Services
{
    public interface ICardService
    {
        List<Card> GetAllCards();
        Card GetCardById(int id);
        void AddCard(Card card);
        void UpdateCard(Card card);
        void DeleteCard(int id);
    }
}