using Microsoft.EntityFrameworkCore;
using PerrosPeligrososApi.Models; 

namespace PerrosPeligrososApi.Data
{
    public class PerrosPeligrososApiDbContext : DbContext
    {
        public PerrosPeligrososApiDbContext(DbContextOptions<PerrosPeligrososApiDbContext> options)
            : base(options)
        {
        }

       
        public DbSet<PerroPeligroso> PerrosPeligrosos { get; set; }
        public DbSet<ChipPerroPeligroso> ChipsPerroPeligroso { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PerroPeligroso>()
                .HasOne(p => p.Chip)
                .WithOne(c => c.PerroPeligroso)
                .HasForeignKey<ChipPerroPeligroso>(c => c.PerroPeligrosoId);

            base.OnModelCreating(modelBuilder);
        }
    }
}