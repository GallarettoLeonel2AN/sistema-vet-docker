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
    [Authorize(Roles = "Veterinario")]
    public class MascotaController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Lista de ejemplo de razas consideradas peligrosas
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

        // Muestra una lista de todos los clientes para que el veterinario elija uno para asociar una mascota.
        public async Task<IActionResult> ListaClientes()
        {
            var clientes = await _context.Clientes
                                         .OrderBy(c => c.Apellido)
                                         .ToListAsync();
            return View(clientes); 
        }

 
        // Muestra el formulario para registrar una mascota para un cliente específico.
        [HttpGet]
        public async Task<IActionResult> Registro(int clienteId)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                TempData["Error"] = "El cliente seleccionado no existe.";
                return RedirectToAction(nameof(ListaClientes)); 
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


        // Procesa el formulario de registro de la mascota.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(MascotaRegistroViewModel model)
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
                return RedirectToAction(nameof(ListaClientes));
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
                return RedirectToAction(nameof(ListaClientes));
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

    }
}