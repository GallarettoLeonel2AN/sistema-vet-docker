using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.ViewsModels;
using System.Text;
using System.Text.Json;

namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinario,Veterinaria")]
    public class MascotaController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly List<string> _razasPeligrosas = new List<string>
        {
            "pitbull", "rottweiler", "dogo argentino", "fila brasileiro",
            "akita inu", "tosa inu", "doberman", "staffordshire bull terrier",
            "american staffordshire terrier", "pastor alemán"
        };

        public MascotaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Listado de clientes para asociarle MASCOTAS
        public async Task<IActionResult> ListarClientes()
        {
            var clientes = await _context.Clientes
                                         .OrderBy(c => c.Apellido)
                                         .ToListAsync();
            return View(clientes); 
        }

        // **ACCIONES PARA LAS MASCOTAS**

        // ------------------ REGISTRAR MASCOTA ------------------ 
        #region REGISTRAR MASCOTA

        [HttpGet]
        public async Task<IActionResult> RegistrarMascota(int clienteId)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                TempData["Error"] = "El cliente seleccionado no existe.";
                return RedirectToAction(nameof(ListarClientes)); 
            }

            var viewModel = new MascotaRegistroViewModel
            {
                ClienteId = clienteId,
                // Inicializamos RazaPeligrosa y Chip a false por defecto
                RazaPeligrosa = false,
                Chip = false
            };

            ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}"; // Pasamos el nombre para mostrarlo en la vista.

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarMascota(MascotaRegistroViewModel model)
        {
            model.RazaPeligrosa = IsRazaPeligrosa(model.Especie, model.Raza);

            if (!ModelState.IsValid)
            {
                var cliente = await _context.Clientes.FindAsync(model.ClienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                }
                return View(model);
            }

            var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == model.ClienteId);
            if (!clienteExiste)
            {
                TempData["Error"] = "El cliente asociado no es válido. Intente de nuevo.";
                return RedirectToAction(nameof(ListarClientes));
            }

            var mascota = new Mascota
            {
                Nombre = model.Nombre,
                Especie = model.Especie,
                Raza = model.Raza,
                FechaNacimiento = model.FechaNacimiento,
                Sexo = model.Sexo,
                RazaPeligrosa = model.RazaPeligrosa,
                ClienteId = model.ClienteId
                // La propiedad Chip de Mascota será asignada condicionalmente
            };

            // Handling HistoriaClinica
            var historiaClinica = new HistoriaClinica();
            historiaClinica.Mascota = mascota;
            mascota.HistoriaClinica = historiaClinica;

            try
            {
                _context.Mascotas.Add(mascota);
                await _context.SaveChangesAsync(); // Guarda la mascota y su historia clínica

                // *** Lógica para el CHIP y la API de Perros Peligrosos ***
                bool apiCommunicationSuccess = true;
                string apiMessage = "";

                if (mascota.RazaPeligrosa)
                {
                    Chip chipAsociado = null; // Inicialmente no hay chip

                    if (model.Chip)
                    {
                        // Si la mascota peligrosa tiene chip, creamos el objeto Chip
                        chipAsociado = new Chip
                        {
                            Codigo = Guid.NewGuid().ToString("N").Substring(0, 16), // Genera un código de 16 caracteres hex
                            MascotaId = mascota.Id // Asocia el chip con la Mascota recién creada
                        };
                        mascota.Chip = chipAsociado; // Asigna el chip a la mascota
                        _context.Chips.Add(chipAsociado); // Añade el chip al contexto para guardarlo
                        await _context.SaveChangesAsync(); // Guarda el chip
                    }

                    // Prepara los datos para enviar a la API de Perros Peligrosos
                    var clienteAsociado = await _context.Clientes.FindAsync(model.ClienteId);

                    // Llamamos a metodo SendDataToDangerousDogApi
                    apiCommunicationSuccess = await SendDataToDangerousDogApi(
                        mascota.Id,
                        mascota.Nombre,
                        mascota.Raza,
                        mascota.RazaPeligrosa,
                        model.Chip, // true/false del checkbox
                        chipAsociado?.Codigo, // El código del chip (será null si no tiene chip)
                        clienteAsociado.Dni,
                        clienteAsociado.Nombre,
                        clienteAsociado.Apellido
                    );

                    if (apiCommunicationSuccess)
                    {
                        apiMessage = (model.Chip)
                            ? $"Datos de mascota peligrosa y chip (Código: {chipAsociado?.Codigo}) enviados exitosamente a la API externa."
                            : "Notificación de mascota peligrosa sin chip enviada exitosamente a la API externa.";
                    }
                    else
                    {
                        //Si la comunicación con la API falla mostrar un error
                        apiMessage = "Hubo un problema al comunicar con la API de perros peligrosos.";
                    }
                }

                TempData["Mensaje"] = $"Mascota '{mascota.Nombre}' registrada correctamente. " + apiMessage;
                return RedirectToAction(nameof(ListarClientes));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar la mascota: {ex.Message}");
                // Loggear el stack trace completo para mas detalles de error
                Console.WriteLine(ex.StackTrace);

                TempData["Error"] = "Error al registrar la mascota. Por favor, inténtelo de nuevo.";
                // Vuelve a la vista con los datos para que el usuario no pierda lo que ya ingreso
                var cliente = await _context.Clientes.FindAsync(model.ClienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                }
                return View(model);
            }
        }
        #endregion


        // ------------------ MODIFICAR MASCOTA ------------------
        #region MODIFICAR MASCOTA

        [HttpGet]
        public async Task<IActionResult> ModificarMascota(int? id)
        {
            // 1. Validar que se recibió un ID
            if (id == null)
            {
                TempData["Error"] = "No se pudo encontrar la mascota. ID no proporcionado.";
                return RedirectToAction(nameof(ListarClientes));
            }

            // 2. Buscar la mascota en la base de datos
            var mascota = await _context.Mascotas.Include(m => m.Propietario).FirstOrDefaultAsync(m => m.Id == id);

            // 3. Validar si la mascota existe
            if (mascota == null)
            {
                TempData["Error"] = "La mascota que intenta editar no existe.";
                return RedirectToAction(nameof(ListarClientes));
            }

            // 4. Mapear la entidad a un ViewModel para la vista
            var viewModel = new MascotaEditarViewModel
            {
                Id = mascota.Id,
                ClienteId = mascota.ClienteId,
                Nombre = mascota.Nombre,
                Especie = mascota.Especie,
                Raza = mascota.Raza,
                FechaNacimiento = mascota.FechaNacimiento,
                Sexo = mascota.Sexo,
                RazaPeligrosa = mascota.RazaPeligrosa,
                Chip = (mascota.Chip != null) // Verifica si la mascota tiene un chip asociado
            };

            // 5. Pasar el nombre del cliente a la vista
            if (mascota.Propietario != null)
            {
                ViewBag.ClienteNombre = $"{mascota.Propietario.Nombre} {mascota.Propietario.Apellido}";
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarMascota(MascotaEditarViewModel model)
        {
            // Volver a verificar si es una raza peligrosa, ya que el usuario pudo haberla cambiado
            model.RazaPeligrosa = IsRazaPeligrosa(model.Especie, model.Raza);

            if (!ModelState.IsValid)
            {
                // Si el modelo no es válido, vuelve a cargar el nombre del cliente y la vista
                var cliente = await _context.Clientes.FindAsync(model.ClienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                }
                TempData["Error"] = "Hubo un error con los datos proporcionados.";
                return View(model);
            }

            // 1. Buscar la mascota a editar en la base de datos
            var mascota = await _context.Mascotas.Include(m => m.Chip).FirstOrDefaultAsync(m => m.Id == model.Id);

            if (mascota == null)
            {
                TempData["Error"] = "La mascota que intenta editar no existe.";
                return RedirectToAction(nameof(ListarClientes));
            }

            // 2. Actualizar las propiedades de la entidad con los datos del ViewModel
            mascota.Nombre = model.Nombre;
            mascota.Especie = model.Especie;
            mascota.Raza = model.Raza;
            mascota.FechaNacimiento = model.FechaNacimiento;
            mascota.Sexo = model.Sexo;
            mascota.RazaPeligrosa = model.RazaPeligrosa;

            try
            {
                // 3. Manejar la lógica del chip
                if (mascota.RazaPeligrosa && model.Chip && mascota.Chip == null)
                {
                    // Caso 1: Se convirtió en peligrosa y se le agregó un chip
                    var nuevoChip = new Chip
                    {
                        Codigo = Guid.NewGuid().ToString("N").Substring(0, 16),
                        MascotaId = mascota.Id
                    };
                    mascota.Chip = nuevoChip;
                    _context.Chips.Add(nuevoChip);
                }
                else if (mascota.RazaPeligrosa && !model.Chip && mascota.Chip != null)
                {
                    // Caso 2: Era peligrosa con chip y ahora ya no tiene chip
                    _context.Chips.Remove(mascota.Chip);
                    mascota.Chip = null;
                }
                // Si no es peligrosa, aseguramos que el chip se elimine si existía
                else if (!mascota.RazaPeligrosa && mascota.Chip != null)
                {
                    _context.Chips.Remove(mascota.Chip);
                    mascota.Chip = null;
                }

                // 4. Llamar a la API si es una raza peligrosa
                if (mascota.RazaPeligrosa)
                {
                    var clienteAsociado = await _context.Clientes.FindAsync(model.ClienteId);
                    await SendDataToDangerousDogApi(
                       mascota.Id,
                       mascota.Nombre,
                       mascota.Raza,
                       mascota.RazaPeligrosa,
                       model.Chip,
                       mascota.Chip?.Codigo,
                       clienteAsociado.Dni,
                       clienteAsociado.Nombre,
                       clienteAsociado.Apellido
                    );
                }

                // 5. Guardar los cambios
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = $"Mascota '{mascota.Nombre}' actualizada correctamente.";
                return View("MascotaActualizada", mascota);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar la mascota: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                TempData["Error"] = "Error al actualizar la mascota. Por favor, inténtelo de nuevo.";

                // Vuelve a la vista con los datos para que el usuario no pierda lo que ya ingreso
                var cliente = await _context.Clientes.FindAsync(model.ClienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                }
                return View(model);
            }
        }
        #endregion

        // ------------------ ELIMINAR MASCOTA ------------------
        #region ELIMINAR MASCOTA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarMascota(int? id)
        {
            // Validar que se recibió un ID
            if (id == null)
            {
                TempData["Error"] = "No se pudo eliminar la mascota. ID no proporcionado.";
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }

            // Carga completa en cascada de todas las entidades relacionadas
            var mascota = await _context.Mascotas
                .Include(m => m.HistoriaClinica)
                    .ThenInclude(h => h.Atenciones)
                        .ThenInclude(a => a.Tratamiento)
                .Include(m => m.HistoriaClinica)
                    .ThenInclude(h => h.Atenciones)
                        .ThenInclude(a => a.Vacunas)
                .Include(m => m.HistoriaClinica)
                    .ThenInclude(h => h.Atenciones)
                        .ThenInclude(a => a.EstudiosComplementarios)
                .Include(m => m.Chip)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mascota == null)
            {
                TempData["Error"] = "La mascota que intenta eliminar no existe.";
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }

            try
            {
                // Eliminar primero las entidades relacionadas (Tratamiento, Vacuna, Estudio) de las atenciones.
                if (mascota.HistoriaClinica?.Atenciones != null)
                {
                    foreach (var atencion in mascota.HistoriaClinica.Atenciones.ToList())
                    {
                        if (atencion.Tratamiento != null)
                        {
                            _context.Tratamientos.Remove(atencion.Tratamiento);
                        }
                        if (atencion.Vacunas != null)
                        {
                            _context.Vacunas.RemoveRange(atencion.Vacunas);
                        }
                        if (atencion.EstudiosComplementarios != null)
                        {
                            _context.Estudios.RemoveRange(atencion.EstudiosComplementarios);
                        }
                    }
                    // Eliminar las Atenciones Veterinarias
                    _context.AtencionesVeterinarias.RemoveRange(mascota.HistoriaClinica.Atenciones);
                }

                // Eliminar la Historia Clínica 
                if (mascota.HistoriaClinica != null)
                {
                    _context.HistoriasClinicas.Remove(mascota.HistoriaClinica);
                }

                // Eliminar el Chip 
                if (mascota.Chip != null)
                {
                    _context.Chips.Remove(mascota.Chip);
                }

                // 6. Finalmente, eliminar la mascota.
                _context.Mascotas.Remove(mascota);

                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "La mascota ha sido eliminada exitosamente.";
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error al eliminar la mascota: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                TempData["Error"] = "No se pudo eliminar la mascota. Hay registros asociados.";
            }

            return RedirectToAction("PaginaPrincipal", "Veterinaria");
        }
        #endregion

        // Método para verificar raza peligrosa 
        private bool IsRazaPeligrosa(string especie, string raza)
        {
            if (string.IsNullOrEmpty(especie) || string.IsNullOrEmpty(raza))
            {
                return false;
            }

            var especieLower = especie.ToLower().Trim();
            var razaLower = raza.ToLower().Trim();

            return especieLower == "perro" && _razasPeligrosas.Contains(razaLower);
        }


        #region API PERROSPELIGROSOS
        // Metodo para enviar datos a la API de Perros Peligrosos
        private async Task<bool> SendDataToDangerousDogApi(
            int mascotaId,
            string nombreMascota,
            string razaMascota,
            bool esRazaPeligrosa,
            bool tieneChip, // Si el checkbox fue marcado
            string chipCodigo, 
            long clienteDni,
            string clienteNombre,
            string clienteApellido)
        {
            // URL API 
            var apiEndpoint = "http://localhost:5075/api/perros-peligrosos/registrar"; 

            // Objeto de datos a enviar a API
            var dataToSend = new
            {
                MascotaId = mascotaId,
                NombreMascota = nombreMascota,
                RazaMascota = razaMascota,
                EsRazaPeligrosa = esRazaPeligrosa,
                TieneChip = tieneChip,
                ChipCodigo = chipCodigo, // Será null si no tiene chip
                ClienteDni = clienteDni,
                ClienteNombre = clienteNombre,
                ClienteApellido = clienteApellido,
                FechaRegistro = DateTime.Now 
            };

            using (var client = new HttpClient())
            {
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(dataToSend),
                    Encoding.UTF8,
                    "application/json"
                );

                try
                {
                    Console.WriteLine($"Enviando a API de Perros Peligrosos: {jsonContent.ReadAsStringAsync().Result}");
                    var response = await client.PostAsync(apiEndpoint, jsonContent);

                    // Si la API retorna un código de estado de exito
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Respuesta exitosa de la API: {await response.Content.ReadAsStringAsync()}");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Error de API ({response.StatusCode}): {await response.Content.ReadAsStringAsync()}");
                        return false;
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    // Errores de red,DNS,conexión rechazada,etc
                    Console.WriteLine($"Error de conexión HTTP con la API: {httpEx.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    // Otros errores (serialización,etc)
                    Console.WriteLine($"Error general al enviar datos a la API: {ex.Message}");
                    return false;
                }
            }
        }
        #endregion

    }
}