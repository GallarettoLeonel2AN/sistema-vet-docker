using SistemaVetIng.Models;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IVeterinariaService
    {
       Task<ConfiguracionVeterinaria> Agregar(ConfiguracionVeterinaria model);
        
    }
}
