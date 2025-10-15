using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class VacunaService : IVacunaService
    {
        private readonly IVacunaRepository _vacunaRepository;

        public VacunaService(IVacunaRepository vacunaRepository)
        {
            _vacunaRepository = vacunaRepository;
        }

        public async Task<IEnumerable<Vacuna>> ListarTodoAsync()
        {
            return await _vacunaRepository.ListarTodoAsync();
        }
    }
}
