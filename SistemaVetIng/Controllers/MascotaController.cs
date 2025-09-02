using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Implementacion;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Text;
using System.Text.Json;

namespace SistemaVetIng.Controllers
{
    [Authorize(Roles = "Veterinario,Veterinaria")]
    public class MascotaController : Controller
    {
        private readonly IMascotaService _mascotaService;
        private readonly IClienteService _clienteService;
        private readonly IToastNotification _toastNotification;
       

        private readonly List<string> _razasPeligrosas = new List<string>
        {
            "pitbull", "rottweiler", "dogo argentino", "fila brasileiro",
            "akita inu", "tosa inu", "doberman", "staffordshire bull terrier",
            "american staffordshire terrier", "pastor alemán"
        };

        public MascotaController(IMascotaService mascotaService, IClienteService clienteService, IToastNotification toastNotification)
        {

            _mascotaService = mascotaService;
            _clienteService = clienteService;
            _toastNotification = toastNotification;
        }

        // Listado de clientes para asociarle MASCOTAS
        public async Task<IActionResult> ListarClientes()
        {
            var clientes = await _clienteService.ListarTodo();
            return View(clientes); 
        }

       
        #region REGISTRAR MASCOTA

        [HttpGet]
        public async Task<IActionResult> RegistrarMascota(int clienteId)
        {
            // En este caso, el clienteId es un requisito para el registro de una mascota.
            // Se puede usar para precargar datos en la vista.
            var cliente = await _clienteService.ObtenerPorId(clienteId);
            if (cliente == null)
            {
                _toastNotification.AddInfoToastMessage("Seleccione un Cliente.");
                return RedirectToAction(nameof(ListarClientes));
            }

            ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";

            var model = new MascotaRegistroViewModel
            {
                ClienteId = clienteId
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarMascota(MascotaRegistroViewModel model)
        {
            // El método IsRazaPeligrosa ahora se llama en el servicio, pero podemos mantener la llamada aquí para la validación del modelo si es necesario.
            model.RazaPeligrosa = IsRazaPeligrosa(model.Especie, model.Raza);

            if (!ModelState.IsValid)
            {
                var cliente = await _clienteService.ObtenerPorId(model.ClienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                }
                return View(model);
            }

            // Llama al servicio para manejar toda la lógica de negocio.
            var (success, message) = await _mascotaService.Registrar(model);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(message);
                return RedirectToAction(nameof(ListarClientes));
            }
            else
            {
                // Maneja los errores devueltos por el servicio.
                _toastNotification.AddErrorToastMessage(message);
                // Si el error es por un cliente no válido, redirige a la lista de clientes.
                if (message.Contains("El cliente asociado no es válido"))
                {
                    return RedirectToAction(nameof(ListarClientes));
                }

                // Vuelve a la vista con los datos para que el usuario no pierda lo que ya ingresó.
                var cliente = await _clienteService.ObtenerPorId(model.ClienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                }
                return View(model);
            }
        }
        #endregion


        
        #region MODIFICAR MASCOTA

        [HttpGet]
        public async Task<IActionResult> ModificarMascota(int? id)
        {
            if (id == null)
            {
                _toastNotification.AddErrorToastMessage("No se pudo encontrar la mascota. ID no proporcionado.");
                return RedirectToAction(nameof(ListarClientes));
            }

            var mascota = await _mascotaService.ObtenerPorId(id.Value);

            if (mascota == null)
            {
                _toastNotification.AddErrorToastMessage("La mascota que intenta editar no existe.");
                return RedirectToAction(nameof(ListarClientes));
            }

            
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
                Chip = (mascota.Chip != null)
            };

           
            var cliente = await _clienteService.ObtenerPorId(mascota.ClienteId);
            if (cliente != null)
            {
                ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarMascota(MascotaEditarViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var cliente = await _clienteService.ObtenerPorId(model.ClienteId);
                if (cliente != null)
                {
                    ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                }
                _toastNotification.AddErrorToastMessage("Hubo un error con los datos proporcionados.");
                return View(model);
            }

            try
            {
                var (success, message) = await _mascotaService.Modificar(model);

                if (success)
                {
                    _toastNotification.AddSuccessToastMessage(message);
                    return RedirectToAction(nameof(ListarClientes)); // Redirigir al cliente después de la modificación.
                }
                else
                {
                    _toastNotification.AddErrorToastMessage(message);
                    var cliente = await _clienteService.ObtenerPorId(model.ClienteId);
                    if (cliente != null)
                    {
                        ViewBag.ClienteNombre = $"{cliente.Nombre} {cliente.Apellido}";
                    }
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Error inesperado: {ex.Message}");
                var cliente = await _clienteService.ObtenerPorId(model.ClienteId);
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
            
            if (id == null)
            {
                _toastNotification.AddErrorToastMessage("No se pudo eliminar la mascota. ID no proporcionado.");
                if (User.IsInRole("Veterinario"))
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinario");
                }
                else
                {
                    return RedirectToAction("PaginaPrincipal", "Veterinaria");
                }
                ;
            }

            var (success, message) = await _mascotaService.Eliminar(id.Value);

            if (success)
            {
                _toastNotification.AddSuccessToastMessage(message);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(message);
            }

            if (User.IsInRole("Veterinario"))
            {
                return RedirectToAction("PaginaPrincipal", "Veterinario");
            }
            else
            {
                return RedirectToAction("PaginaPrincipal", "Veterinaria");
            }
            ;
        }
        #endregion


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