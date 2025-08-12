using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // Inyectamos los servicios necesarios
        public VeterinariaController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }
        public async Task<IActionResult> PaginaPrincipal()
        {
            var viewModel = new VeterinariaPaginaPrincipalViewModel();

            //  Cargar Peluqueros
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

        // Acciones para Peluqueros
        public IActionResult Registro()
        {
            return View();
        }

        // Acción POST para procesar el formulario de registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(VeterinarioRegistroViewModel model)
        {
            // Validar el modelo
            if (!ModelState.IsValid)
            {
                // Si hay errores de validación, vuelve a mostrar el formulario
                return View(model);
            }

            // Crear el objeto Usuario para Identity
            var usuario = new Usuario
            {
                UserName = model.Email, // Usamos el email como nombre de usuario
                Email = model.Email,
                NombreUsuario = model.Nombre + "" + model.Apellido,

            };

            // Crear el usuario en la base de datos de Identity
            var result = await _userManager.CreateAsync(usuario, model.Password);

            if (!result.Succeeded)
            {
                // Si la creación falla, agrega los errores al ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                TempData["Error"] = "Hubo errores al crear el usuario. Por favor, revise los datos.";
                return View(model);
            }

            // Asignar el rol al nuevo usuario
            await _userManager.AddToRoleAsync(usuario, "Veterinario");

            // Crear el objeto Veterinario con los datos adicionales
            var veterinario = new Veterinario
            {
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                Dni = model.Dni,
                Direccion = model.Direccion,
                Telefono = model.Telefono,
                Matricula = model.Matricula,
                UsuarioId = usuario.Id // Enlaza el Veterinario con el Usuario de Identity
            };

            // Guardar Veterionario
            try
            {
                _context.Veterinarios.Add(veterinario);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Manejar errores si no se puede guardar el Veterinario
                Console.WriteLine($"Error al guardar el veterinario: {ex.Message}");
                TempData["Error"] = "Error al guardar los datos del veterinario. Por favor, inténtelo de nuevo.";
                return View(model);
            }
      
            return RedirectToAction("PaginaPrincipal", "Veterinaria");
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
                TempData["SuccessMessage"] = "Configuración de turnos guardada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Error al guardar la configuración de turnos.";
            return RedirectToAction(nameof(Index)); 
        }
    }
}
