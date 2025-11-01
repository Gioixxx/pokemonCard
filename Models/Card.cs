using System;
using System.ComponentModel.DataAnnotations;

namespace PokemonCardManager.Models
{
    public class Card
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Il nome della carta è obbligatorio")]
        [StringLength(200, ErrorMessage = "Il nome non può superare 200 caratteri")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Il set è obbligatorio")]
        [StringLength(100, ErrorMessage = "Il set non può superare 100 caratteri")]
        public string Set { get; set; }

        [Required(ErrorMessage = "Il numero della carta è obbligatorio")]
        [StringLength(50, ErrorMessage = "Il numero non può superare 50 caratteri")]
        public string Number { get; set; }

        [StringLength(50)]
        public string Rarity { get; set; }

        [StringLength(50)]
        public string Language { get; set; }

        [StringLength(50)]
        public string Condition { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Il prezzo di acquisto non può essere negativo")]
        public decimal PurchasePrice { get; set; }

        public DateTime? PurchaseDate { get; set; }

        [StringLength(200)]
        public string Source { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Il prezzo attuale non può essere negativo")]
        public decimal CurrentPrice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La quantità deve essere almeno 1")]
        public int Quantity { get; set; } = 1;
        
        // Proprietà calcolate
        public decimal TotalValue => CurrentPrice * Quantity;
        
        public decimal EstimatedProfit => TotalValue - (PurchasePrice * Quantity);
        
        public decimal ROI => PurchasePrice > 0 ? (CurrentPrice - PurchasePrice) / PurchasePrice * 100 : 0;
    }
}