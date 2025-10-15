using SistemaVetIng.Models;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IVacunaService
    {
        Task<IEnumerable<Vacuna>> ListarTodoAsync();
    }
}
