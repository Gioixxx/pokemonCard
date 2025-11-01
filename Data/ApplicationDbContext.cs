using Microsoft.EntityFrameworkCore;
using PokemonCardManager.Models;

namespace PokemonCardManager.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Card> Cards { get; set; }
        public DbSet<Sale> Sales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurazione delle relazioni
            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Card)
                .WithMany()
                .HasForeignKey(s => s.CardId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurazione RowVersion per optimistic concurrency
            modelBuilder.Entity<Card>()
                .Property(c => c.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<Sale>()
                .Property(s => s.RowVersion)
                .IsRowVersion();
        }
    }
}