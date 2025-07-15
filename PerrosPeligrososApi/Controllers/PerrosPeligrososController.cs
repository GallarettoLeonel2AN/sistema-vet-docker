using Microsoft.AspNetCore.Mvc;
using PerrosPeligrososApi.Models;
using PerrosPeligrososApi.Data; 
using System.Net;

namespace PerrosPeligrososApi.Controllers
{
    [ApiController]
    [Route("api/perros-peligrosos")]
    public class PerrosPeligrososController : ControllerBase
    {
        private readonly ILogger<PerrosPeligrososController> _logger;
        private readonly PerrosPeligrososApiDbContext _context;

        public PerrosPeligrososController(ILogger<PerrosPeligrososController> logger, PerrosPeligrososApiDbContext context)
        {
            _logger = logger; // _logger es para registrar eventos, advertencias y errores, esencial para depurar y monitorear la API.
            _context = context;
        }

        // Endpoint para registrar un perro peligroso
        [HttpPost("registrar")] // POST a /api/perros-peligrosos/registrar
        public async Task<IActionResult> Registrar([FromBody] PerroPeligrosoRegistroDto registroDto) 
        {

            if (registroDto == null)
            {
                _logger.LogWarning("Se recibió una solicitud de registro nula.");
                return BadRequest("Datos de registro no válidos.");
            }

            try
            {
                var perroPeligroso = new PerroPeligroso
                {
                    Nombre = registroDto.NombreMascota,
                    Raza = registroDto.RazaMascota,
                    MascotaIdOriginal = registroDto.MascotaId,
                    ClienteDni = registroDto.ClienteDni,
                    ClienteNombre = registroDto.ClienteNombre,
                    ClienteApellido = registroDto.ClienteApellido,
                    FechaRegistroApi = DateTime.Now // La fecha de registro en esta API
                }; 

                if (registroDto.TieneChip && !string.IsNullOrEmpty(registroDto.ChipCodigo))
                {
                    var chip = new ChipPerroPeligroso
                    {
                        Codigo = registroDto.ChipCodigo,
                    };
                    perroPeligroso.Chip = chip; // Asigna el chip al perro peligroso
                }

                _context.PerrosPeligrosos.Add(perroPeligroso);
                await _context.SaveChangesAsync(); // Guarda el perro peligroso y su chip (si existe)

                _logger.LogInformation("--------------------------------------------------");
                _logger.LogInformation($"Registro de Perro Peligroso guardado en la base de datos de la API:");
                _logger.LogInformation($"  ID en API: {perroPeligroso.Id}");
                _logger.LogInformation($"  Nombre: {perroPeligroso.Nombre}");
                _logger.LogInformation($"  Raza: {perroPeligroso.Raza}");
                _logger.LogInformation($"  Tiene Chip: {perroPeligroso.Chip != null}");
                if (perroPeligroso.Chip != null)
                {
                    _logger.LogInformation($"  Código de Chip: {perroPeligroso.Chip.Codigo}");
                }
                _logger.LogInformation("--------------------------------------------------");

                return Ok(new { message = "Registro de perro peligroso recibido y guardado exitosamente.", apiId = perroPeligroso.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar el registro de perro peligroso en la base de datos.");
                return StatusCode((int)HttpStatusCode.InternalServerError, "Error interno al procesar el registro: " + ex.Message);
            }
        }
    }
}