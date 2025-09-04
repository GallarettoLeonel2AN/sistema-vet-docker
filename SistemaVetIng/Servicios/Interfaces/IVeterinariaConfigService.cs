using SistemaVetIng.Models;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IVeterinariaConfigService
    {
       Task<ConfiguracionVeterinaria> Agregar(ConfiguracionVeterinaria model);
       Task<IEnumerable<ConfiguracionVeterinaria>> ListarTodo();
    }
}
