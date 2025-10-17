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
        private readonly IMascotaService _mascotaService;
        private readonly IAtencionVeterinariaService _atencionVeterinariaService;


        public ClienteController(IToastNotification toastNotification, 
            IClienteService clienteService,
            ITurnoService turnoService,
            IMascotaService mascotaService,
            IAtencionVeterinariaService atencionVeterinariaService)
        {
            _toastNotification = toastNotification;
            _clienteService = clienteService;
            _turnoService = turnoService;
            _mascotaService = mascotaService;
            _atencionVeterinariaService = atencionVeterinariaService;
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

            // 1. Obtener el cliente y el nombre de usuario
            var userName = User.Identity.Name;

            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "Account");
            }

            // Usaremos esta instancia de cliente para obtener turnos y mascotas
            var cliente = await _clienteService.ObtenerClientePorUserNameAsync(userName);

            if (cliente == null)
            {
                // Redirigir si no se encuentra el cliente con el userName
                return RedirectToAction("Login", "Account");
            }
            // 2. Obtener datos del cliente (Turnos y Mascotas)
            var turnos = await _turnoService.ObtenerTurnosPorClienteIdAsync(cliente.Id);
            var mascotas = await _mascotaService.ObtenerMascotasPorClienteUserNameAsync(userName);
            // 3. Obtener Pagos Pendientes (¡INTEGRACIÓN MERCADO PAGO!)
            // Llamamos al nuevo servicio para obtener la lista de AtencionDetalleViewModel
            var pagosPendientes = await _atencionVeterinariaService.ObtenerPagosPendientesPorClienteId(cliente.Id);
            // Inicializar el ViewModel
            var viewModel = new ClientePaginaPrincipalViewModel
            {
                // Asignar inmediatamente el nombre completo
                NombreCompleto = $"{cliente.Nombre} {cliente.Apellido}"
            };

            // 2. Obtener y asignar los turnos
            

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

            
            viewModel.Mascotas = mascotas.ToList();
            viewModel.PagosPendientes = pagosPendientes;

            // Devolver la vista con el viewModel completo
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