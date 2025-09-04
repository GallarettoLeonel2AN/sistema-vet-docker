using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;

namespace SistemaVetIng.Repository.Implementacion
{
    public class VeterinariaConfiguracionRepository : IGeneralRepository<ConfiguracionVeterinaria>
    {
        private readonly ApplicationDbContext _contextoConfiguracion;
        public VeterinariaConfiguracionRepository(ApplicationDbContext contextoConfiguracion)
        {
            _contextoConfiguracion = contextoConfiguracion;
        }

        public async Task Agregar(ConfiguracionVeterinaria entity)
            => await _contextoConfiguracion.AddAsync(entity);

        public void Modificar(ConfiguracionVeterinaria entity)
        {
    
        }
        public async Task<ConfiguracionVeterinaria> ObtenerPorId(int id)
            => await _contextoConfiguracion.ConfiguracionVeterinarias.FirstOrDefaultAsync(c => c.Id == id);
        public async Task Guardar()
             => await _contextoConfiguracion.SaveChangesAsync();

        public async Task<IEnumerable<ConfiguracionVeterinaria>> ListarTodo()
        {
            return await _contextoConfiguracion.ConfiguracionVeterinarias.ToListAsync();
        }

        public void Eliminar(ConfiguracionVeterinaria entity)
        {
          
        }

    }
}
