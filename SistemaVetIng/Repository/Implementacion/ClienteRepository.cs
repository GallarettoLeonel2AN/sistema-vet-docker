using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;
using System.Threading.Tasks;

namespace SistemaVetIng.Repository.Implementacion
{
    public class ClienteRepository : IClienteRepository
    {

        private readonly ApplicationDbContext _context;

        public ClienteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Agregar(Cliente entity)
            => await _context.AddAsync(entity);

        public void Eliminar(Cliente entity)
            => _context.Clientes.Remove(entity);

        public Task Guardar()
            => _context.SaveChangesAsync();

        public async Task<IEnumerable<Cliente>> ListarTodo()
            => await _context.Clientes.Include(c => c.Usuario).ToListAsync();

        public void Modificar(Cliente entity)
        {
            _context.Clientes.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async Task<Cliente> ObtenerPorId(int id)
            => await _context.Clientes.Include(c => c.Usuario).FirstOrDefaultAsync(x => x.Id == id);

        public async Task<Cliente> ObtenerPorIdUsuario(int Usuario)
        {
          return await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == Usuario);

        }
    }
}
