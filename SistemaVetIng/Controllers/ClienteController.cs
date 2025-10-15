using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SistemaVetIng.Models;
using SistemaVetIng.Servicios.Implementacion;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Security.Claims;

namespace SistemaVetIng.Controllers
{

    public class ClienteController : Controller
    {

        private readonly IToastNotification _toastNotification;
        private readonly IClienteService _clienteService;
        private readonly ITurnoService _turnoService;
        

        public ClienteController(IToastNotification toastNotification, 
            IClienteService clienteService,
            ITurnoService turnoService)
        {
            _toastNotification = toastNotification;
            _clienteService = clienteService;
            _turnoService = turnoService;
        }


        #region PAGINA PRINCIPAL
        [HttpGet]
        public async Task<IActionResult> PaginaPrincipal()
        {

            var usuarioIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Convertir el ID (string) a un entero (int).
            if (!int.TryParse(usuarioIdString, out int usuarioIdNumerico))
            {
                return BadRequest("El formato ID de usuario no es valido");
            }

            var cliente = await _clienteService.ObtenerPorIdUsuario(usuarioIdNumerico);

            var viewModel = new ClientePaginaPrincipalViewModel();

            var turnos = await _turnoService.ObtenerTurnosPorClienteIdAsync(cliente.Id);

            if (turnos != null && turnos.Any())
            {
                viewModel.Turnos = turnos.Select(t => new TurnoViewModel
                {
                    Id = t.Id,
                    Fecha = t.Fecha,
                    Horario = t.Horario,
                    Estado = t.Estado,
                    Motivo = t.Motivo,
                    PrimeraCita = t.PrimeraCita,
                    Cliente = t.Cliente,
                    Mascota = t.Mascota,
                }).ToList();
            }
            else
            {
                viewModel.Turnos = new List<TurnoViewModel>();
            }

            return View(viewModel);
        }
        #endregion


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
                }
                ;
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
                }
                ;
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