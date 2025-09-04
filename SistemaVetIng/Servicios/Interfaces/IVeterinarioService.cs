using SistemaVetIng.Models;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IVeterinarioService
    {
        Task<Veterinario> Registrar(VeterinarioRegistroViewModel viewModel);
        Task<Veterinario> Modificar(VeterinarioEditarViewModel viewModel);
        Task Eliminar(int id);
        Task<Veterinario> ObtenerPorId(int id);
        Task<IEnumerable<Veterinario>> ListarTodo();
        Task<IEnumerable<Veterinario>> FiltrarPorBusqueda(string busqueda);
    }
}
