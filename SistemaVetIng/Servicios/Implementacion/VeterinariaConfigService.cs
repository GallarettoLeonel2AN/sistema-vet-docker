using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class VeterinariaConfigService : IVeterinariaConfigService
    {
        private readonly IConfiguracionVeterinariaRepository _configRepository;

        public VeterinariaConfigService(IConfiguracionVeterinariaRepository configRepository)
        {
            _configRepository = configRepository;
        }

        public async Task<ConfiguracionVeterinaria> ObtenerConfiguracionAsync()
        {
            return await _configRepository.ObtenerConfiguracionConHorariosAsync();
        }

        public async Task<ConfiguracionVeterinaria> Guardar(ConfiguracionVeterinaria model)
        {
            try
            {
                var configExistente = await _configRepository.ObtenerConfiguracionConHorariosAsync();

                if (configExistente == null)
                {
                    await _configRepository.AgregarAsync(model);
                }
                else
                {
                    // Si ya existe, actualizamos sus propiedades.
                    configExistente.DuracionMinutosPorConsulta = model.DuracionMinutosPorConsulta;

                    // Actualizamos los horarios uno por uno.
                    foreach (var horarioNuevo in model.HorariosPorDia)
                    {
                        var horarioExistente = configExistente.HorariosPorDia
                            .FirstOrDefault(h => h.DiaSemana == horarioNuevo.DiaSemana);

                        if (horarioExistente != null)
                        {
                            horarioExistente.EstaActivo = horarioNuevo.EstaActivo;
                            horarioExistente.HoraInicio = horarioNuevo.HoraInicio;
                            horarioExistente.HoraFin = horarioNuevo.HoraFin;
                        }
                    }

                    _configRepository.Actualizar(configExistente);
                }

                await _configRepository.GuardarCambiosAsync();
                return model;
            }
            catch (Exception ex)
            {

                throw new Exception("No se pudo guardar la configuración.", ex);
            }
        }
    }
}
