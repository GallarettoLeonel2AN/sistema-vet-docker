using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using SistemaVetIng.Servicios.Interfaces;


namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinario,Veterinaria")]
    public class HistoriaClinicaController : Controller
    {
        private readonly IHistoriaClinicaService _historiaClinicaService;
        private readonly IToastNotification _toastNotification;
        public HistoriaClinicaController(IHistoriaClinicaService historiaClinicaService, IToastNotification toastNotification)
        {
            _historiaClinicaService = historiaClinicaService;
            _toastNotification = toastNotification;
        }

        #region LISTADO CLIENTES
        public async Task<IActionResult> ListaClientesParaSeguimiento(string searchString)
        {
            var clientesList = await _historiaClinicaService.GetClientesParaSeguimiento(searchString);
            ViewBag.SearchString = searchString;
            return View(clientesList);
        }
        #endregion

        #region LISTADO MASCOTAS DEL CLIENTE
        public async Task<IActionResult> MascotasCliente(int clienteId)
        {
            var cliente = await _historiaClinicaService.GetMascotasCliente(clienteId);

            if (cliente == null)
            {
                _toastNotification.AddErrorToastMessage("El cliente no existe!");
                return RedirectToAction(nameof(ListaClientesParaSeguimiento));
            }

            return View(cliente);
        }
        #endregion

        #region DETALLES DE HISTORIAS CLINICAS
        public async Task<IActionResult> DetalleHistoriaClinica(int mascotaId)
        {
            var mascota = await _historiaClinicaService.GetDetalleHistoriaClinica(mascotaId);

            if (mascota == null)
            {
                return RedirectToAction(nameof(ListaClientesParaSeguimiento));
            }
            if (mascota.HistoriaClinica == null)
            {
                return RedirectToAction("MascotasCliente", new { clienteId = mascota.Propietario.Id });
            }

            return View(mascota);
        }
        #endregion
    }
}