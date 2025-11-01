using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonCardManager.Services
{
    public class PokemonSetService
    {
        private readonly Dictionary<string, List<string>> _setsByGeneration;

        public PokemonSetService()
        {
            _setsByGeneration = new Dictionary<string, List<string>>
            {
                ["generation-i"] = new List<string>
                {
                    "Base Set",
                    "Jungle",
                    "Fossil",
                    "Base Set 2",
                    "Team Rocket",
                    "Gym Heroes",
                    "Gym Challenge",
                    "Wizards Black Star Promos"
                },
                ["generation-ii"] = new List<string>
                {
                    "Neo Genesis",
                    "Neo Discovery",
                    "Neo Revelation",
                    "Neo Destiny",
                    "Legendary Collection",
                    "Expedition Base Set",
                    "Aquapolis",
                    "Skyridge"
                },
                ["generation-iii"] = new List<string>
                {
                    "Ruby & Sapphire",
                    "Sandstorm",
                    "Dragon",
                    "Team Magma vs Team Aqua",
                    "Hidden Legends",
                    "FireRed & LeafGreen",
                    "Team Rocket Returns",
                    "Deoxys",
                    "Emerald",
                    "Unseen Forces",
                    "Delta Species",
                    "Legend Maker",
                    "Holon Phantoms",
                    "Crystal Guardians",
                    "Dragon Frontiers",
                    "Power Keepers"
                },
                ["generation-iv"] = new List<string>
                {
                    "Diamond & Pearl",
                    "Mysterious Treasures",
                    "Secret Wonders",
                    "Great Encounters",
                    "Majestic Dawn",
                    "Legends Awakened",
                    "Stormfront",
                    "Platinum",
                    "Rising Rivals",
                    "Supreme Victors",
                    "Arceus",
                    "HeartGold & SoulSilver",
                    "Unleashed",
                    "Undaunted",
                    "Triumphant",
                    "Call of Legends"
                },
                ["generation-v"] = new List<string>
                {
                    "Black & White",
                    "Emerging Powers",
                    "Noble Victories",
                    "Next Destinies",
                    "Dark Explorers",
                    "Dragons Exalted",
                    "Dragon Vault",
                    "Boundaries Crossed",
                    "Plasma Storm",
                    "Plasma Freeze",
                    "Plasma Blast",
                    "Legendary Treasures",
                    "XY",
                    "Flashfire",
                    "Furious Fists",
                    "Phantom Forces",
                    "Primal Clash",
                    "Roaring Skies",
                    "Ancient Origins",
                    "BREAKthrough",
                    "BREAKpoint",
                    "Fates Collide",
                    "Steam Siege",
                    "Evolutions"
                },
                ["generation-vi"] = new List<string>
                {
                    "XY",
                    "Flashfire",
                    "Furious Fists",
                    "Phantom Forces",
                    "Primal Clash",
                    "Roaring Skies",
                    "Ancient Origins",
                    "BREAKthrough",
                    "BREAKpoint",
                    "Fates Collide",
                    "Steam Siege",
                    "Evolutions",
                    "Sun & Moon",
                    "Guardians Rising",
                    "Burning Shadows",
                    "Crimson Invasion",
                    "Ultra Prism",
                    "Forbidden Light",
                    "Celestial Storm",
                    "Lost Thunder",
                    "Team Up",
                    "Detective Pikachu",
                    "Unbroken Bonds",
                    "Unified Minds",
                    "Hidden Fates",
                    "Cosmic Eclipse"
                },
                ["generation-vii"] = new List<string>
                {
                    "Sun & Moon",
                    "Guardians Rising",
                    "Burning Shadows",
                    "Crimson Invasion",
                    "Ultra Prism",
                    "Forbidden Light",
                    "Celestial Storm",
                    "Lost Thunder",
                    "Team Up",
                    "Detective Pikachu",
                    "Unbroken Bonds",
                    "Unified Minds",
                    "Hidden Fates",
                    "Cosmic Eclipse",
                    "Sword & Shield",
                    "Rebel Clash",
                    "Darkness Ablaze",
                    "Champions Path",
                    "Vivid Voltage",
                    "Shining Fates",
                    "Battle Styles",
                    "Chilling Reign",
                    "Evolving Skies",
                    "Celebrations",
                    "Fusion Strike",
                    "Brilliant Stars"
                },
                ["generation-viii"] = new List<string>
                {
                    "Sword & Shield",
                    "Rebel Clash",
                    "Darkness Ablaze",
                    "Champions Path",
                    "Vivid Voltage",
                    "Shining Fates",
                    "Battle Styles",
                    "Chilling Reign",
                    "Evolving Skies",
                    "Celebrations",
                    "Fusion Strike",
                    "Brilliant Stars",
                    "Astral Radiance",
                    "Pokémon GO",
                    "Lost Origin",
                    "Silver Tempest",
                    "Crown Zenith",
                    "Scarlet & Violet",
                    "Paldea Evolved",
                    "Obsidian Flames",
                    "151",
                    "Paradox Rift",
                    "Paldean Fates",
                    "Temporal Forces"
                },
                ["generation-ix"] = new List<string>
                {
                    "Scarlet & Violet",
                    "Paldea Evolved",
                    "Obsidian Flames",
                    "151",
                    "Paradox Rift",
                    "Paldean Fates",
                    "Temporal Forces"
                }
            };
        }

        public List<string> GetSetsForGeneration(string? generationName)
        {
            if (string.IsNullOrEmpty(generationName))
                return GetAllSets();

            // Normalizza il nome della generazione
            generationName = generationName.ToLowerInvariant();

            foreach (var kvp in _setsByGeneration)
            {
                if (generationName.Contains(kvp.Key) || 
                    generationName.Contains(kvp.Key.Replace("generation-", "")))
                {
                    return kvp.Value;
                }
            }

            return GetAllSets();
        }

        public List<string> GetSetsForPokemonId(int pokemonId)
        {
            // Stima la generazione basata sull'ID nazionale
            if (pokemonId <= 151)
                return _setsByGeneration["generation-i"];
            else if (pokemonId <= 251)
                return _setsByGeneration["generation-ii"];
            else if (pokemonId <= 386)
                return _setsByGeneration["generation-iii"];
            else if (pokemonId <= 493)
                return _setsByGeneration["generation-iv"];
            else if (pokemonId <= 649)
                return _setsByGeneration["generation-v"];
            else if (pokemonId <= 721)
                return _setsByGeneration["generation-vi"];
            else if (pokemonId <= 809)
                return _setsByGeneration["generation-vii"];
            else if (pokemonId <= 905)
                return _setsByGeneration["generation-viii"];
            else
                return _setsByGeneration["generation-ix"];
        }

        public List<string> GetAllSets()
        {
            var allSets = new HashSet<string>();
            foreach (var sets in _setsByGeneration.Values)
            {
                foreach (var set in sets)
                {
                    allSets.Add(set);
                }
            }
            return allSets.OrderBy(s => s).ToList();
        }
    }
}

