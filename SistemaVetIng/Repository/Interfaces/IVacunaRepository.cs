using SistemaVetIng.Models;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IVacunaRepository
    {
        Task<IEnumerable<Vacuna>> ListarTodoAsync();
        Task<Vacuna> ObtenerPorIdAsync(int id);
    }
}
