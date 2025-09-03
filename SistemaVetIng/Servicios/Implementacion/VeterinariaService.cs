using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Implementacion;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class VeterinariaService : IVeterinariaService
    {

        private readonly IGeneralRepository<ConfiguracionVeterinaria> _veterinariaRepository;

        public VeterinariaService(IGeneralRepository<ConfiguracionVeterinaria> veterinariaRepository)
        {
            _veterinariaRepository = veterinariaRepository;
        }
        public async Task<ConfiguracionVeterinaria> Agregar(ConfiguracionVeterinaria model)
        {
          

            try
            {
                // Buscamos la unica configuración existente
                var configuracionExistente = await _veterinariaRepository.ObtenerPorId(model.Id);

                if (configuracionExistente == null)
                {
                    // Si no existe, creamos una nueva configuracion
                   await _veterinariaRepository.Agregar(model);
                   
                }
                else
                {
                    // Si ya existe, actualizamos
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

                    _veterinariaRepository.Modificar(configuracionExistente);
                    
                }

                await _veterinariaRepository.Guardar();

            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo guardar la configuracion.");
            }
            return model;           
           
        }
    }
}
