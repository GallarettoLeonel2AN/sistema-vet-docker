using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SistemaVetIng.Controllers
{

    [Route("api/webhooks/mercadopago")]
    [ApiController]
    public class MercadoPagoWebhookController : ControllerBase
    {
        // ... inyecta IAtencionService y otros servicios ...

        [HttpPost]
        public IActionResult ReceiveNotification([FromQuery] string id, [FromQuery] string topic)
        {
            // En un entorno REAL, aquí harías lo siguiente:

            // 1. Verificar si 'topic' es 'payment' y si 'id' existe.
            if (topic == "payment" && !string.IsNullOrEmpty(id))
            {
                // 2. Llamar a la API de Mercado Pago (PaymentClient) para OBTENER los detalles del pago usando el 'id'
                //    Esto es para asegurarse de que la notificación es auténtica y obtener el 'ExternalReference' (el ID de tu atención)

                // 3. Obtener el ExternalReference (que es el ID de tu atención) y el estado final del pago (e.g., approved)

                // 4. Actualizar el estado de la atención en tu base de datos (e.g., de 'Pendiente' a 'Pagado')
                //    await _atencionService.MarcarComoPagado(idAtencion, payment.Status);

                // 5. ¡MUY IMPORTANTE! Devolver un 200 OK inmediatamente.
                //    Si no devuelves 200, Mercado Pago intentará enviar la notificación de nuevo.
                return Ok();
            }

            return BadRequest();
        }
    }
}
