using PokemonCardManager.Models.Api;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonCardManager.Services
{
    public interface IPokeApiService
    {
        Task<PokemonDto?> GetPokemonByNameAsync(string name);
        Task<PokemonDto?> GetPokemonByIdAsync(int id);
        Task<PokemonSpeciesDto?> GetPokemonSpeciesByNameAsync(string name);
        Task<List<NamedApiResourceDto>> GetAllPokemonAsync(int limit = 1000, int offset = 0);
        Task<string?> GetPokemonImageUrlAsync(string name);
    }
}

