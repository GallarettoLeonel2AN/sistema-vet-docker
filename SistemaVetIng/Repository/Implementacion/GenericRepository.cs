using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Repository.Interfaces;

public class GenericRepository<T> : IGeneralRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<IEnumerable<T>> ListarTodo()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T> ObtenerPorId(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task Agregar(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public void Modificar(T entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Eliminar(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task Guardar()
    {
        await _context.SaveChangesAsync();
    }
}