// Archivo: Controllers/AtencionVeterinariaController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.ViewsModels;
using System.Security.Claims; // Necesario para obtener el ID del usuario logueado


namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinario")] // Solo los veterinarios pueden crear atenciones
    public class AtencionVeterinariaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AtencionVeterinariaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AtencionVeterinaria/Crear?historiaClinicaId=5
        // Muestra el formulario para registrar una nueva atención veterinaria.
        [HttpGet]
        public async Task<IActionResult> Crear(int historiaClinicaId)
        {
            // Verificamos que la Historia Clínica exista
            var historiaClinica = await _context.HistoriasClinicas
                                                .Include(hc => hc.Mascota) // Incluimos la mascota para mostrar su nombre
                                                    .ThenInclude(m => m.Propietario) // Y el propietario
                                                .FirstOrDefaultAsync(hc => hc.Id == historiaClinicaId);

            if (historiaClinica == null)
            {
                TempData["Error"] = "Historia Clínica no encontrada.";
                return RedirectToAction("ListaClientesParaSeguimiento", "HistoriaClinica");
            }
            // Obtener todas las vacunas y estudios disponibles
            ViewBag.VacunasDisponibles = new SelectList(await _context.Vacunas.ToListAsync(), "Id", "Nombre");
            ViewBag.EstudiosDisponibles = new SelectList(await _context.Estudios.ToListAsync(), "Id", "Nombre");

            // Si necesitas el precio, puedes cargarlo también
            var vacunasConPrecio = await _context.Vacunas.Select(v => new { v.Id, v.Nombre, v.Precio }).ToListAsync();
            var estudiosConPrecio = await _context.Estudios.Select(e => new { e.Id, e.Nombre, e.Precio }).ToListAsync();
            
            // Creamos una instancia del ViewModel y le pasamos el ID de la Historia Clínica
            var viewModel = new AtencionVeterinariaViewModel
            {
                HistoriaClinicaId = historiaClinicaId,
                // Puedes precargar otros valores si es necesario, por ejemplo, el peso actual si lo tuvieras
            };

            ViewBag.MascotaNombre = historiaClinica.Mascota.Nombre;
            ViewBag.PropietarioNombre = $"{historiaClinica.Mascota.Propietario?.Nombre} {historiaClinica.Mascota.Propietario?.Apellido}";
            ViewBag.MascotaId = historiaClinica.Mascota.Id; // Para el botón de volver

            return View(viewModel);
        }

        // POST: /AtencionVeterinaria/Crear
        // Procesa el formulario de registro de la atención veterinaria.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(AtencionVeterinariaViewModel model)
        {

            // Obtener el ID del veterinario logueado

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                TempData["Error"] = "No se pudo identificar al veterinario logueado.";
                return RedirectToAction("ListaClientesParaSeguimiento", "HistoriaClinica");
            }

            // Convertir el ID del usuario de string a int
            if (!int.TryParse(userIdString, out int userIdInt)) // Convertir a int el ID del usuario logueado
            {
                TempData["Error"] = "Error al obtener el ID del usuario. Formato incorrecto.";
                return RedirectToAction("ListaClientesParaSeguimiento", "HistoriaClinica");
            }

            // Buscar el Veterinario en la base de datos usando el UsuarioId que se mapea al ID del ApplicationUser
            // Asumiendo que tu modelo Veterinario tiene una propiedad int llamada 'UsuarioId'
            var veterinario = await _context.Veterinarios // _context.Veterinarios ya filtra por Discriminator si está configurado para TPH
                                            .FirstOrDefaultAsync(v => v.UsuarioId == userIdInt); // Buscar por UsuarioId (que ahora es int)

            // Si el Veterinario no se encuentra, significa que el usuario logueado no tiene un perfil de Veterinario asociado
            if (veterinario == null)
            {
                TempData["Error"] = $"El usuario logueado (ID: {userIdInt}) no está asociado a un perfil de veterinario en la base de datos.";
                return RedirectToAction("ListaClientesParaSeguimiento", "HistoriaClinica");
            }

            // Asignar el ID del Veterinario encontrado (el Id de la tabla Personas) al modelo de atención
            model.VeterinarioId = veterinario.Id; // Asigna el Id del registro de la tabla Personas/Veterinarios


            if (!ModelState.IsValid)
            {
                // Si la validación falla, volvemos a la vista con los datos del modelo.
                // Necesitamos volver a obtener los datos de la mascota y propietario para la vista.
                var historiaClinica = await _context.HistoriasClinicas
                                                    .Include(hc => hc.Mascota)
                                                        .ThenInclude(m => m.Propietario)
                                                    .FirstOrDefaultAsync(hc => hc.Id == model.HistoriaClinicaId);

                if (historiaClinica != null)
                {
                    ViewBag.MascotaNombre = historiaClinica.Mascota.Nombre;
                    ViewBag.PropietarioNombre = $"{historiaClinica.Mascota.Propietario?.Nombre} {historiaClinica.Mascota.Propietario?.Apellido}";
                    ViewBag.MascotaId = historiaClinica.Mascota.Id;
                }
                return View(model);
            }

            
            Tratamiento? tratamiento = null;
           
                tratamiento = new Tratamiento
                {
                    Medicamento = model.Medicamento,
                    Dosis = model.Dosis,
                    Frecuencia = model.Frecuencia,
                    Duracion = model.DuracionDias,
                    Observaciones = model.ObservacionesTratamiento
                };
                _context.Tratamientos.Add(tratamiento);
                // No es necesario agregar tratamiento al contexto aquí si es parte de la atención
                // EF Core lo manejará cuando se agregue la atención
            
            var vacunasSeleccionadas = new List<Vacuna>();
            decimal costoVacunas = 0;
            if (model.VacunasSeleccionadasIds != null && model.VacunasSeleccionadasIds.Any())
            {
                vacunasSeleccionadas = await _context.Vacunas
                    .Where(v => model.VacunasSeleccionadasIds.Contains(v.Id))
                    .ToListAsync();
                costoVacunas = vacunasSeleccionadas.Sum(v => v.Precio);
            }

            // Obtener los estudios completos desde la base de datos
            var estudiosSeleccionados = new List<Estudio>();
            decimal costoEstudios = 0;
            if (model.EstudiosSeleccionadosIds != null && model.EstudiosSeleccionadosIds.Any())
            {
                estudiosSeleccionados = await _context.Estudios
                    .Where(e => model.EstudiosSeleccionadosIds.Contains(e.Id))
                    .ToListAsync();
                costoEstudios = estudiosSeleccionados.Sum(e => e.Precio);
            }

            decimal costoConsultaBase = 1000;
            // Calcular el costo total
            decimal costoTotal = costoVacunas + costoConsultaBase + costoEstudios;

            // Crear la instancia de AtencionVeterinaria
            var atencion = new AtencionVeterinaria
            {
                Fecha = DateTime.Now,
                Diagnostico = model.Diagnostico,
                PesoMascota = model.PesoKg,
                HistoriaClinicaId = model.HistoriaClinicaId,
                VeterinarioId = model.VeterinarioId,
                Tratamiento = tratamiento, // Asignar el tratamiento (puede ser null)
                CostoTotal = costoTotal,

                // *** Asignas las colecciones aquí para que EF Core las persista ***
                Vacunas = vacunasSeleccionadas,
                EstudiosComplementarios = estudiosSeleccionados
            };

            try
            {
                _context.AtencionesVeterinarias.Add(atencion);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = $"Atención registrada correctamente para la mascota {ViewBag.MascotaNombre}.";
                // Redirigir de nuevo a la historia clínica de la mascota
                return RedirectToAction("DetalleHistoriaClinica", "HistoriaClinica", new { mascotaId = _context.HistoriasClinicas.Find(model.HistoriaClinicaId)?.MascotaId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar la atención: {ex.Message}");
                TempData["Error"] = "Error al registrar la atención. Por favor, inténtelo de nuevo.";

                // Volver a cargar datos para la vista en caso de error
                var historiaClinica = await _context.HistoriasClinicas
                                                    .Include(hc => hc.Mascota)
                                                        .ThenInclude(m => m.Propietario)
                                                    .FirstOrDefaultAsync(hc => hc.Id == model.HistoriaClinicaId);
                if (historiaClinica != null)
                {
                    ViewBag.MascotaNombre = historiaClinica.Mascota.Nombre;
                    ViewBag.PropietarioNombre = $"{historiaClinica.Mascota.Propietario?.Nombre} {historiaClinica.Mascota.Propietario?.Apellido}";
                    ViewBag.MascotaId = historiaClinica.Mascota.Id;
                }
                return View(model);
            }
        }
    }
}