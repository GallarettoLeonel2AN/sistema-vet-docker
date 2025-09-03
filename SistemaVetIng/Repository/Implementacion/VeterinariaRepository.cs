using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;

namespace SistemaVetIng.Repository.Implementacion
{
    public class VeterinariaRepository : IGeneralRepository<ConfiguracionVeterinaria>
    {
        private readonly ApplicationDbContext _contextoConfiguracion;
        public VeterinariaRepository(ApplicationDbContext contextoConfiguracion)
        {
            _contextoConfiguracion = contextoConfiguracion;
        }

        public async Task Agregar(ConfiguracionVeterinaria entity)
            => await _contextoConfiguracion.AddAsync(entity);

        public void Modificar(ConfiguracionVeterinaria entity)
        {
            _contextoConfiguracion.ConfiguracionVeterinarias.Attach(entity);
            _contextoConfiguracion.Entry(entity).State = EntityState.Modified;
        }

        public async Task<ConfiguracionVeterinaria> ObtenerPorId(int id)
            => await _contextoConfiguracion.ConfiguracionVeterinarias.FirstOrDefaultAsync(m => m.Id == id);
        public async Task Guardar()
             => await _contextoConfiguracion.SaveChangesAsync();

        public Task<IEnumerable<ConfiguracionVeterinaria>> ListarTodo()
        {
            throw new NotImplementedException();
        }

        public void Eliminar(ConfiguracionVeterinaria entity)
        {
            throw new NotImplementedException();
        }
    }
}
