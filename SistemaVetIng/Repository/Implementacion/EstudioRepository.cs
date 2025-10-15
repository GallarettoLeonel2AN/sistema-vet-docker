using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;

namespace SistemaVetIng.Repository.Implementacion
{
    public class EstudioRepository : IEstudioRepository
    {
        private readonly ApplicationDbContext _context;

        public EstudioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Estudio>> ListarTodoAsync()
        {
            return await _context.Estudios.ToListAsync();
        }

        public async Task<Estudio> ObtenerPorIdAsync(int id)
        {
            return await _context.Estudios.FindAsync(id);
        }
    }
}
