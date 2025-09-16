using SistemaVetIng.Models;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IMascotaRepository
    {
        Task Agregar(Mascota entity);
        Task Guardar();
        Task<IEnumerable<Mascota>> ListarTodo();
        void Modificar(Mascota entity);
        Task<Mascota> ObtenerPorId(int id);
        void Eliminar(Mascota entity);
        Task<Mascota> ObtenerMascotaChipPorId(int id);
        Task<IEnumerable<Mascota>> ListarMascotasPorClienteId(int clienteId);
    }
}
