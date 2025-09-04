using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IVeterinariaService
    {
       Task<VeterinariaPaginaPrincipalViewModel> PaginaPrincipalAsync(
       string busquedaVeterinario = null,
       string busquedaCliente = null,
       string busquedaMascota = null);
    }
}
