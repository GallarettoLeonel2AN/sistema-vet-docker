using SistemaVetIng.Models;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface ITurnoService
    {
        Task<List<string>> GetHorariosDisponiblesAsync(DateTime fecha);
        Task ReservarTurnoAsync(ReservaTurnoViewModel model);
        Task<IEnumerable<Turno>> ObtenerTurnosAsync();
        Task<IEnumerable<Turno>> ObtenerTurnosPorClienteIdAsync(int clienteId);

        Task<IEnumerable<Turno>> ObtenerTurnosPorFechaAsync(DateTime fecha);
    }
}
