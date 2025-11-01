using PokemonCardManager.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PokemonCardManager.Services
{
    public class PokeApiService : IPokeApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://pokeapi.co/api/v2/";
        private readonly JsonSerializerOptions _jsonOptions;

        public PokeApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<PokemonDto?> GetPokemonByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            try
            {
                var response = await _httpClient.GetAsync($"pokemon/{name.ToLowerInvariant()}");
                
                if (!response.IsSuccessStatusCode)
                    return null;

                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PokemonDto>(jsonString, _jsonOptions);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<PokemonDto?> GetPokemonByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"pokemon/{id}");
                
                if (!response.IsSuccessStatusCode)
                    return null;

                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PokemonDto>(jsonString, _jsonOptions);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<PokemonSpeciesDto?> GetPokemonSpeciesByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            try
            {
                var response = await _httpClient.GetAsync($"pokemon-species/{name.ToLowerInvariant()}");
                
                if (!response.IsSuccessStatusCode)
                    return null;

                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PokemonSpeciesDto>(jsonString, _jsonOptions);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<NamedApiResourceDto>> GetAllPokemonAsync(int limit = 1000, int offset = 0)
        {
            try
            {
                var response = await _httpClient.GetAsync($"pokemon?limit={limit}&offset={offset}");
                
                if (!response.IsSuccessStatusCode)
                    return new List<NamedApiResourceDto>();

                var jsonString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResourceListDto>(jsonString, _jsonOptions);
                
                return result?.Results ?? new List<NamedApiResourceDto>();
            }
            catch (Exception)
            {
                return new List<NamedApiResourceDto>();
            }
        }

        public async Task<string?> GetPokemonImageUrlAsync(string name)
        {
            var pokemon = await GetPokemonByNameAsync(name);
            return pokemon?.Sprites?.FrontDefault;
        }
    }
}

