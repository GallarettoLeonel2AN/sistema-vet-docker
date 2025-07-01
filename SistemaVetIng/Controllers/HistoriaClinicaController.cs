// Archivo: Controllers/HistoriaClinicaController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using System.Linq; // Necesario para OrderBy
using System.Threading.Tasks; // Necesario para async/await

namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinario")] // Asumiendo que solo los veterinarios pueden acceder a esto
    public class HistoriaClinicaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HistoriaClinicaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /HistoriaClinica/ListaClientesParaSeguimiento
        // Muestra una lista de todos los clientes para buscar.
        public async Task<IActionResult> ListaClientesParaSeguimiento(string searchString)
        {
            var clientes = from c in _context.Clientes
                           select c;

            if (!string.IsNullOrEmpty(searchString))
            {
                // Convierte la cadena de búsqueda a minúsculas una sola vez para eficiencia
                searchString = searchString.ToLower();
                clientes = clientes.Where(c => c.Nombre.ToLower().Contains(searchString) ||
                                               c.Apellido.ToLower().Contains(searchString) ||
                                               c.Dni.ToString().Contains(searchString));
            }

            // Carga todos los clientes de la base de datos y los ordena.
            var clientesList = await clientes.OrderBy(c => c.Apellido).ToListAsync();

            ViewBag.SearchString = searchString; // Para mantener el valor de búsqueda en la vista
            return View(clientesList);
        }
        public async Task<IActionResult> MascotasCliente(int clienteId)
        {
            // Carga el cliente junto con sus mascotas
            var cliente = await _context.Clientes
                                        .Include(c => c.Mascotas) // Incluye las mascotas asociadas
                                        .FirstOrDefaultAsync(c => c.Id == clienteId);

            if (cliente == null)
            {
                TempData["Error"] = "El cliente seleccionado no existe.";
                return RedirectToAction(nameof(ListaClientesParaSeguimiento));
            }

            // Pasa el cliente (que ahora incluye sus mascotas) a la vista.
            // La vista se encargará de iterar sobre cliente.Mascotas
            return View(cliente);
        }
        // GET: /HistoriaClinica/DetalleHistoriaClinica/5
        // Muestra la historia clínica completa de una mascota.
        public async Task<IActionResult> DetalleHistoriaClinica(int mascotaId)
        {
            // Cargar la Mascota, incluyendo su HistoriaClinica y las Atenciones asociadas
            var mascota = await _context.Mascotas
                                        .Include(m => m.Propietario) // Para mostrar los datos del propietario
                                        .Include(m => m.HistoriaClinica)
                                            .ThenInclude(hc => hc.Atenciones)// Cargar las atenciones de la historia clínica
                                                .ThenInclude(a => a.Tratamiento) // Y los tratamientos dentro de las atenciones
                                        
                                         .Include(m => m.HistoriaClinica)
                                            .ThenInclude(hc => hc.Atenciones)
                                             .ThenInclude(a => a.Veterinario)

                                        .Include(m => m.HistoriaClinica) // Necesario para cargar también las vacunas y estudios si son parte de la atención
                                            .ThenInclude(hc => hc.Atenciones)
                                                .ThenInclude(a => a.Vacunas)
                                        .Include(m => m.HistoriaClinica)
                                            .ThenInclude(hc => hc.Atenciones)
                                                .ThenInclude(a => a.EstudiosComplementarios)
                                        .FirstOrDefaultAsync(m => m.Id == mascotaId);

            if (mascota == null)
            {
                TempData["Error"] = "La mascota no existe.";
                return RedirectToAction(nameof(ListaClientesParaSeguimiento));
            }
            if (mascota.HistoriaClinica == null)
            {
                // Esto no debería pasar con la lógica actual, pero es una buena práctica
                TempData["Error"] = "La mascota no tiene una historia clínica asociada.";
                return RedirectToAction("MascotasCliente", new { clienteId = mascota.ClienteId });
            }

            // Pasamos la Mascota (que incluye la HistoriaClinica y sus Atenciones) a la vista.
            return View(mascota);
        }
        // Próximos métodos:
        // - MascotasCliente(int clienteId)
        // - DetalleHistoriaClinica(int mascotaId)
    }
}