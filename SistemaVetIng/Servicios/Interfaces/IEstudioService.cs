using SistemaVetIng.Models;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IEstudioService
    {
        Task<IEnumerable<Estudio>> ListarTodoAsync();
    }
}
