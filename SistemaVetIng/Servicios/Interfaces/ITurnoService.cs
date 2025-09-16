using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Servicios.Interfaces
{
    public interface ITurnoService
    {
        Task<List<string>> GetHorariosDisponiblesAsync(DateTime fecha);
        Task ReservarTurnoAsync(ReservaTurnoViewModel model);
    }
}
