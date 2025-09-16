using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Globalization;

namespace SistemaVetIng.Controllers
{
    public class TurnoController : Controller
    {


        private readonly IToastNotification _toastNotification;
        private readonly IMascotaService _mascotaService;
        private readonly ITurnoService _turnoService;
        private readonly UserManager<Usuario> _userManager;

        public TurnoController(
            IToastNotification toastNotification,
            IMascotaService mascotaService,
            UserManager<Usuario> userManager,
            ITurnoService turnoService)
        {
            _toastNotification = toastNotification;
            _mascotaService = mascotaService;
            _userManager = userManager;
            _turnoService = turnoService;
        }

        [HttpGet]
        public async Task<IActionResult> ReservarTurno()
        {
            var usuarioActual = await _userManager.GetUserAsync(User);
            if (usuarioActual == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var mascotasDelCliente = (await _mascotaService.ListarMascotasPorClienteId(usuarioActual.Id)).ToList();

            var viewModel = new ReservaTurnoViewModel
            {
                Mascotas = mascotasDelCliente.ToList(),
                HasMascotas = mascotasDelCliente.Any()
            };

            return View(viewModel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reservar(ReservaTurnoViewModel model)
        {
            // Logiva para manejar Primera Cita
            if (model.PrimeraCita)
            {
                model.MascotaId = null;
            }
            else
            {
                if (model.MascotaId == null || model.MascotaId == 0)
                {
                    ModelState.AddModelError("MascotaId", "Por favor, seleccione una mascota.");
                }
            }


            if (!ModelState.IsValid)
            {
                var usuarioActual = await _userManager.GetUserAsync(User);

                if (usuarioActual != null)
                {
                    model.Mascotas = (await _mascotaService.ListarMascotasPorClienteId(usuarioActual.Id)).ToList();
                }
                return View("ReservarTurno", model);
            }

            var cliente = await _userManager.GetUserAsync(User);
            if (cliente == null)
            {
                return Unauthorized();
            }
            model.ClienteId = cliente.Id;

            await _turnoService.ReservarTurnoAsync(model);

            _toastNotification.AddSuccessToastMessage("¡Turno reservado con éxito!");
            return RedirectToAction("PaginaPrincipal", "Cliente");
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerHorariosDisponibles(string fecha)
        {
            const string formatoFecha = "yyyy-MM-dd";
            DateTime fechaSeleccionada;

            if (!DateTime.TryParseExact(fecha, formatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaSeleccionada))
            {
                return Json(new List<string>());
            }

            var horarios = await _turnoService.GetHorariosDisponiblesAsync(fechaSeleccionada);
            return Json(horarios);
        }

    }
}
