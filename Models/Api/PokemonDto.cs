using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PokemonCardManager.Models.Api
{
    public class PokemonDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("base_experience")]
        public int? BaseExperience { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("weight")]
        public int Weight { get; set; }

        [JsonPropertyName("sprites")]
        public PokemonSpritesDto? Sprites { get; set; }

        [JsonPropertyName("types")]
        public List<PokemonTypeDto> Types { get; set; } = new();

        [JsonPropertyName("stats")]
        public List<PokemonStatDto> Stats { get; set; } = new();

        [JsonPropertyName("species")]
        public NamedApiResourceDto? Species { get; set; }
    }

    public class PokemonSpritesDto
    {
        [JsonPropertyName("front_default")]
        public string? FrontDefault { get; set; }

        [JsonPropertyName("front_shiny")]
        public string? FrontShiny { get; set; }

        [JsonPropertyName("back_default")]
        public string? BackDefault { get; set; }

        [JsonPropertyName("back_shiny")]
        public string? BackShiny { get; set; }
    }

    public class PokemonTypeDto
    {
        [JsonPropertyName("slot")]
        public int Slot { get; set; }

        [JsonPropertyName("type")]
        public NamedApiResourceDto Type { get; set; } = new();
    }

    public class PokemonStatDto
    {
        [JsonPropertyName("base_stat")]
        public int BaseStat { get; set; }

        [JsonPropertyName("stat")]
        public NamedApiResourceDto Stat { get; set; } = new();
    }

    public class NamedApiResourceDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }

    public class PokemonSpeciesDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("generation")]
        public NamedApiResourceDto? Generation { get; set; }

        [JsonPropertyName("flavor_text_entries")]
        public List<FlavorTextEntryDto> FlavorTextEntries { get; set; } = new();

        [JsonPropertyName("names")]
        public List<NameDto> Names { get; set; } = new();
    }

    public class FlavorTextEntryDto
    {
        [JsonPropertyName("flavor_text")]
        public string FlavorText { get; set; } = string.Empty;

        [JsonPropertyName("language")]
        public NamedApiResourceDto Language { get; set; } = new();

        [JsonPropertyName("version")]
        public NamedApiResourceDto Version { get; set; } = new();
    }

    public class NameDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("language")]
        public NamedApiResourceDto Language { get; set; } = new();
    }

    public class ApiResourceListDto
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("previous")]
        public string? Previous { get; set; }

        [JsonPropertyName("results")]
        public List<NamedApiResourceDto> Results { get; set; } = new();
    }
}

