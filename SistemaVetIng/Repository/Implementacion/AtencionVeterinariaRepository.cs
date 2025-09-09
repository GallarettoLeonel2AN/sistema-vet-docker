using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;

namespace SistemaVetIng.Repository.Implementacion
{
    public class AtencionVeterinariaRepository : IAtencionVeterinariaRepository
    {
        private readonly ApplicationDbContext _context;

        public AtencionVeterinariaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HistoriaClinica> GetHistoriaClinicaConMascotayPropietario(int historiaClinicaId)
        {
            return await _context.HistoriasClinicas
                                 .Include(hc => hc.Mascota)
                                     .ThenInclude(m => m.Propietario)
                                 .FirstOrDefaultAsync(hc => hc.Id == historiaClinicaId);
        }

        public async Task<List<Vacuna>> GetVacunas()
        {
            return await _context.Vacunas.ToListAsync();
        }

        public async Task<List<Estudio>> GetEstudios()
        {
            return await _context.Estudios.ToListAsync();
        }

        public async Task<Veterinario> GetVeterinarioPorId(int usuarioId)
        {
            return await _context.Veterinarios
                                 .FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);
        }

        public async Task<List<Vacuna>> GetVacunaSeleccionada(IEnumerable<int> ids)
        {
            return await _context.Vacunas
                                 .Where(v => ids.Contains(v.Id))
                                 .ToListAsync();
        }

        public async Task<List<Estudio>> GetEstudioSeleccionado(IEnumerable<int> ids)
        {
            return await _context.Estudios
                                 .Where(e => ids.Contains(e.Id))
                                 .ToListAsync();
        }

        public async Task AgregarAtencionVeterinaria(AtencionVeterinaria atencion)
        {
            await _context.AtencionesVeterinarias.AddAsync(atencion);
        }

        public async Task AgregarTratamiento(Tratamiento tratamiento)
        {
            await _context.Tratamientos.AddAsync(tratamiento);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<HistoriaClinica> GetHistoriaClinicaPorId(int id)
        {
            return await _context.HistoriasClinicas.FindAsync(id);
        }
    }
}
