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
                    NombreUsuario = model.Email,
                    Clave = model.Password // esto debería estar encriptado por Identity, no lo guardes así
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

                try
                {
                    _context.Clientes.Add(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {

                    // ¡Esto es clave para ver el error real!
                    Console.WriteLine("--- ERROR AL GUARDAR CLIENTE ---");
                    Console.WriteLine($"Mensaje: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception Mensaje: {ex.InnerException.Message}");
                        Console.WriteLine($"Inner Exception StackTrace: {ex.InnerException.StackTrace}");
                    }
                    Console.WriteLine("-------------------------------");

                    TempData["Error"] = "Error al guardar el cliente en la base de datos. Por favor, inténtelo de nuevo. Detalles: " + ex.Message;
                    return View(model);
                }

                await _signInManager.SignInAsync(usuario, isPersistent: false);

                TempData["Mensaje"] = "Cliente registrado correctamente.";
                return RedirectToAction("Index", "Home");
            }
        }
    }

