using SistemaVetIng.Models;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IAtencionVeterinariaRepository
    {
        Task<HistoriaClinica> GetHistoriaClinicaConMascotayPropietario(int historiaClinicaId);
        Task<List<Vacuna>> GetVacunas();
        Task<List<Estudio>> GetEstudios();
        Task<Veterinario> GetVeterinarioPorId(int usuarioId);
        Task<List<Vacuna>> GetVacunaSeleccionada(IEnumerable<int> ids);
        Task<List<Estudio>> GetEstudioSeleccionado(IEnumerable<int> ids);
        Task AgregarAtencionVeterinaria(AtencionVeterinaria atencion);
        Task AgregarTratamiento(Tratamiento tratamiento);
        Task SaveChangesAsync();
        Task<HistoriaClinica> GetHistoriaClinicaPorId(int id);
    }
}
