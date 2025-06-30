// Archivo: Controllers/MascotaController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Controllers
{
    // Solo permitimos el acceso a los usuarios con el rol "Veterinario"
    [Authorize(Roles = "Veterinario")]
    public class MascotaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MascotaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Mascota/ListaClientes
        // Muestra una lista de todos los clientes para que el veterinario elija uno.
        public async Task<IActionResult> ListaClientes()
        {
            // Carga todos los clientes de la base de datos.
            // También puedes agregar un .OrderBy() para ordenar la lista.
            var clientes = await _context.Clientes
                                         .OrderBy(c => c.Apellido) // Opcional: ordena por apellido
                                         .ToListAsync();

            // Verificamos si la lista tiene elementos para debug
            Console.WriteLine($"Se encontraron {clientes.Count} clientes en la base de datos.");

            return View(clientes);
        }

        // GET: /Mascota/Registrar/5
        // Muestra el formulario para registrar una mascota para un cliente específico.
        [HttpGet]
        public IActionResult Registro(int clienteId)
        {
            // Validamos que el cliente exista antes de mostrar el formulario.
            var cliente = _context.Clientes.Find(clienteId);
            if (cliente == null)
            {
                TempData["Error"] = "El cliente seleccionado no existe.";
                return RedirectToAction(nameof(ListaClientes));
            }

            // Creamos una instancia del ViewModel para el formulario.
            var viewModel = new MascotaRegistroViewModel
            {
                ClienteId = clienteId,
                // Opcionalmente, puedes precargar la lista de clientes aquí,
                // aunque para este flujo no es estrictamente necesario ya que el ClienteId ya fue seleccionado.
                // Lo dejaremos simple por ahora.
            };

            ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}"; // Pasamos el nombre para mostrarlo en la vista.

            return View(viewModel);
        }

        // POST: /Mascota/Registrar
        // Procesa el formulario de registro de la mascota.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(MascotaRegistroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Si la validación falla, volvemos a la vista con los datos del modelo.
                // Necesitamos volver a obtener el nombre del cliente para mostrarlo.
                var cliente = await _context.Clientes.FindAsync(model.ClienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                }
                return View(model);
            }

            // Creamos el objeto Mascota a partir del ViewModel.
            var mascota = new Mascota
            {
                Nombre = model.Nombre,
                Raza = model.Raza,
                FechaNacimiento = model.FechaNacimiento,
                Sexo = model.Sexo,
                RazaPeligrosa = model.RazaPeligrosa,
                // Asociamos la mascota al cliente seleccionado
                Propietario = await _context.Clientes.FindAsync(model.ClienteId)
            };

            if (mascota.Propietario == null)
            {
                // Si por alguna razón el propietario no se encuentra, manejamos el error.
                TempData["Error"] = "El cliente propietario no fue encontrado.";
                return RedirectToAction(nameof(ListaClientes));
            }

            try
            {
                _context.Mascotas.Add(mascota);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = $"Mascota {mascota.Nombre} registrada correctamente para el cliente {mascota.Propietario.Nombre} {mascota.Propietario.Apellido}.";
                // Redirigimos a la lista de clientes o a donde quieras.
                return RedirectToAction(nameof(ListaClientes));
            }
            catch (Exception ex)
            {
                // En un entorno de desarrollo, es bueno imprimir el error para depurar.
                Console.WriteLine($"Error al guardar la mascota: {ex.Message}");

                // Muestra un mensaje de error genérico.
                TempData["Error"] = "Error al registrar la mascota. Por favor, inténtelo de nuevo.";
                // Vuelve a la vista con los datos para que el usuario no pierda lo que ingresó.
                var cliente = await _context.Clientes.FindAsync(model.ClienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                }
                return View(model);
            }
        }
    }
}