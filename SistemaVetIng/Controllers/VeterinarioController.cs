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
    [Authorize(Roles = "Veterinario,Veterinaria")]
    public class VeterinarioController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification _toastNotification;

        // Inyectamos los servicios necesarios
        public VeterinarioController(
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

            var viewModel = new VeterinarioPaginaPrincipalViewModel();

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

            return View(viewModel);
        }


        // **ACCIONES PARA LOS VETERINARIOS**

        // ------------------ REGISTRAR VETERINARIO ------------------ 
        #region REGISTRAR VETERINARIO
        public IActionResult RegistrarVeterinario()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarVeterinario(VeterinarioRegistroViewModel model)
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
                _toastNotification.AddErrorToastMessage("Hubo errores al crear el usuario. Por favor, revise los datos");
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
                _toastNotification.AddErrorToastMessage("Error al guardar los datos del veterinario. Por favor, inténtelo de nuevo.");
                return View(model);
            }

            return RedirectToAction("PaginaPrincipal", "Veterinaria");
        }
        #endregion

        // ------------------ MODIFICAR VETERINARIO ------------------ 
        #region MODIFICAR VETERINARIO 
        [HttpGet]
        public async Task<IActionResult> ModificarVeterinario(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Usamos .Include() para cargar explícitamente el objeto 'Usuario'
            var veterinario = await _context.Veterinarios
                .Include(v => v.Usuario)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (veterinario == null)
            {
                return NotFound();
            }

            var viewModel = new VeterinarioEditarViewModel
            {
                Id = veterinario.Id,
                Nombre = veterinario.Nombre,
                Apellido = veterinario.Apellido,
                Dni = veterinario.Dni,
                Email = veterinario.Usuario?.UserName,
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
            // Validar si el modelo recibido es válido
            if (ModelState.IsValid)
            {
                // Buscar al veterinario existente en la base de datos por su ID
                var veterinario = await _context.Veterinarios.FindAsync(model.Id);
                if (veterinario == null)
                {
                    return NotFound();
                }

                // Actualizar las propiedades del modelo de la BD con los datos del ViewModel
                veterinario.Nombre = model.Nombre;
                veterinario.Apellido = model.Apellido;
                veterinario.Dni = model.Dni;
                veterinario.Direccion = model.Direccion;
                veterinario.Telefono = model.Telefono;
                veterinario.Matricula = model.Matricula;

                await _context.SaveChangesAsync();
                _toastNotification.AddSuccessToastMessage("¡Veterinario actualizado correctamente!");

                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }

            // Si el modelo no es válido, devolver la vista con los errores
            return View(model);
        }
        #endregion

        // ------------------ ELIMINAR VETERINARIO ------------------ 
        #region ELIMINAR VETERINARIO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarVeterinario(int? id)
        {
            // Validar que se recibió un ID
            if (id == null)
            {
                _toastNotification.AddErrorToastMessage("No se pudo eliminar el veterinario. ID no proporcionado.");
    
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }

            var veterinario = await _context.Veterinarios.Include(v => v.Usuario).FirstOrDefaultAsync(v => v.Id == id);

            if (veterinario == null)
            {
                _toastNotification.AddErrorToastMessage("El veterinario que intenta eliminar no existe.");
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }

            try
            {
             
                // Eliminar primero el veterinario
                _context.Veterinarios.Remove(veterinario);
                await _context.SaveChangesAsync();

                // Eliminar luego el usuario asociado
                if (veterinario.Usuario != null)
                {
                    var result = await _userManager.DeleteAsync(veterinario.Usuario);
                    if (!result.Succeeded)
                    {
                        // Manejar el caso en que la eliminación del usuario falle
                        _toastNotification.AddErrorToastMessage("No se pudo eliminar el usuario asociado al veterinario.");
                        return RedirectToAction("PaginaPrincipal", "Veterinaria");
                    }
                }
                _toastNotification.AddSuccessToastMessage("El veterinario ha sido eliminado exitosamente.");
               
            }
            catch (DbUpdateException)
            {

                _toastNotification.AddErrorToastMessage("No se pudo eliminar el veterinario. Hay registros asociados.");
            }

            return RedirectToAction("PaginaPrincipal", "Veterinaria");
        }
        #endregion


    }
}
