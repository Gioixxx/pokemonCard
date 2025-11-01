using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PokemonCardManager.Models
{
    public class Sale
    {
        [Key]
        public int Id { get; set; }
        
        public int CardId { get; set; }
        
        [ForeignKey("CardId")]
        public Card Card { get; set; }
        
        public DateTime SaleDate { get; set; } = DateTime.Now;
        
        public decimal SalePrice { get; set; }
        
        public decimal Fee { get; set; }
        
        public decimal ShippingCost { get; set; }
        
        public int Quantity { get; set; } = 1;
        
        // Proprietà calcolate
        public decimal NetProfit => SalePrice - Fee - ShippingCost - (Card?.PurchasePrice * Quantity ?? 0);
    }
}