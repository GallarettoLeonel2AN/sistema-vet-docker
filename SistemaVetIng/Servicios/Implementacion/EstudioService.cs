using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class EstudioService : IEstudioService
    {
        private readonly IEstudioRepository _estudioRepository;

        public EstudioService(IEstudioRepository estudioRepository)
        {
            _estudioRepository = estudioRepository;
        }

        public async Task<IEnumerable<Estudio>> ListarTodoAsync()
        {
            return await _estudioRepository.ListarTodoAsync();
        }
    }
}
