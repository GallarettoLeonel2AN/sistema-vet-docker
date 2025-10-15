using SistemaVetIng.Models;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IClienteService
    {
        Task<Cliente> Registrar(ClienteRegistroViewModel viewModel);
        Task<Cliente> Modificar(ClienteEditarViewModel viewModel);
        Task Eliminar(int id);
        Task<Cliente> ObtenerPorId(int id);
        Task<IEnumerable<Cliente>> ListarTodo();
        Task<IEnumerable<Cliente>> FiltrarPorBusqueda(string busqueda);
        Task<Cliente> ObtenerClientePorUserNameAsync(string userName);
        Task<Cliente> ObtenerPorIdUsuario(int id);
    }
}
