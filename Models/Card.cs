using System;
using System.ComponentModel.DataAnnotations;

namespace PokemonCardManager.Models
{
    public class Card
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Set { get; set; }
        
        [Required]
        public string Number { get; set; }
        
        public string Rarity { get; set; }
        
        public string Language { get; set; }
        
        public string Condition { get; set; }
        
        public decimal PurchasePrice { get; set; }
        
        public DateTime? PurchaseDate { get; set; }
        
        public string Source { get; set; }
        
        public decimal CurrentPrice { get; set; }
        
        public int Quantity { get; set; } = 1;
        
        // Proprietà calcolate
        public decimal TotalValue => CurrentPrice * Quantity;
        
        public decimal EstimatedProfit => TotalValue - (PurchasePrice * Quantity);
        
        public decimal ROI => PurchasePrice > 0 ? (CurrentPrice - PurchasePrice) / PurchasePrice * 100 : 0;
    }
}