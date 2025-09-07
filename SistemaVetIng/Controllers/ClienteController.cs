using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SistemaVetIng.Data; 
using SistemaVetIng.Models; 
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Servicios.Implementacion;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.Views.Veterinaria;
using SistemaVetIng.ViewsModels; 

namespace SistemaVetIng.Controllers
{

    public class ClienteController : Controller
    {
 
        private readonly IToastNotification _toastNotification;
        private readonly IClienteService _clienteService;
        private readonly IMascotaService _mascotaService;

        public ClienteController(IToastNotification toastNotification, IClienteService clienteService, IMascotaService mascotaService)
        {
            _toastNotification = toastNotification;
            _clienteService = clienteService;
            _mascotaService = mascotaService;
        }

        [HttpGet]
        public async Task<IActionResult> PaginaPrincipal()
        {

            var viewModel = new ClientePaginaPrincipalViewModel();
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ReservarTurno()
        {
            // 1. Obtener la información necesaria
            var clienteId = _clienteService.ObtenerPorId;

            // 2. Obtener las listas de datos (Mascotas y Veterinarios)
            var mascotasDelCliente = await _mascotaService.ListarTodo();

            // 3. Crear y rellenar el ViewModel
            var viewModel = new ReservaTurnoViewModel
            {
                Mascotas = (List<Mascota>)mascotasDelCliente,
                HasMascotas = mascotasDelCliente.Any()
            };

            // 4. Pasar el ViewModel a la vista
            return View(viewModel);
        }


        #region REGISTRAR CLIENTE
        [HttpGet]
        public IActionResult RegistrarCliente()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Prevenir ataques de falsificación de solicitudes
        public async Task<IActionResult> RegistrarCliente(ClienteRegistroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _toastNotification.AddErrorToastMessage("Hubo errores al registrar el cliente.");
                return View(model);
            }

            try
            {
                await _clienteService.Registrar(model);
                _toastNotification.AddSuccessToastMessage("¡Cliente registrado correctamente!");
                // Redireccion
                if (User.IsInRole("Veterinario"))
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinario");
                }
                else if (User.IsInRole("Veterinaria"))
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinaria");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error: {ex.Message}");
                return View(model);
            }
        }
        #endregion


        #region MODIFICAR CLIENTE
        [HttpGet]
        public async Task<IActionResult> ModificarCliente(int id)
        {

            var cliente = await _clienteService.ObtenerPorId(id);

            if (cliente == null)
            {
                _toastNotification.AddErrorToastMessage("Cliente no encontrado.");
                if (User.IsInRole("Veterinario"))
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinario");
                }
                else
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinaria");
                };
            }

            var viewModel = new ClienteEditarViewModel
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Dni = cliente.Dni,
                Email = cliente.Usuario?.UserName,
                Direccion = cliente.Direccion,
                Telefono = cliente.Telefono
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarCliente(ClienteEditarViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _clienteService.Modificar(model);
                _toastNotification.AddSuccessToastMessage("¡Cliente actualizado correctamente!");

                if (User.IsInRole("Veterinario"))
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinario");
                }
                else
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinaria");
                };
            }
            catch (KeyNotFoundException)
            {
                _toastNotification.AddErrorToastMessage("Cliente no encontrado.");
                return View(model);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error: {ex.Message}");
                return View(model);
            }
           
        }
        #endregion


        #region ELIMINAR CLIENTE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            try
            {
                await _clienteService.Eliminar(id);
                _toastNotification.AddSuccessToastMessage("El cliente ha sido eliminado exitosamente.");
            }
            catch (KeyNotFoundException)
            {
                _toastNotification.AddErrorToastMessage("El cliente que intenta eliminar no existe.");
            }
            catch (DbUpdateException)
            {
                _toastNotification.AddErrorToastMessage("No se pudo eliminar el cliente. Hay registros asociados.");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error: {ex.Message}");
            }

            if (User.IsInRole("Veterinario"))
            {
                return RedirectToAction("PaginaPrincipal", "Veterinario");
            }
            else
            {
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }
        }
        #endregion
    }
}