using SistemaVetIng.Models;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IEstudioService
    {
        Task<IEnumerable<Estudio>> ListarTodoAsync();
        Task<IEnumerable<Estudio>> ObtenerPorIdsAsync(List<int> ids);
    }
}
