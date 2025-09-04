using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;

public class VeterinarioRepository : IGeneralRepository<Veterinario>
{
    private readonly ApplicationDbContext _context;

    public VeterinarioRepository(ApplicationDbContext contexto)
    {
        _context = contexto;
    }

    public async Task<IEnumerable<Veterinario>> ListarTodo()
        => await _context.Veterinarios.Include(v => v.Usuario).ToListAsync();


    public async Task<Veterinario> ObtenerPorId(int id)
        => await _context.Veterinarios.Include(v => v.Usuario) .FirstOrDefaultAsync(v => v.Id == id);


    public async Task Agregar(Veterinario entity)
        => await _context.Veterinarios.AddAsync(entity);

    public void Modificar(Veterinario entity)
    {
        _context.Veterinarios.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Eliminar(Veterinario entity)
        => _context.Veterinarios.Remove(entity);

    public async Task Guardar()
        => await _context.SaveChangesAsync();
}