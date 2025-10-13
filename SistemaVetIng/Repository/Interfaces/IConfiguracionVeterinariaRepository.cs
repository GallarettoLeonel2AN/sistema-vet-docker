using SistemaVetIng.Models;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IConfiguracionVeterinariaRepository
    {
  
        Task<ConfiguracionVeterinaria> ObtenerConfiguracionConHorariosAsync();
 
        Task AgregarAsync(ConfiguracionVeterinaria configuracion);

        void Actualizar(ConfiguracionVeterinaria configuracion);

        Task<bool> GuardarCambiosAsync();
    }
}
