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
        public DbSet<ConfiguracionVeterinaria> ConfiguracionVeterinarias { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<Veterinaria> Veterinarias { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed para Vacunas
            modelBuilder.Entity<Vacuna>().HasData(
                new Vacuna { Id = 1, Nombre = "Vacuna Antirrábica", Lote = "Lote-A123", Precio = 2500.00m, FechaAplicacion = DateTime.MinValue },
                new Vacuna { Id = 2, Nombre = "Vacuna Quíntuple Canina", Lote = "Lote-B456", Precio = 3200.00m, FechaAplicacion = DateTime.MinValue },
                new Vacuna { Id = 3, Nombre = "Vacuna Triple Felina", Lote = "Lote-C789", Precio = 2800.00m, FechaAplicacion = DateTime.MinValue },
                new Vacuna { Id = 4, Nombre = "Vacuna de la Tos de las Perreras", Lote = "Lote-D012", Precio = 2000.00m, FechaAplicacion = DateTime.MinValue }
            );

            // Seed para Estudios
            modelBuilder.Entity<Estudio>().HasData(
                new Estudio { Id = 1, Nombre = "Análisis de sangre completo", Precio = 4500.00m, Informe = null, },
                new Estudio { Id = 2, Nombre = "Radiografía de tórax", Precio = 6000.00m, Informe = null },
                new Estudio { Id = 3, Nombre = "Análisis de orina", Precio = 2000.00m, Informe = null },
                new Estudio { Id = 4, Nombre = "Ecografía abdominal", Precio = 7500.00m, Informe = null },
                new Estudio { Id = 5, Nombre = "Estudio parasitológico", Precio = 1800.00m, Informe = null }
            );

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
                .HasOne(t => t.Cliente)
                .WithMany(c => c.Turnos)
                .HasForeignKey(t => t.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);


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
                 .IsRequired(false)
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

            // Mascota Cliente
            modelBuilder.Entity<Mascota>()
                .HasOne(m => m.Propietario)
                .WithMany(c => c.Mascotas)
                .HasForeignKey(m => m.ClienteId);
        }


    }
}
