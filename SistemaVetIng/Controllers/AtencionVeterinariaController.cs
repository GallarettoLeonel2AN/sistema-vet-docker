using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NToastNotify;
using SistemaVetIng.Servicios.Implementacion;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;


namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinario,Veterinaria")]
    public class AtencionVeterinariaController : Controller
    {
        private readonly IAtencionVeterinariaService _atencionService;
        private readonly IToastNotification _toastNotification;
        private readonly IHistoriaClinicaService _historiaClinicaService;
        private readonly ITurnoService _turnoService;
        private readonly IVacunaService _vacunaService;
        private readonly IEstudioService _estudioService;

        public AtencionVeterinariaController(IAtencionVeterinariaService atencionService,
            IToastNotification toastNotification,
            IHistoriaClinicaService historiaClinicaService,
            ITurnoService turnoService,
            IVacunaService vacunaService,
            IEstudioService estudioService)
        {
            _atencionService = atencionService;
            _toastNotification = toastNotification;
            _historiaClinicaService = historiaClinicaService;
            _turnoService = turnoService;
            _vacunaService = vacunaService;
            _estudioService = estudioService;
        }

        #region REGISTRAR ATENCION SINTURNO
        [HttpGet]
        public async Task<IActionResult> RegistrarAtencion(int historiaClinicaId)
        {
            var viewModel = await _atencionService.GetAtencionVeterinariaViewModel(historiaClinicaId);

            if (viewModel == null)
            {
                _toastNotification.AddErrorToastMessage("Historia Clinica no encontrada!");
                return RedirectToAction("ListaClientesParaSeguimiento", "HistoriaClinica");
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarAtencion(AtencionVeterinariaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Si la validación falla, volvemos a cargar los datos necesarios para la vista
                var viewModel = await _atencionService.GetAtencionVeterinariaViewModel(model.HistoriaClinicaId);
                if (viewModel != null)
                {
                    viewModel.Diagnostico = model.Diagnostico;
                    viewModel.PesoKg = model.PesoKg;
                    viewModel.Medicamento = model.Medicamento;
                    viewModel.Dosis = model.Dosis;
                    viewModel.Frecuencia = model.Frecuencia;
                    viewModel.DuracionDias = model.DuracionDias;
                    viewModel.ObservacionesTratamiento = model.ObservacionesTratamiento;
                    viewModel.VacunasSeleccionadasIds = model.VacunasSeleccionadasIds;
                    viewModel.EstudiosSeleccionadosIds = model.EstudiosSeleccionadosIds;
                }
                return View(viewModel);
            }

            var result = await _atencionService.CreateAtencionVeterinaria(model, User);

            if (result != null)
            {
                _toastNotification.AddErrorToastMessage("Error al crear la atencion!");
                var viewModel = await _atencionService.GetAtencionVeterinariaViewModel(model.HistoriaClinicaId);
                return View(viewModel);
            }

            _toastNotification.AddSuccessToastMessage("Atencion creada correctamente!");
            // Obtener el Id de la mascota desde la base de datos
            var historiaClinica = await _historiaClinicaService.GetDetalleHistoriaClinica(model.HistoriaClinicaId);
            if (historiaClinica != null)
            {
                return RedirectToAction("DetalleHistoriaClinica", "HistoriaClinica", new { mascotaId = historiaClinica.Id });
            }
            return RedirectToAction("ListaClientesParaSeguimiento", "HistoriaClinica");
        }
    #endregion


    #region REGISTRAR ATENCION TURNO

    [HttpGet]
        public async Task<IActionResult> AtencionPorTurno(int turnoId)
        {

            var todasLasVacunas = await _vacunaService.ListarTodoAsync();
            var todosLosEstudios = await _estudioService.ListarTodoAsync();


            var turno = await _turnoService.ObtenerPorIdConDatosAsync(turnoId); 
            if (turno == null)
            {
                _toastNotification.AddErrorToastMessage("El turno no fue encontrado.");
                return RedirectToAction("PaginaPrincipal", "Veterinario");
            }

            if (turno.PrimeraCita && turno.MascotaId == null)
            {
                _toastNotification.AddInfoToastMessage("Es una primera cita. Por favor, registra primero la mascota.");
                return RedirectToAction("Crear", "Mascota", new { clienteId = turno.ClienteId, turnoIdParaRedireccion = turno.Id });
            }

            var viewModel = new AtencionPorTurnoViewModel
            {
                TurnoId = turno.Id,
                MascotaId = turno.MascotaId.Value,
                NombreMascota = turno.Mascota.Nombre,
                NombreCliente = $"{turno.Cliente.Nombre} {turno.Cliente.Apellido}",
                VacunasDisponibles = new SelectList(todasLasVacunas, "Id", "Nombre"),
                EstudiosDisponibles = new SelectList(todosLosEstudios, "Id", "Nombre")
            };

            // 4. Mostramos la nueva vista.
            return View("AtencionVeterinariaPorTurno", viewModel);
        }


        // ===== MÉTODO POST: Guarda la atención y actualiza el turno =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtencionPorTurno(AtencionPorTurnoViewModel model)
        {
            var todasLasVacunas = await _vacunaService.ListarTodoAsync();
            var todosLosEstudios = await _estudioService.ListarTodoAsync();

            if (!ModelState.IsValid)
            {
                _toastNotification.AddErrorToastMessage("Por favor, corrige los errores del formulario.");
                // Si hay un error, debemos volver a cargar los dropdowns antes de mostrar la vista.
                model.VacunasDisponibles = new SelectList(todasLasVacunas, "Id", "Nombre");
                model.EstudiosDisponibles = new SelectList(todosLosEstudios, "Id", "Nombre");
                return View("AtencionVeterinariaPorTurno", model);
            }

            try
            {
                // Llamamos a un servicio que encapsula toda la lógica de guardado.
                await _atencionService.RegistrarAtencionDesdeTurnoAsync(model);

                _toastNotification.AddSuccessToastMessage("Atención registrada y turno finalizado con éxito.");
                return RedirectToAction("PaginaPrincipal", "Veterinario");
            }
            catch (Exception ex)
            {
                // Loguear el error 'ex' es una buena práctica.
                _toastNotification.AddErrorToastMessage("Ocurrió un error inesperado al guardar la atención.");
                model.VacunasDisponibles = new SelectList(todasLasVacunas, "Id", "Nombre");
                model.EstudiosDisponibles = new SelectList(todosLosEstudios, "Id", "Nombre");
                return View("AtencionVeterinariaPorTurno", model);
            }
        }
        #endregion
    }
}