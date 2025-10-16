using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using SistemaVetIng.Servicios.Implementacion;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinario,Veterinaria")]
    public class MascotaController : Controller
    {
        private readonly IMascotaService _mascotaService;
        private readonly IClienteService _clienteService;
        private readonly IToastNotification _toastNotification;
        private readonly ITurnoService _turnoService;


        private readonly List<string> _razasPeligrosas = new List<string>
        {
            "pitbull", "rottweiler", "dogo argentino", "fila brasileiro",
            "akita inu", "tosa inu", "doberman", "staffordshire bull terrier",
            "american staffordshire terrier", "pastor alemán"
        };

        public MascotaController(
            IMascotaService mascotaService,
            IClienteService clienteService, 
            IToastNotification toastNotification,
            ITurnoService turnoService)
        {

            _mascotaService = mascotaService;
            _clienteService = clienteService;
            _toastNotification = toastNotification;
            _turnoService = turnoService;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Detalle(int id)
        {
            if (id <= 0)
            {
                _toastNotification.AddErrorToastMessage("ID de mascota inválido.");
                return RedirectToAction("PaginaPrincipal", "Cliente");
            }


            var viewModel = await _mascotaService.ObtenerDetalleConHistorial(id);

            if (viewModel == null)
            {
                _toastNotification.AddErrorToastMessage("Mascota no encontrada.");
                return RedirectToAction("PaginaPrincipal", "Cliente");
            }


            if (User.IsInRole("Cliente"))
            {
                var userName = User.Identity.Name;

                var clienteActual = await _clienteService.ObtenerClientePorUserNameAsync(userName);

                if (clienteActual == null || !viewModel.PropietarioNombreCompleto.Contains(clienteActual.Nombre))
                {
                    _toastNotification.AddWarningToastMessage("Acceso denegado. No es el propietario de esta mascota.");
                    return RedirectToAction("PaginaPrincipal", "Cliente");
                }
            }

            return View(viewModel);
        }


        #region LISTARCLIENTES

        public async Task<IActionResult> ListarClientes()
        {
            var clientes = await _clienteService.ListarTodo();
            return View(clientes);
        }
        #endregion

        #region REGISTRAR MASCOTA

        [HttpGet]

        public async Task<IActionResult> RegistrarMascota(int clienteId, int? turnoIdParaRedireccion = null)
        {
            var cliente = await _clienteService.ObtenerPorId(clienteId);
            if (cliente == null)
            {
                _toastNotification.AddInfoToastMessage("El Cliente no fue encontrado.");
                return RedirectToAction(nameof(ListarClientes)); 
            }

            ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";

            var model = new MascotaRegistroViewModel
            {
                ClienteId = clienteId,
                TurnoIdParaRedireccion = turnoIdParaRedireccion
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarMascota(MascotaRegistroViewModel model)
        {

            if (!ModelState.IsValid)
            {
                var cliente = await _clienteService.ObtenerPorId(model.ClienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                }
                return View(model);
            }


            var (nuevaMascota, success, message) = await _mascotaService.Registrar(model);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(message);

                // Redireccion: Si el registro viene de un turno:
                if (model.TurnoIdParaRedireccion.HasValue)
                {
                    // Buscamos el turno original para vincularle la mascota.
                    var turnoOriginal = await _turnoService.ObtenerPorIdConDatosAsync(model.TurnoIdParaRedireccion.Value);
                    if (turnoOriginal != null)
                    {
                        turnoOriginal.MascotaId = nuevaMascota.Id;
                        _turnoService.Actualizar(turnoOriginal);
                        await _turnoService.Guardar();
                    }

                    return RedirectToAction("RegistrarAtencionConTurno", "AtencionVeterinaria", new { turnoId = model.TurnoIdParaRedireccion.Value });
                }

                // Si no, es una creación normal
                return RedirectToAction(nameof(ListarClientes));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(message);
                return View(model);
            }
        }
        #endregion

        #region MODIFICAR MASCOTA

        [HttpGet]
        public async Task<IActionResult> ModificarMascota(int? id)
        {
            if (id == null)
            {
                _toastNotification.AddErrorToastMessage("No se pudo encontrar la mascota. ID no proporcionado.");
                return RedirectToAction(nameof(ListarClientes));
            }

            var mascota = await _mascotaService.ObtenerPorId(id.Value);

            if (mascota == null)
            {
                _toastNotification.AddErrorToastMessage("La mascota que intenta editar no existe.");
                return RedirectToAction(nameof(ListarClientes));
            }


            var viewModel = new MascotaEditarViewModel
            {
                Id = mascota.Id,
                ClienteId = mascota.ClienteId,
                Nombre = mascota.Nombre,
                Especie = mascota.Especie,
                Raza = mascota.Raza,
                FechaNacimiento = mascota.FechaNacimiento,
                Sexo = mascota.Sexo,
                RazaPeligrosa = mascota.RazaPeligrosa,
                Chip = (mascota.Chip != null)
            };


            var cliente = await _clienteService.ObtenerPorId(mascota.ClienteId);
            if (cliente != null)
            {
                ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarMascota(MascotaEditarViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var cliente = await _clienteService.ObtenerPorId(model.ClienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                }
                _toastNotification.AddErrorToastMessage("Hubo un error con los datos proporcionados.");
                return View(model);
            }

            try
            {
                var (success, message) = await _mascotaService.Modificar(model);

                if (success)
                {
                    _toastNotification.AddSuccessToastMessage(message);
                    return RedirectToAction(nameof(ListarClientes)); 
                }
                else
                {
                    _toastNotification.AddErrorToastMessage(message);
                    var cliente = await _clienteService.ObtenerPorId(model.ClienteId);
                    if (cliente != null)
                    {
                        ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                    }
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error inesperado: {ex.Message}");
                var cliente = await _clienteService.ObtenerPorId(model.ClienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                }
                return View(model);
            }
        }
        #endregion

        #region ELIMINAR MASCOTA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarMascota(int? id)
        {

            if (id == null)
            {
                _toastNotification.AddErrorToastMessage("No se pudo eliminar la mascota. ID no proporcionado.");
                if (User.IsInRole("Veterinario"))
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinario");
                }
                else
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinaria");
                }
                ;
            }

            var (success, message) = await _mascotaService.Eliminar(id.Value);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(message);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(message);
            }

            if (User.IsInRole("Veterinario"))
            {
                return RedirectToAction("PaginaPrincipal", "Veterinario");
            }
            else
            {
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }
            ;
        }
        #endregion


    }
}