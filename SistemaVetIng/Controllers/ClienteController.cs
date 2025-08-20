using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data; 
using SistemaVetIng.Models; 
using SistemaVetIng.Models.Indentity;
using SistemaVetIng.Views.Veterinaria;
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

        // **ACCIONES PARA LOS CLIENTES**


        // ------------------ REGISTRAR CLIENTE ------------------ 
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
            // Validamos reglas del viewmodel
            if (!ModelState.IsValid)
                return View(model); 

     
            var usuario = new Usuario
            {
                UserName = model.Email, // Usamos el email como nombre de usuario.
                Email = model.Email,
                NombreUsuario = model.Nombre + "" + model.Apellido, // Combinamos nombre y apellido para el nombre de usuario.
            };

            // Intentamos crear el usuario en el sistema de autenticación de Identity con la contraseña proporcionada.
            var result = await _userManager.CreateAsync(usuario, model.Password);

            // Si la creación del usuario falla
            if (!result.Succeeded)
            {
                // Agregamos los errores de Identity al ModelState para que se muestren en la vista.
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                TempData["Error"] = "Hubo errores al crear el usuario."; 
                return View(model); 
            }

            // Si el usuario se creó con exito en Identity, le asignamos el rol "Cliente".
            await _userManager.AddToRoleAsync(usuario, "Cliente");

            // Creamos al cliente 
            var cliente = new Cliente
            {
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                Dni = model.Dni,
                Telefono = model.Telefono,
                Direccion = model.Direccion,
                UsuarioId = usuario.Id, // Vinculación clave con la cuenta de Identity.
            };

            try
            {
                _context.Clientes.Add(cliente); // Agregamos el nuevo cliente al contexto.
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "¡Cliente registrado con éxito!";

                // Redirigimos al usuario al inicio
                if (User.IsInRole("cliente"))
                {
                    return RedirectToAction("PaginaPrincipal", "cliente");
                }
                else if (User.IsInRole("Veterinaria"))
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinaria");
                }
                // Si no es ninguno de los roles anteriores, asumimos que es un cliente
                // y lo redirigimos a la página de inicio
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            // Si hay Error
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // Registramos el error completo en la consola
                Console.WriteLine("--- ERROR AL GUARDAR EN LA BASE DE DATOS ---");
                Console.WriteLine($"Error principal: {ex.Message}");
                Console.WriteLine($"Error interno: {ex.InnerException?.Message}"); 
                Console.WriteLine("------------------------------------------");

                // Mostramos un mensaje de error
                ModelState.AddModelError(string.Empty, "Hubo un error al guardar el cliente. Es posible que el DNI ya esté registrado.");

                return View(model);
            }
        }
        #endregion

        // ------------------ MODIFICAR CLIENTE ------------------ 
        #region MODIFICAR CLIENTE
        [HttpGet]
        public async Task<IActionResult> ModificarCliente(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Usamos .Include() para cargar explícitamente el objeto 'Usuario'
            var cliente = await _context.Clientes
                .Include(v => v.Usuario)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (cliente == null)
            {
                return NotFound();
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
            // Validar si el modelo recibido es válido
            if (ModelState.IsValid)
            {
                // Buscar al cliente existente en la base de datos por su ID
                var cliente = await _context.Clientes.FindAsync(model.Id);
                if (cliente == null)
                {
                    return NotFound();
                }

                // Actualizar las propiedades del modelo de la BD con los datos del ViewModel
                cliente.Nombre = model.Nombre;
                cliente.Apellido = model.Apellido;
                cliente.Dni = model.Dni;
                cliente.Direccion = model.Direccion;
                cliente.Telefono = model.Telefono;

                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Cliente actualizado correctamente.";

                // Redirigimos al usuario al inicio
                if (User.IsInRole("cliente"))
                {
                    return RedirectToAction("PaginaPrincipal", "cliente");
                }
                else if (User.IsInRole("Veterinaria"))
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinaria");
                }
            }

            // Si el modelo no es válido, devolver la vista con los errores
            return View(model);
        }
        #endregion

        // ------------------ ELIMINAR CLIENTE ------------------ 
        #region ELIMINAR CLIENTE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCliente(int? id)
        {
            // Validar que se recibió un ID
            if (id == null)
            {
                TempData["Error"] = "No se pudo eliminar el cliente. ID no proporcionado.";
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }

            var cliente = await _context.Clientes.Include(v => v.Usuario).FirstOrDefaultAsync(v => v.Id == id);

            if (cliente == null)
            {
                TempData["Error"] = "El cliente que intenta eliminar no existe.";
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }

            try
            {

                // Eliminar primero el cliente
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();

                // Eliminar luego el usuario asociado
                if (cliente.Usuario != null)
                {
                    var result = await _userManager.DeleteAsync(cliente.Usuario);
                    if (!result.Succeeded)
                    {
                        // Manejar el caso en que la eliminación del usuario falle
                        TempData["Error"] = "No se pudo eliminar el usuario asociado al cliente.";
                        return RedirectToAction("PaginaPrincipal", "Veterinaria");
                    }
                }

                TempData["Mensaje"] = "El cliente ha sido eliminado exitosamente.";
            }
            catch (DbUpdateException)
            {
                // Manejar errores de la base de datos
                TempData["Error"] = "No se pudo eliminar el cliente. Hay registros asociados.";
            }

            return RedirectToAction("PaginaPrincipal", "Veterinaria");
        }
        #endregion
    }
}