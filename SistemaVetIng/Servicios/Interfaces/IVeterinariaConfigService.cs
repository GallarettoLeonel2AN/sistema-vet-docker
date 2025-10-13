using SistemaVetIng.Models;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IVeterinariaConfigService
    {
        Task<ConfiguracionVeterinaria> Guardar(ConfiguracionVeterinaria model);
        Task<ConfiguracionVeterinaria> ObtenerConfiguracionAsync();

    }
}
