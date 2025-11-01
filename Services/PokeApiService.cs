using PokemonCardManager.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PokemonCardManager.Services
{
    public class PokeApiService : IPokeApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly ICacheService _cacheService;
        private readonly IRateLimiter _rateLimiter;
        private const string BaseUrl = "https://pokeapi.co/api/v2/";
        private readonly JsonSerializerOptions _jsonOptions;
        private const string CachePrefix = "pokeapi_";

        public PokeApiService(HttpClient httpClient, ILogger logger, ICacheService cacheService, IRateLimiter rateLimiter)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
            
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
            {
                _logger.LogWarning("GetPokemonByNameAsync called with empty name");
                return null;
            }

            var cacheKey = $"{CachePrefix}pokemon_name_{name.ToLowerInvariant()}";
            
            // Check cache first
            var cached = await _cacheService.GetAsync<PokemonDto>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            try
            {
                // Apply rate limiting
                await _rateLimiter.WaitForSlotAsync("pokeapi", CancellationToken.None);

                _logger.LogDebug($"Fetching Pokemon data for: {name}");
                var response = await _httpClient.GetAsync($"pokemon/{name.ToLowerInvariant()}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"PokeAPI returned status {response.StatusCode} for Pokemon: {name}");
                    return null;
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var pokemon = JsonSerializer.Deserialize<PokemonDto>(jsonString, _jsonOptions);
                
                // Cache the result
                if (pokemon != null)
                {
                    await _cacheService.SetAsync(cacheKey, pokemon, TimeSpan.FromHours(24));
                }
                
                _logger.LogInformation($"Successfully fetched Pokemon data for: {name}");
                return pokemon;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Network error while fetching Pokemon '{name}': {ex.Message}", ex);
                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError($"Request timeout while fetching Pokemon '{name}': {ex.Message}", ex);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Failed to parse JSON response for Pokemon '{name}': {ex.Message}", ex);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching Pokemon '{name}': {ex.Message}", ex);
                return null;
            }
        }

        public async Task<PokemonDto?> GetPokemonByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"GetPokemonByIdAsync called with invalid ID: {id}");
                return null;
            }

            var cacheKey = $"{CachePrefix}pokemon_id_{id}";
            
            // Check cache first
            var cached = await _cacheService.GetAsync<PokemonDto>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            try
            {
                // Apply rate limiting
                await _rateLimiter.WaitForSlotAsync("pokeapi", CancellationToken.None);

                _logger.LogDebug($"Fetching Pokemon data for ID: {id}");
                var response = await _httpClient.GetAsync($"pokemon/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"PokeAPI returned status {response.StatusCode} for Pokemon ID: {id}");
                    return null;
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var pokemon = JsonSerializer.Deserialize<PokemonDto>(jsonString, _jsonOptions);
                
                // Cache the result
                if (pokemon != null)
                {
                    await _cacheService.SetAsync(cacheKey, pokemon, TimeSpan.FromHours(24));
                }
                
                _logger.LogInformation($"Successfully fetched Pokemon data for ID: {id}");
                return pokemon;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Network error while fetching Pokemon ID '{id}': {ex.Message}", ex);
                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError($"Request timeout while fetching Pokemon ID '{id}': {ex.Message}", ex);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Failed to parse JSON response for Pokemon ID '{id}': {ex.Message}", ex);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching Pokemon ID '{id}': {ex.Message}", ex);
                return null;
            }
        }

        public async Task<PokemonSpeciesDto?> GetPokemonSpeciesByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("GetPokemonSpeciesByNameAsync called with empty name");
                return null;
            }

            var cacheKey = $"{CachePrefix}species_{name.ToLowerInvariant()}";
            
            // Check cache first
            var cached = await _cacheService.GetAsync<PokemonSpeciesDto>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            try
            {
                // Apply rate limiting
                await _rateLimiter.WaitForSlotAsync("pokeapi", CancellationToken.None);

                _logger.LogDebug($"Fetching Pokemon species data for: {name}");
                var response = await _httpClient.GetAsync($"pokemon-species/{name.ToLowerInvariant()}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"PokeAPI returned status {response.StatusCode} for species: {name}");
                    return null;
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var species = JsonSerializer.Deserialize<PokemonSpeciesDto>(jsonString, _jsonOptions);
                
                // Cache the result
                if (species != null)
                {
                    await _cacheService.SetAsync(cacheKey, species, TimeSpan.FromHours(24));
                }
                
                _logger.LogInformation($"Successfully fetched species data for: {name}");
                return species;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Network error while fetching species '{name}': {ex.Message}", ex);
                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError($"Request timeout while fetching species '{name}': {ex.Message}", ex);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Failed to parse JSON response for species '{name}': {ex.Message}", ex);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching species '{name}': {ex.Message}", ex);
                return null;
            }
        }

        public async Task<List<NamedApiResourceDto>> GetAllPokemonAsync(int limit = 1000, int offset = 0)
        {
            var cacheKey = $"{CachePrefix}pokemon_list_{limit}_{offset}";
            
            // Check cache first
            var cached = await _cacheService.GetAsync<List<NamedApiResourceDto>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            try
            {
                // Apply rate limiting
                await _rateLimiter.WaitForSlotAsync("pokeapi", CancellationToken.None);

                _logger.LogDebug($"Fetching Pokemon list with limit={limit}, offset={offset}");
                var response = await _httpClient.GetAsync($"pokemon?limit={limit}&offset={offset}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"PokeAPI returned status {response.StatusCode} for Pokemon list");
                    return new List<NamedApiResourceDto>();
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResourceListDto>(jsonString, _jsonOptions);
                var pokemonList = result?.Results ?? new List<NamedApiResourceDto>();
                
                // Cache the result (shorter expiration for lists as they can change)
                if (pokemonList.Count > 0)
                {
                    await _cacheService.SetAsync(cacheKey, pokemonList, TimeSpan.FromHours(6));
                }
                
                _logger.LogInformation($"Successfully fetched {pokemonList.Count} Pokemon from list");
                return pokemonList;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Network error while fetching Pokemon list: {ex.Message}", ex);
                return new List<NamedApiResourceDto>();
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError($"Request timeout while fetching Pokemon list: {ex.Message}", ex);
                return new List<NamedApiResourceDto>();
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Failed to parse JSON response for Pokemon list: {ex.Message}", ex);
                return new List<NamedApiResourceDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching Pokemon list: {ex.Message}", ex);
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

