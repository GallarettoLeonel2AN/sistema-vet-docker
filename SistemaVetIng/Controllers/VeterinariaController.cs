using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Repository.Implementacion;
using SistemaVetIng.Servicios.Implementacion;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinaria")] 
    public class VeterinariaController : Controller
    {
       
        private readonly IToastNotification _toastNotification;
        private readonly IVeterinariaConfigService _veterinariaConfigService;
        private readonly IVeterinarioService _veterinarioService;
        private readonly IClienteService _clienteService;
        private readonly IMascotaService _mascotaService;
        private readonly IVeterinariaService _veterinariaService;

        public VeterinariaController(
            IVeterinariaConfigService service,
            IToastNotification toastNotification,
            IMascotaService mascotaService,
            IClienteService clienteService,
            IVeterinarioService veterinarioService,
            IVeterinariaService veterinariaService
            )
        {
            _mascotaService = mascotaService;
            _veterinarioService = veterinarioService;
            _clienteService = clienteService;
            _veterinariaConfigService = service;
            _toastNotification = toastNotification;
            _veterinariaService = veterinariaService;
        }

        #region PAGINA PRINCIPAL
        [HttpGet]
        public async Task<IActionResult> PaginaPrincipal(
            string busquedaVeterinario = null,
            string busquedaCliente = null,
            string busquedaMascota = null)
        {
            var viewModel = new VeterinariaPaginaPrincipalViewModel();

            // Carga la configuración de turnos desde la base de datos.
            var configuracionDb = (await _veterinariaConfigService.ListarTodo()).FirstOrDefault();

            if (configuracionDb != null)
            {
                viewModel.ConfiguracionTurnos = new ConfiguracionVeterinariaViewModel
                {
                    HoraInicio = configuracionDb.HoraInicio,
                    HoraFin = configuracionDb.HoraFin,
                    DuracionMinutosPorConsulta = configuracionDb.DuracionMinutosPorConsulta,
                    TrabajaLunes = configuracionDb.TrabajaLunes,
                    TrabajaMartes = configuracionDb.TrabajaMartes,
                    TrabajaMiercoles = configuracionDb.TrabajaMiercoles,
                    TrabajaJueves = configuracionDb.TrabajaJueves,
                    TrabajaViernes = configuracionDb.TrabajaViernes,
                    TrabajaSabado = configuracionDb.TrabajaSabado,
                    TrabajaDomingo = configuracionDb.TrabajaDomingo
                };
            }
            else
            {
                viewModel.ConfiguracionTurnos = null;
            }

            //  Cargar Veterinario
            var veterinarios = string.IsNullOrWhiteSpace(busquedaVeterinario)
            ? await _veterinarioService.ListarTodo()
            : await _veterinarioService.FiltrarPorBusqueda(busquedaVeterinario);
            viewModel.Veterinarios = veterinarios.Select(p => new VeterinarioViewModel
            {
                Id = p.Id,
                NombreCompleto = $"{p.Nombre} {p.Apellido}",
                Telefono = p.Telefono,
                NombreUsuario = p.Usuario?.Email,
                Direccion = p.Direccion,
                Matricula = p.Matricula,
            }).ToList();

            // Cargar Clientes en las tablas
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


            // Hardcoded de datos para Reportes Analíticos 
            // Cards
            viewModel.CantidadPerrosPeligrosos = 5;

            var razaMayorDemanda = mascotas
                 .GroupBy(m => m.Raza)
                 .OrderByDescending(g => g.Count())
                 .Select(g => g.Key)
                 .FirstOrDefault();

            viewModel.RazaMayorDemanda = razaMayorDemanda;
            viewModel.IngresosMensualesEstimados = 1500.00m; 
            viewModel.IngresosDiariosEstimados = 5000.00m;    

            return View(viewModel);
        }
        #endregion

        #region CONFIGURACION HORARIOS
        [HttpGet]
        public IActionResult GuardarConfiguracion()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarConfiguracion(ConfiguracionVeterinaria model)
        {
            if (!ModelState.IsValid)
            {
                _toastNotification.AddErrorToastMessage("Hay errores en el formulario. Por favor, corríjalos.");
                return View(model);
            }

            try
            {
                await _veterinariaConfigService.Agregar(model);
                _toastNotification.AddSuccessToastMessage("Configuración guardada con éxito.");
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Ocurrió un error inesperado. Por favor, inténtelo de nuevo.");
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }
        }
        #endregion
    }
}
