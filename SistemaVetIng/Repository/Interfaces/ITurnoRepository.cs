using SistemaVetIng.Models;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface ITurnoRepository
    {
        Task<IEnumerable<Turno>> GetTurnosByFecha(DateTime fecha);
        Task AgregarTurno(Turno turno);
        Task Guardar();
        Task<IEnumerable<Turno>> ListarTodo();
        Task<IEnumerable<Turno>> ObtenerTurnosPorClienteIdAsync(int clienteId);
    }
}
