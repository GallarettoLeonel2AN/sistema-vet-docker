using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Servicios.Implementacion;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinaria")] 
    public class VeterinariaController : Controller
    {
       
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification _toastNotification;
        private readonly IVeterinariaService _veterinariaService;
        public VeterinariaController(
            IVeterinariaService service,
            ApplicationDbContext context,
            IToastNotification toastNotification)
        {
            
            _context = context;
            _veterinariaService = service;
            _toastNotification = toastNotification;
        }
        [HttpGet]
        public async Task<IActionResult> PaginaPrincipal()
        {
            var viewModel = new VeterinariaPaginaPrincipalViewModel();

            // Carga la configuración de turnos desde la base de datos.
            var configuracionDb = await _context.ConfiguracionVeterinarias.FirstOrDefaultAsync();

            if (configuracionDb != null)
            {
                // Si la configuración existe en la BD, la mapeamos al ViewModel
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
                // Si no hay configuración, la propiedad del ViewModel se mantiene como null
                viewModel.ConfiguracionTurnos = null;
            }

            //  Cargar Veterinario
            viewModel.Veterinarios = await _context.Veterinarios
                .Select(p => new VeterinarioViewModel
                {
                    Id = p.Id,
                    NombreCompleto = $"{p.Nombre} {p.Apellido}",
                    Telefono = p.Telefono,
                    NombreUsuario = p.Usuario.Email,
                    Direccion = p.Direccion,
                    Matricula = p.Matricula,
                })
                .ToListAsync();

            // Cargar Clientes en las tablas
            viewModel.Clientes = await _context.Clientes
                .Select(c => new ClienteViewModel
                {
                    Id = c.Id,
                    NombreCompleto = $"{c.Nombre} {c.Apellido}",
                    Telefono = c.Telefono,
                    NombreUsuario = c.Usuario.Email
                })
                .ToListAsync();

            // Cargar Mascotas en las tablas
            viewModel.Mascotas = await _context.Mascotas
                .Include(m => m.Propietario)
                .Select(m => new MascotaListViewModel
                {
                    Id = m.Id,
                    NombreMascota = m.Nombre,
                    Especie = m.Especie,
                    Sexo = m.Sexo,
                    Raza = m.Raza,
                    EdadAnios = DateTime.Today.Year - m.FechaNacimiento.Year - (DateTime.Today.Month < m.FechaNacimiento.Month || (DateTime.Today.Month == m.FechaNacimiento.Month && DateTime.Today.Day < m.FechaNacimiento.Day) ? 1 : 0),
                    NombreDueno = $"{m.Propietario.Nombre} {m.Propietario.Apellido}",
                    ClienteId = m.Propietario.Id
                })
                .ToListAsync();

            // Hardcoded de datos para Reportes Analíticos 
            // Card Perros Peligrosos
            viewModel.CantidadPerrosPeligrosos = 5;
        
            // Card Raza mas Demandada
            viewModel.RazaMayorDemanda = _context.Mascotas
                .GroupBy(m => m.Raza)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

            // Card Ingresos 
            viewModel.IngresosMensualesEstimados = 150000.00m; 
            viewModel.IngresosDiariosEstimados = 5000.00m;    

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> FiltrarVeterinario(string busqueda)
        {
            var consulta = _context.Veterinarios.AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                consulta = consulta.Where(v =>
                    v.Nombre.Contains(busqueda) ||
                    v.Apellido.Contains(busqueda)
                );
            }

            var veterinariosViewModel = await consulta
                .Select(p => new VeterinarioViewModel
                {
                    Id = p.Id,
                    NombreCompleto = $"{p.Nombre} {p.Apellido}",
                    Telefono = p.Telefono,
                    NombreUsuario = p.Usuario.Email,
                    Direccion = p.Direccion,
                    Matricula = p.Matricula,
                })
                .ToListAsync();

            var viewModelPagina = new VeterinariaPaginaPrincipalViewModel
            {
                Veterinarios = veterinariosViewModel
            };

            return View("PaginaPrincipal", viewModelPagina);
        }

        [HttpGet]
        public async Task<IActionResult> FiltrarCliente(string busqueda)
        {
            var consulta = _context.Clientes.AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                consulta = consulta.Where(c => c.Dni.ToString().Contains(busqueda));
            }

            var clientesViewModel = await consulta
                .Select(p => new ClienteViewModel
                {
                    Id = p.Id,
                    NombreCompleto = $"{p.Nombre} {p.Apellido}",
                    Telefono = p.Telefono,
                    NombreUsuario = p.Usuario.Email,
                })
                .ToListAsync();

            var viewModelPagina = new VeterinariaPaginaPrincipalViewModel
            {
                Clientes = clientesViewModel
            };

            return View("PaginaPrincipal", viewModelPagina);
        }

        [HttpGet]
        public async Task<IActionResult> FiltrarMascota(string busqueda)
        {
            var consulta = _context.Mascotas.Include(m => m.Propietario).AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                consulta = consulta.Where(m => m.Propietario.Dni.ToString().Contains(busqueda));
            }

            var mascotasViewModel = await consulta

                .Select(m => new MascotaListViewModel
                {
                    Id = m.Id,
                    NombreMascota = m.Nombre,
                    Especie = m.Especie,
                    Raza = m.Raza,
                    Sexo = m.Sexo,
                    EdadAnios = (DateTime.Now.Year - m.FechaNacimiento.Year),
                    NombreDueno = $"{m.Propietario.Nombre} {m.Propietario.Apellido}"
                })
                .ToListAsync();

            var viewModelPagina = new VeterinariaPaginaPrincipalViewModel
            {
                Mascotas = mascotasViewModel
            };

            return View("PaginaPrincipal", viewModelPagina);
        }


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
                await _veterinariaService.Agregar(model);
                _toastNotification.AddErrorToastMessage("Configuracion guardada con exito.");
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Ocurrió un error inesperado. Por favor, inténtelo de nuevo.");
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }
        }

    }
}
