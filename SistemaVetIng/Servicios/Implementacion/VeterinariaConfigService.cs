using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Implementacion;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class VeterinariaConfigService : IVeterinariaConfigService
    {

        private readonly IGeneralRepository<ConfiguracionVeterinaria> _veterinariaRepository;

        public VeterinariaConfigService(IGeneralRepository<ConfiguracionVeterinaria> veterinariaRepository)
        {
            _veterinariaRepository = veterinariaRepository;
        }

        public async Task<ConfiguracionVeterinaria> Agregar(ConfiguracionVeterinaria model)
        {
            try
            {
                var configuracionExistente = (await _veterinariaRepository.ListarTodo()).FirstOrDefault();

                if (configuracionExistente == null)
                {
                    await _veterinariaRepository.Agregar(model);
                }
                else
                {
                    // Si ya existe, actualizamos sus propiedades con los datos del model
                    configuracionExistente.HoraInicio = model.HoraInicio;
                    configuracionExistente.HoraFin = model.HoraFin;
                    configuracionExistente.DuracionMinutosPorConsulta = model.DuracionMinutosPorConsulta;
                    configuracionExistente.TrabajaLunes = model.TrabajaLunes;
                    configuracionExistente.TrabajaMartes = model.TrabajaMartes;
                    configuracionExistente.TrabajaMiercoles = model.TrabajaMiercoles;
                    configuracionExistente.TrabajaJueves = model.TrabajaJueves;
                    configuracionExistente.TrabajaViernes = model.TrabajaViernes;
                    configuracionExistente.TrabajaSabado = model.TrabajaSabado;
                    configuracionExistente.TrabajaDomingo = model.TrabajaDomingo;
                }

                await _veterinariaRepository.Guardar();
            }
            catch (Exception ex)
            {          
                throw new Exception("No se pudo guardar la configuración.");
            }

            return model;
        }

        public async Task<IEnumerable<ConfiguracionVeterinaria>> ListarTodo()
        {
            return await _veterinariaRepository.ListarTodo();
        }
    }
}
