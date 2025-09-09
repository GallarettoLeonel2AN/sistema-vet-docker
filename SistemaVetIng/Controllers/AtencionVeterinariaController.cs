using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
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
        public AtencionVeterinariaController(IAtencionVeterinariaService atencionService, 
            IToastNotification toastNotification,
            IHistoriaClinicaService historiaClinicaService)
        {
            _atencionService = atencionService;
            _toastNotification = toastNotification;
            _historiaClinicaService = historiaClinicaService;
        }

        #region REGISTRAR ATENCION
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
    }
    #endregion
}