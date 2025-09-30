using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinario,Veterinaria")]
    public class VeterinarioController : Controller
    {
        private readonly IToastNotification _toastNotification;
        private readonly IVeterinarioService _veterinarioService;
        private readonly IClienteService _clienteService;
        private readonly IMascotaService _mascotaService;

        public VeterinarioController(
            IVeterinarioService veterinarioService,
            IToastNotification toastNotification,
            IClienteService clienteService,
            IMascotaService mascotaService)
        {
            _veterinarioService = veterinarioService;
            _toastNotification = toastNotification;
            _clienteService = clienteService;
            _mascotaService = mascotaService;
        }

        #region PAGINA PRINCIPAL
        [HttpGet]
        public async Task<IActionResult> PaginaPrincipal(string busquedaCliente = null,
            string busquedaMascota = null)
        {

            var viewModel = new VeterinarioPaginaPrincipalViewModel();
            var clientes = string.IsNullOrWhiteSpace(busquedaCliente)
            ? await _clienteService.ListarTodo()
            : await _clienteService.FiltrarPorBusqueda(busquedaCliente);
            viewModel.Clientes = clientes.Select(c => new ClienteViewModel
            {
                Id = c.Id,
                NombreCompleto = $"{c.Nombre} {c.Apellido}",
                Telefono = c.Telefono,
                NombreUsuario = c.Usuario?.Email,
                DNI = c.Dni
            }).ToList();

            // Cargar Mascotas en las tablas
            var mascotas = string.IsNullOrWhiteSpace(busquedaMascota)
            ? await _mascotaService.ListarTodo()
            : await _mascotaService.FiltrarPorBusqueda(busquedaMascota);
            viewModel.Mascotas = mascotas.Select(m => new MascotaListViewModel
            {
                Id = m.Id,
                NombreMascota = m.Nombre,
                Especie = m.Especie,
                Sexo = m.Sexo,
                Raza = m.Raza,
                EdadAnios = DateTime.Today.Year - m.FechaNacimiento.Year - (DateTime.Today.Month < m.FechaNacimiento.Month || (DateTime.Today.Month == m.FechaNacimiento.Month && DateTime.Today.Day < m.FechaNacimiento.Day) ? 1 : 0),
                NombreDueno = $"{m.Propietario?.Nombre} {m.Propietario?.Apellido}",
                ClienteId = m.Propietario?.Id ?? 0
            }).ToList();

            return View(viewModel);
        }
        #endregion


        #region REGISTRAR VETERINARIO

        [HttpGet]
        public IActionResult RegistrarVeterinario()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarVeterinario(VeterinarioRegistroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _toastNotification.AddErrorToastMessage("Hubo errores al registrar el veterinario.");
                return View(model);
            }

            try
            {
                await _veterinarioService.Registrar(model);
                _toastNotification.AddSuccessToastMessage("¡Veterinario registrado correctamente!");
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error: {ex.Message}");
                return View(model);
            }
        }
        #endregion


        #region MODIFICAR VETERINARIO
        [HttpGet]
        public async Task<IActionResult> ModificarVeterinario(int id)
        {
            var veterinario = await _veterinarioService.ObtenerPorId(id);
            if (veterinario == null)
            {
                _toastNotification.AddErrorToastMessage("Veterinario no encontrado.");
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }

            // Mapeo manual de la entidad al ViewModel para mostrarlo en el formulario de Modificar
            var viewModel = new VeterinarioEditarViewModel
            {
                Id = veterinario.Id,
                Nombre = veterinario.Nombre,
                Apellido = veterinario.Apellido,
                Dni = veterinario.Dni,
                Email = veterinario.Usuario?.Email,
                Direccion = veterinario.Direccion,
                Telefono = veterinario.Telefono,
                Matricula = veterinario.Matricula
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarVeterinario(VeterinarioEditarViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _veterinarioService.Modificar(model);
                _toastNotification.AddSuccessToastMessage("¡Veterinario actualizado correctamente!");
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }
            catch (KeyNotFoundException)
            {
                _toastNotification.AddErrorToastMessage("Veterinario no encontrado.");
                return View(model);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error: {ex.Message}");
                return View(model);
            }
        }
        #endregion


        #region ELIMINAR VETERINARIO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarVeterinario(int id)
        {
            try
            {
                await _veterinarioService.Eliminar(id);
                _toastNotification.AddSuccessToastMessage("El veterinario ha sido eliminado exitosamente.");
            }
            catch (KeyNotFoundException)
            {
                _toastNotification.AddErrorToastMessage("El veterinario que intenta eliminar no existe.");
            }
            catch (DbUpdateException)
            {
                _toastNotification.AddErrorToastMessage("No se pudo eliminar el veterinario. Hay registros asociados.");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error: {ex.Message}");
            }

            return RedirectToAction("PaginaPrincipal", "Veterinaria");
        }
        #endregion


    }
}
