using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Globalization;
using System.Threading.Tasks;

namespace SistemaVetIng.Controllers
{
    public class TurnoController : Controller
    {
        private readonly IToastNotification _toastNotification;
        private readonly IMascotaService _mascotaService;
        private readonly ITurnoService _turnoService;
        private readonly IClienteService _clienteService;
        private readonly UserManager<Usuario> _userManager;

        public TurnoController(
            IToastNotification toastNotification,
            IMascotaService mascotaService,
            UserManager<Usuario> userManager,
            IClienteService clienteService,
            ITurnoService turnoService)
        {
            _toastNotification = toastNotification;
            _mascotaService = mascotaService;
            _userManager = userManager;
            _turnoService = turnoService;
            _clienteService = clienteService;
        }

        [HttpGet]
        public async Task<IActionResult> ReservarTurno()
        {
            var usuarioActual = await _userManager.GetUserAsync(User);
            if (usuarioActual == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cliente = await _clienteService.ObtenerPorIdUsuario(usuarioActual.Id);
            if (cliente == null)
            {
                _toastNotification.AddWarningToastMessage("Debe completar su perfil de cliente para reservar un turno.");
           
                return RedirectToAction("RegistrarCliente", "Cliente");
            }

            var mascotasDelCliente = (await _mascotaService.ListarMascotasPorClienteId(cliente.Id)).ToList();

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
            // Lógica para manejar Primera Cita
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

            var usuarioActual = await _userManager.GetUserAsync(User);
            if (usuarioActual == null)
            {
                return Json(new { success = false, message = "Usuario no autenticado." });
            }

            var cliente = await _clienteService.ObtenerPorIdUsuario(usuarioActual.Id);
            if (cliente == null)
            {
                return Json(new { success = false, message = "No se encontró el perfil de cliente." });
            }

            model.ClienteId = cliente.Id;


            try
            {
                await _turnoService.ReservarTurnoAsync(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Registra la excepción para fines de depuración
                return Json(new { success = false, message = "Ocurrió un error al reservar el turno." });
            }
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