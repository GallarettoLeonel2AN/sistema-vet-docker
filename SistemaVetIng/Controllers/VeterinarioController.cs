using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaVetIng.Data;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Models;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Controllers
{
    public class VeterinarioController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly ApplicationDbContext _context;

        // Inyectamos los servicios necesarios
        public VeterinarioController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // Acción GET para mostrar el formulario de registro
        [HttpGet]
        public IActionResult PaginaPrincipal()
        {
            
            return View(); // Por defecto, busca la vista "Dashboard.cshtml" en la carpeta "Views/Veterinario".
        }
        public IActionResult Registro()
        {
            // Devuelve la vista que crearemos en la siguiente sección
            return View();
        }

        // Acción POST para procesar el formulario de registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(VeterinarioRegistroViewModel model)
        {
            // 1. Validar el modelo
            if (!ModelState.IsValid)
            {
                // Si hay errores de validación, vuelve a mostrar el formulario
                return View(model);
            }

            // 2. Crear el objeto Usuario para Identity
            var usuario = new Usuario
            {
                UserName = model.Email, // Usamos el email como nombre de usuario
                Email = model.Email,
                NombreUsuario = model.Nombre + "" + model.Apellido,
          
            };

            // 3. Crear el usuario en la base de datos de Identity
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

            // 4. Asignar el rol al nuevo usuario (asegúrate de que el rol "Veterinario" exista)
            await _userManager.AddToRoleAsync(usuario, "Veterinario");

            // 5. Crear el objeto Veterinario con los datos adicionales
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

            // 6. Guardar el objeto Veterinario en la base de datos
            try
            {
                _context.Veterinarios.Add(veterinario);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Manejar errores si no se puede guardar el Veterinario
                // Opcionalmente, podrías eliminar el usuario de Identity si falla el guardado aquí
                Console.WriteLine($"Error al guardar el veterinario: {ex.Message}");
                TempData["Error"] = "Error al guardar los datos del veterinario. Por favor, inténtelo de nuevo.";
                return View(model);
            }


            // 7. Iniciar sesión del nuevo usuario (opcional)
            await _signInManager.SignInAsync(usuario, isPersistent: false);

            // 8. Redirigir a una página de éxito
            //TempData["Mensaje"] = "Veterinario registrado correctamente.";
            return RedirectToAction("Index", "Home");
        }
    }
}
