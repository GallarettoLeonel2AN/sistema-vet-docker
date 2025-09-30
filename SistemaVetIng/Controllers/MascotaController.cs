using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
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


        private readonly List<string> _razasPeligrosas = new List<string>
        {
            "pitbull", "rottweiler", "dogo argentino", "fila brasileiro",
            "akita inu", "tosa inu", "doberman", "staffordshire bull terrier",
            "american staffordshire terrier", "pastor alemán"
        };

        public MascotaController(IMascotaService mascotaService, IClienteService clienteService, IToastNotification toastNotification)
        {

            _mascotaService = mascotaService;
            _clienteService = clienteService;
            _toastNotification = toastNotification;
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
        public async Task<IActionResult> RegistrarMascota(int clienteId)
        {

            var cliente = await _clienteService.ObtenerPorId(clienteId);
            if (cliente == null)
            {
                _toastNotification.AddInfoToastMessage("Seleccione un Cliente.");
                return RedirectToAction(nameof(ListarClientes));
            }

            ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";

            var model = new MascotaRegistroViewModel
            {
                ClienteId = clienteId
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

            
            var (success, message) = await _mascotaService.Registrar(model);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(message);
                return RedirectToAction(nameof(ListarClientes));
            }
            else
            {
                
                _toastNotification.AddErrorToastMessage(message);
                // Si el error es por un cliente no válido, redirige a la lista de clientes.
                if (message.Contains("El cliente asociado no es válido"))
                {
                    return RedirectToAction(nameof(ListarClientes));
                }

                
                var cliente = await _clienteService.ObtenerPorId(model.ClienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                }
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