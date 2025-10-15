using SistemaVetIng.Models;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IMascotaService
    {
        Task<(bool success, string message)> Registrar(MascotaRegistroViewModel viewModel);
        Task<(bool success, string message)> Modificar(MascotaEditarViewModel viewModel);
        Task<(bool success, string message)> Eliminar(int id);
        Task<Mascota> ObtenerPorId(int id);
        Task<IEnumerable<Mascota>> ListarTodo();
        Task<IEnumerable<Mascota>> FiltrarPorBusqueda(string busqueda);
        Task<IEnumerable<Mascota>> ListarMascotasPorClienteId(int clienteId);
        Task<IEnumerable<MascotaListViewModel>> ObtenerMascotasPorClienteUserNameAsync(string userName);
        Task<MascotaDetalleViewModel> ObtenerDetalleConHistorial(int mascotaId);
    }
}
