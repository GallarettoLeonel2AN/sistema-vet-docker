using SistemaVetIng.Models;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IHistoriaClinicaService
    {
        Task<List<Cliente>> GetClientesParaSeguimiento(string searchString);
        Task<Cliente> GetMascotasCliente(int clienteId);
        Task<Mascota> GetDetalleHistoriaClinica(int mascotaId);
    }
}
