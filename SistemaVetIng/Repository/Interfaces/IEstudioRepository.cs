using SistemaVetIng.Models;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IEstudioRepository
    {
        Task<IEnumerable<Estudio>> ListarTodoAsync();
        Task<Estudio> ObtenerPorIdAsync(int id);
    }
}
