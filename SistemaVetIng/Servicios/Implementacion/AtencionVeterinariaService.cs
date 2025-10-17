using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Implementacion;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Security.Claims;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class AtencionVeterinariaService : IAtencionVeterinariaService
    {
        private readonly IAtencionVeterinariaRepository _repository;
        private readonly ApplicationDbContext _context;
        private readonly IVeterinarioService _veterinarioService;
        private readonly IVacunaService _vacunaService;
        private readonly IEstudioService _estudioService;
        private readonly ITurnoService _turnoService;

        public AtencionVeterinariaService(
            IAtencionVeterinariaRepository repository,
            ApplicationDbContext context,
            IVeterinarioService veterinarioService,
            IVacunaService vacunaService,
            IEstudioService estudioService,
            ITurnoService turnoService)
        {
            _repository = repository;
            _context = context;
            _veterinarioService = veterinarioService;
            _vacunaService = vacunaService;
            _estudioService = estudioService;
            _turnoService = turnoService;
        }

        public async Task<AtencionVeterinariaViewModel> GetAtencionVeterinariaViewModel(int historiaClinicaId)
        {
            var historiaClinica = await _repository.GetHistoriaClinicaConMascotayPropietario(historiaClinicaId);

            if (historiaClinica == null)
            {
                return null;
            }

            var viewModel = new AtencionVeterinariaViewModel
            {
                HistoriaClinicaId = historiaClinicaId
            };

            // Pasar datos para la vista 
            viewModel.MascotaNombre = historiaClinica.Mascota.Nombre;
            viewModel.PropietarioNombre = $"{historiaClinica.Mascota.Propietario?.Nombre} {historiaClinica.Mascota.Propietario?.Apellido}";
            viewModel.MascotaId = historiaClinica.Mascota.Id;

            // Obtener datos para SelectList
            var vacunas = await _repository.GetVacunas();
            var estudios = await _repository.GetEstudios();

            viewModel.VacunasDisponibles = new SelectList(vacunas, "Id", "Nombre");
            viewModel.EstudiosDisponibles = new SelectList(estudios, "Id", "Nombre");
            viewModel.VacunasConPrecio = vacunas.Select(v => new { v.Id, v.Nombre, v.Precio }).ToList();
            viewModel.EstudiosConPrecio = estudios.Select(e => new { e.Id, e.Nombre, e.Precio }).ToList();

            return viewModel;
        }

        public async Task<string> CreateAtencionVeterinaria(AtencionVeterinariaViewModel model, ClaimsPrincipal user)
        {
            
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userIdInt))
            {
                return "Error al obtener el ID del usuario.";
            }

            var veterinario = await _repository.GetVeterinarioPorId(userIdInt);
            if (veterinario == null)
            {
                return "El usuario logueado no está asociado a un perfil de veterinario.";
            }

            model.VeterinarioId = veterinario.Id;

            // Obtener vacunas y estudios y calcular costos
            var vacunasSeleccionadas = await _repository.GetVacunaSeleccionada(model.VacunasSeleccionadasIds);
            var estudiosSeleccionados = await _repository.GetEstudioSeleccionado(model.EstudiosSeleccionadosIds);

            decimal costoVacunas = vacunasSeleccionadas.Sum(v => v.Precio);
            decimal costoEstudios = estudiosSeleccionados.Sum(e => e.Precio);
            decimal costoConsultaBase = 1000;
            decimal costoTotal = costoVacunas + costoConsultaBase + costoEstudios;

            // Crear tratamiento
            Tratamiento? tratamiento = null;
            if (!string.IsNullOrEmpty(model.Medicamento) || !string.IsNullOrEmpty(model.Dosis))
            {
                tratamiento = new Tratamiento
                {
                    Medicamento = model.Medicamento,
                    Dosis = model.Dosis,
                    Frecuencia = model.Frecuencia,
                    Duracion = model.DuracionDias,
                    Observaciones = model.ObservacionesTratamiento
                };
                await _repository.AgregarTratamiento(tratamiento);
            }

            // Crear la atención
            var atencion = new AtencionVeterinaria
            {
                Fecha = DateTime.Now,
                Diagnostico = model.Diagnostico,
                PesoMascota = model.PesoKg,
                HistoriaClinicaId = model.HistoriaClinicaId,
                VeterinarioId = model.VeterinarioId,
                Tratamiento = tratamiento,
                CostoTotal = costoTotal,
                Vacunas = vacunasSeleccionadas,
                EstudiosComplementarios = estudiosSeleccionados
            };

            await _repository.AgregarAtencionVeterinaria(atencion);
            await _repository.SaveChangesAsync();

            return null; 
        }
        public async Task<AtencionVeterinaria> ObtenerPorId(int id)
        {
            return await _repository.ObtenerAtencionConCliente(id);
        }
        public async Task<List<AtencionDetalleViewModel>> ObtenerPagosPendientesPorClienteId(int clienteId)
        {
            // NOTA: Esta es la lógica central. Tienes que ir a la BD y:
            // 1. Encontrar las Historias Clínicas asociadas a las mascotas del Cliente.
            // 2. Encontrar las Atenciones asociadas a esas Historias Clínicas.
            // 3. Filtrar las Atenciones donde el estado de pago sea "Pendiente" (o similar).

            // *** SIMULACIÓN ACADÉMICA (Debes reemplazar esto con tu lógica real de BD) ***

            // Simulamos una lista de atenciones pendientes que obtendrías de tu Repositorio
            var atencionesDB = await _repository.ObtenerAtencionesPendientesPorCliente(clienteId);

            // Si tu repositorio no tiene ese método, debes crearlo para que filtre por Cliente.Id y Atencion.EstadoDePago.

            var viewModelList = atencionesDB
                .Select(a => new AtencionDetalleViewModel
                {
                    AtencionId = a.Id,
                    CostoTotal = a.CostoTotal,
                    EstadoDePago = "Pendiente", // Esto debe venir de la DB

                    // Mapeo de propiedades heredadas para mostrar en la vista
                    Fecha = a.Fecha,
                    MascotaNombre = a.HistoriaClinica.Mascota.Nombre, // Asegúrate del Include() en el repositorio
                                                                      // ... otras propiedades necesarias
                })
                .ToList();

            return viewModelList;
        }
        public async Task RegistrarAtencionDesdeTurnoAsync(AtencionPorTurnoViewModel model, ClaimsPrincipal user)
        {
            // Usamos una transacción para garantizar la integridad de los datos.
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Obtener veterinario
                var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userIdInt))
                {
                    throw new Exception("Error al obtener el ID del usuario.");
                }

                var veterinario = await _repository.GetVeterinarioPorId(userIdInt);
                if (veterinario == null)
                {
                    throw new Exception("El usuario logueado no está asociado a un perfil de veterinario.");
                }

                model.VeterinarioId = veterinario.Id;

                // Costos
                var vacunasSeleccionadas = await _vacunaService.ObtenerPorIdsAsync(model.VacunasSeleccionadasIds);
                var estudiosSeleccionados = await _estudioService.ObtenerPorIdsAsync(model.EstudiosSeleccionadosIds);

                decimal costoVacunas = vacunasSeleccionadas.Sum(v => v.Precio);
                decimal costoEstudios = estudiosSeleccionados.Sum(e => e.Precio);
                decimal costoConsultaBase = 1000; 
                decimal costoTotal = costoVacunas + costoConsultaBase + costoEstudios;

                // Tratamiento
                Tratamiento tratamiento = null;
                if (!string.IsNullOrWhiteSpace(model.Medicamento))
                {
                    tratamiento = new Tratamiento
                    {
                        Medicamento = model.Medicamento,
                        Dosis = model.Dosis,
                        Frecuencia = model.Frecuencia,
                        Duracion = model.DuracionDias.ToString(), 
                        Observaciones = model.ObservacionesTratamiento
                    };
                    await _repository.AgregarTratamiento(tratamiento);
                }

                // Atencion
                var atencion = new AtencionVeterinaria
                {
                    Fecha = DateTime.Now,
                    Diagnostico = model.Diagnostico,
                    PesoMascota = (float)model.PesoKg,
                    HistoriaClinicaId = model.HistoriaClinicaId,
                    VeterinarioId = veterinario.Id,
                    Tratamiento = tratamiento,
                    CostoTotal = costoTotal,
                    Vacunas = vacunasSeleccionadas.ToList(),
                    EstudiosComplementarios = estudiosSeleccionados.ToList()
                };

                await _repository.AgregarAtencionVeterinaria(atencion);
                await _context.SaveChangesAsync(); 

                // Actualizar Turno
                var turno = await _turnoService.ObtenerPorIdConDatosAsync(model.TurnoId);
                if (turno == null)
                {
                    throw new Exception("El turno asociado no fue encontrado.");
                }

                turno.Estado = "Finalizado";

                _turnoService.Actualizar(turno);
                await _context.SaveChangesAsync();

                // Transaccion
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; 
            }
        }

    }


}
