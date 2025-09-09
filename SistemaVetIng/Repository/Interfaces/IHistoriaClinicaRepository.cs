using SistemaVetIng.Models;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IHistoriaClinicaRepository
    {
        Task<List<Cliente>> GetClientesPorBusqueda(string busqueda);
        Task<Cliente> GetMascotasClientes(int clienteId);
        Task<Mascota> GetHistoriaClinicaCompletaMascota(int mascotaId);
    }
}
