using SistemaVetIng.Models;
using SistemaVetIng.ViewsModels;
using System.Security.Claims;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IAtencionVeterinariaService
    {
        Task<AtencionVeterinaria> ObtenerPorId(int id);

        Task<List<AtencionDetalleViewModel>> ObtenerPagosPendientesPorClienteId(int clienteId);
        Task<AtencionVeterinariaViewModel> GetAtencionVeterinariaViewModel(int historiaClinicaId);
        Task<string> CreateAtencionVeterinaria(AtencionVeterinariaViewModel model, ClaimsPrincipal user);
        Task RegistrarAtencionDesdeTurnoAsync(AtencionPorTurnoViewModel model, ClaimsPrincipal user);
    }
}
