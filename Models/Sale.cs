using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PokemonCardManager.Models
{
    public class Sale
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La carta è obbligatoria")]
        public int CardId { get; set; }

        [ForeignKey("CardId")]
        public Card Card { get; set; }

        [Required(ErrorMessage = "La data di vendita è obbligatoria")]
        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Il prezzo di vendita è obbligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Il prezzo di vendita deve essere maggiore di zero")]
        public decimal SalePrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "La commissione non può essere negativa")]
        public decimal Fee { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Il costo di spedizione non può essere negativo")]
        public decimal ShippingCost { get; set; }

        [Required(ErrorMessage = "La quantità è obbligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La quantità deve essere almeno 1")]
        public int Quantity { get; set; } = 1;
        
        // Proprietà calcolate
        public decimal NetProfit => SalePrice - Fee - ShippingCost - (Card?.PurchasePrice * Quantity ?? 0);
    }
}