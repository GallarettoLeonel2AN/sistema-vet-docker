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

        public ClienteController(IToastNotification toastNotification, IClienteService clienteService)
        {
            _toastNotification = toastNotification;
            _clienteService = clienteService;
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