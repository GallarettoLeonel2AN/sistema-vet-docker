using SistemaVetIng.ViewsModels;
using System.Security.Claims;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IAtencionVeterinariaService
    {
        Task<AtencionVeterinariaViewModel> GetAtencionVeterinariaViewModel(int historiaClinicaId);
        Task<string> CreateAtencionVeterinaria(AtencionVeterinariaViewModel model, ClaimsPrincipal user);
        Task RegistrarAtencionDesdeTurnoAsync(AtencionPorTurnoViewModel model, ClaimsPrincipal user);
    }
}
