using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data; 
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;

public class ConfiguracionVeterinariaRepository : IConfiguracionVeterinariaRepository
{
    private readonly ApplicationDbContext _context;

    public ConfiguracionVeterinariaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ConfiguracionVeterinaria> ObtenerConfiguracionConHorariosAsync()
    {
        return await _context.ConfiguracionVeterinarias
                             .Include(c => c.HorariosPorDia)
                             .FirstOrDefaultAsync();
    }

    public async Task AgregarAsync(ConfiguracionVeterinaria configuracion)
    {
        await _context.ConfiguracionVeterinarias.AddAsync(configuracion);
    }

    public void Actualizar(ConfiguracionVeterinaria configuracion)
    {
        _context.Entry(configuracion).State = EntityState.Modified;
    }

    public async Task<bool> GuardarCambiosAsync()
    {
        return (await _context.SaveChangesAsync() > 0);
    }
}