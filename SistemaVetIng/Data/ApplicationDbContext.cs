using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Models;

namespace SistemaVetIng.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario, Rol, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // DbSets de cada entidad
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Veterinario> Veterinarios { get; set; }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<Chip> Chips { get; set; }
        public DbSet<HistoriaClinica> HistoriasClinicas { get; set; }
        public DbSet<AtencionVeterinaria> AtencionesVeterinarias { get; set; }
        public DbSet<Tratamiento> Tratamientos { get; set; }
        public DbSet<Vacuna> Vacunas { get; set; }
        public DbSet<Estudio> Estudios { get; set; }
        public DbSet<Disponibilidad> Disponibilidades { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<Veterinaria> Veterinarias { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relaciones con Usuario
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Usuario)
                .WithOne()
                .HasForeignKey<Cliente>(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Veterinario>()
                .HasOne(v => v.Usuario)
                .WithOne()
                .HasForeignKey<Veterinario>(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Veterinaria>()
                .HasOne(v => v.Usuario)
                .WithOne()
                .HasForeignKey<Veterinaria>(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relaciones Turno
            modelBuilder.Entity<Turno>()
                .HasOne(t => t.Mascota)
                .WithMany()
                .HasForeignKey(t => t.MascotaId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Turno>()
                .HasOne(t => t.Veterinario)
                .WithMany(v => v.Turnos)
                .HasForeignKey(t => t.VeterinarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Turno>()
                .HasOne(t => t.Cliente)
                .WithMany(c => c.Turnos)
                .HasForeignKey(t => t.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);


            // Relacion Disponibilidad y Veterinario
            modelBuilder.Entity<Disponibilidad>()
                .HasOne(d => d.Veterinario)
                .WithMany(v => v.Disponibilidades)
                .HasForeignKey(d => d.VeterinarioId);

            // Relaciones Historia Clinica
            modelBuilder.Entity<HistoriaClinica>()
                .HasOne(h => h.Mascota)
                .WithOne(m => m.HistoriaClinica)
                .HasForeignKey<HistoriaClinica>(h => h.MascotaId);

            // Relacion Atencion Veterinaria 

            modelBuilder.Entity<AtencionVeterinaria>()
                .HasOne(a => a.HistoriaClinica)
                .WithMany(h => h.Atenciones)
                .HasForeignKey(a => a.HistoriaClinicaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AtencionVeterinaria>()
                .HasOne(a => a.Tratamiento)
                .WithOne()
                .HasForeignKey<AtencionVeterinaria>(a => a.TratamientoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vacuna>()
                .HasOne(v => v.AtencionVeterinaria)
                .WithMany(a => a.Vacunas)
                .HasForeignKey(v => v.AtencionVeterinariaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Estudio>()
                .HasOne(e => e.AtencionVeterinaria)
                .WithMany(a => a.EstudiosComplementarios)
                .HasForeignKey(e => e.AtencionVeterinariaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AtencionVeterinaria>()
                .HasOne(a => a.Veterinario)
                .WithMany()
                .HasForeignKey(a => a.VeterinarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Chip
            modelBuilder.Entity<Chip>()
                .HasOne(c => c.Mascota)
                .WithOne(m => m.Chip)
                .HasForeignKey<Chip>(c => c.MascotaId);
        }


    }
}
