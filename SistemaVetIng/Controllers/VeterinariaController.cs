using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinaria")] // Solamente pueden acceder los roles Veterinaria 
    public class VeterinariaController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification _toastNotification;

        // Inyectamos los servicios necesarios
        public VeterinariaController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            ApplicationDbContext context, 
            IToastNotification toastNotification)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context; 
            _toastNotification = toastNotification;
        }
        [HttpGet]
        public async Task<IActionResult> PaginaPrincipal()
        {
            var viewModel = new VeterinariaPaginaPrincipalViewModel();

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

        // Acciones para Configuración de Turnos **EN DESARROLLO**
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarConfiguracionTurnos(DisponibilidadViewModel model)
        {
            if (ModelState.IsValid)
            {
                var config = await _context.Disponibilidades.FirstOrDefaultAsync(c => c.Id == 1);
                if (config == null)
                {
                    config = new Disponibilidad { Id = 1 }; // Crear si no existe
                    _context.Disponibilidades.Add(config);
                }

                config.HoraInicio = model.HoraInicio;
                config.HoraFin = model.HoraFin;
                config.DuracionMinutosPorConsulta = model.DuracionMinutosPorConsulta;
                config.TrabajaLunes = model.TrabajaLunes;
                config.TrabajaMartes = model.TrabajaMartes;
                config.TrabajaMiercoles = model.TrabajaMiercoles;
                config.TrabajaJueves = model.TrabajaJueves;
                config.TrabajaViernes = model.TrabajaViernes;
                config.TrabajaSabado = model.TrabajaSabado;
                config.TrabajaDomingo = model.TrabajaDomingo;

                await _context.SaveChangesAsync();
                _toastNotification.AddSuccessToastMessage("¡Configuración de turnos guardada exitosamente.!");
                return RedirectToAction(nameof(Index));
            }
            _toastNotification.AddErrorToastMessage("Error al guardar la configuración de turnos.");
            return RedirectToAction(nameof(Index)); 
        }
    }
}
