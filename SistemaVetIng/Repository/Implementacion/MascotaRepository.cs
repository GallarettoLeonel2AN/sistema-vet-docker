using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;

namespace SistemaVetIng.Repository.Implementacion
{
    public class MascotaRepository : IGeneralRepository<Mascota>
    {
        private readonly ApplicationDbContext _context;

        public MascotaRepository(ApplicationDbContext contexto)
        {
            _context = contexto;
        }
        
        public async Task Agregar(Mascota entity)
            => await _context.Mascotas.AddAsync(entity);

       public async Task Guardar()
            => await _context.SaveChangesAsync();

        public async Task<IEnumerable<Mascota>> ListarTodo()
            =>  await _context.Mascotas.Include(m => m.Propietario).ToListAsync();

        public void Modificar(Mascota entity)
        {
            _context.Mascotas.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async Task<Mascota> ObtenerPorId(int id)
            =>  await _context.Mascotas.FirstOrDefaultAsync(m => m.Id == id);

        public void Eliminar(Mascota entity)
            => _context.Mascotas.Remove(entity);

        public async Task<Mascota> ObtenerMascotaChipPorId(int id)
        {
            // Usa Include para cargar la entidad relacionada Chip.
            return await _context.Mascotas.Include(m => m.Chip).FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
