using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaVetIng.Data;
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Models;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Controllers
{
    public class ClienteController : Controller
    {
       
            private readonly UserManager<Usuario> _userManager;
            private readonly SignInManager<Usuario> _signInManager;
            private readonly ApplicationDbContext _context;

            public ClienteController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, ApplicationDbContext context)
            {
                _userManager = userManager;
                _signInManager = signInManager;
                _context = context;
            }

            [HttpGet]
            public IActionResult Registro()
            {
                return View();
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Registro(ClienteRegistroViewModel model)
            {
                if (!ModelState.IsValid)
                    return View(model);

                var usuario = new Usuario
                {
                    UserName = model.Email,
                    Email = model.Email,
                    NombreUsuario = model.Nombre +"" + model.Apellido ,
                  
                };

                var result = await _userManager.CreateAsync(usuario, model.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    TempData["Error"] = "Hubo errores al crear el usuario.";
                    return View(model);
                }

                await _userManager.AddToRoleAsync(usuario, "Cliente");

                var cliente = new Cliente
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Dni = model.Dni,
                    Telefono = model.Telefono,
                    Direccion = model.Direccion,
                    UsuarioId = usuario.Id,
                };
              ;
            // Dentro de tu método Registro [HttpPost]
            // ... después de crear el objeto 'cliente' o 'veterinario'
            try
            {
                // Asumimos que es ClienteController
                _context.Clientes.Add(cliente);

                // Este es el paso que podría estar fallando
                await _context.SaveChangesAsync();

                // Si llegamos aquí, todo salió bien
                TempData["Mensaje"] = "¡Cliente registrado con éxito!";

                // Redirige a donde quieras después del registro exitoso
                // Por ejemplo, a la lista de clientes para asociar mascotas
                return RedirectToAction("Index", "Home");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // Este bloque de código capturará y mostrará el error de la base de datos
                // Imprime el error en la ventana de Salida de Visual Studio
                Console.WriteLine("--- ERROR AL GUARDAR EN LA BASE DE DATOS ---");
                Console.WriteLine($"Error principal: {ex.Message}");
                Console.WriteLine($"Error interno: {ex.InnerException?.Message}"); // <-- ¡Este es el mensaje clave!
                Console.WriteLine("------------------------------------------");

                // Pasa un mensaje de error a la vista para el usuario
                ModelState.AddModelError(string.Empty, "Hubo un error al guardar el cliente. Es posible que el DNI ya esté registrado.");

                // Vuelve a la vista para que el usuario pueda corregir los datos
                return View(model);
            }

            await _signInManager.SignInAsync(usuario, isPersistent: false);

                TempData["Mensaje"] = "Cliente registrado correctamente.";
                return RedirectToAction("Index", "Home");
            }
        }
    }

