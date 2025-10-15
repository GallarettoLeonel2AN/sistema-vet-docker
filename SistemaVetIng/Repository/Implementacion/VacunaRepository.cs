using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;

namespace SistemaVetIng.Repository.Implementacion
{
    public class VacunaRepository : IVacunaRepository
    {
        private readonly ApplicationDbContext _context;

        public VacunaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vacuna>> ListarTodoAsync()
        {
            return await _context.Vacunas.ToListAsync();
        }

        public async Task<Vacuna> ObtenerPorIdAsync(int id)
        {
            return await _context.Vacunas.FindAsync(id);
        }
    }
}
