using Microsoft.AspNetCore.Mvc;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;
using System.Threading.Tasks;

namespace SistemaVetIng.Controllers
{
    public class PagosController : Controller
    {
        private readonly IMercadoPagoService _mercadoPagoService;
        private readonly IAtencionVeterinariaService _atencionService; // Servicio para obtener datos de la atención
        private readonly IClienteService _clienteService; // Servicio para obtener datos del cliente

        public PagosController(IMercadoPagoService mercadoPagoService, IAtencionVeterinariaService atencionService, IClienteService clienteService)
        {
            _mercadoPagoService = mercadoPagoService;
            _atencionService = atencionService;
            _clienteService = clienteService;
        }

        // 1. Acción para iniciar el proceso de pago
        [HttpGet]
        public async Task<IActionResult> GenerarLinkDePago(int idAtencion)
        {
            // 1. Obtener los datos necesarios para la preferencia
            // En un proyecto real, estas serían tus entidades de modelo
            var atencion = await _atencionService.ObtenerPorId(idAtencion);

            // --- Verificación y Asignación de Cliente ---

            // Verificamos si la atención existe y si la cadena de relaciones se cargó correctamente
            if (atencion == null ||
                atencion.HistoriaClinica == null ||
                atencion.HistoriaClinica.Mascota == null ||
                atencion.HistoriaClinica.Mascota.Propietario == null)
            {
                return NotFound("Atención no encontrada, o falta la información del Cliente/Mascota asociada.");
            }

            // Asignamos la referencia directa al objeto Cliente/Propietario
            var cliente = atencion.HistoriaClinica.Mascota.Propietario;

            // Verificamos si el email del cliente es válido antes de usarlo para Mercado Pago
            var email = cliente?.Usuario?.Email; // Accedemos directamente al email en la entidad Usuario
            var documento = cliente?.Dni;
            var nombre = cliente?.Nombre;
            var apellido = cliente?.Apellido;
            if (string.IsNullOrEmpty(email) || atencion == null)
            {
                // Si falta la atención, el cliente, el usuario, o el email
                return BadRequest("No se pudo obtener el correo electrónico del cliente para procesar el pago.");
            }

            // 2. Llamar al servicio de Mercado Pago
            var linkDePago = await _mercadoPagoService.CrearPreferenciaDePago(
                idAtencion,
                atencion.CostoTotal,
                email,
                (long)documento,
                nombre,
                apellido); // <-- Usamos la cadena de navegación correcta

            // 3. Redirigir al cliente a la URL de Mercado Pago
            if (!string.IsNullOrEmpty(linkDePago))
            {
                // Redirección fuera de tu app hacia el Checkout de Mercado Pago
                return Redirect(linkDePago);
            }

            // Si falla la generación del link
            return View("Error", new { Mensaje = "No se pudo generar el link de pago." });
        }

        // 2. Acción que recibe la redirección de Mercado Pago
        [HttpGet]
        public async Task<IActionResult> ResultadoPago([FromQuery] string estado, [FromQuery] int id_atencion)
        {
            // NOTA: Esta acción es solo informativa. El estado REAL debe confirmarse con el Webhook (Punto 5).

            // Aquí podrías actualizar el estado *preliminar* en tu BD.
            // En un proyecto académico, puedes simular la actualización:
            // await _atencionService.ActualizarEstado(id_atencion, estado); 

            var viewModel = new ResultadoPagoViewModel
            {
                AtencionId = id_atencion,
                Mensaje = estado switch
                {
                    "success" => "¡Pago exitoso! La transacción está siendo procesada.",
                    "pending" => "El pago está pendiente. Recibirás una confirmación pronto.",
                    "failure" => "El pago fue rechazado. Por favor, inténtalo de nuevo.",
                    _ => "Estado de pago desconocido."
                },
                ClaseAlerta = estado switch
                {
                    "success" => "alert-success",
                    "pending" => "alert-warning",
                    "failure" => "alert-danger",
                    _ => "alert-info"
                }
            };

            // Redirige a una vista que muestre el resultado al cliente
            return View(viewModel);
        }
    }
}
