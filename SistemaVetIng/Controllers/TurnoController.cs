using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Models.Indentity;

namespace SistemaVetIng.Controllers
{
    public class TurnoController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IToastNotification _toastNotification;
        public TurnoController(
            ApplicationDbContext context,
            IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }



        [HttpGet]
        public IActionResult GuardarConfiguracion()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarConfiguracion(ConfiguracionVeterinaria model)
        {
            if (ModelState.IsValid)
            {
                // Busca la configuración existente por su ID
                var configuracion = await _context.ConfiguracionVeterinarias.FirstOrDefaultAsync(c => c.Id == model.Id);

                if (configuracion == null)
                {
                    // Si el modelo no tiene ID o no existe, crea un nuevo registro
                    _context.ConfiguracionVeterinarias.Add(model);
                }
                else
                {
                    // Si ya existe, actualiza sus propiedades con el modelo recibido
                    configuracion.HoraInicio = model.HoraInicio;
                    configuracion.HoraFin = model.HoraFin;
                    configuracion.DuracionMinutosPorConsulta = model.DuracionMinutosPorConsulta;
                    configuracion.TrabajaLunes = model.TrabajaLunes;
                    configuracion.TrabajaMartes = model.TrabajaMartes;
                    configuracion.TrabajaMiercoles = model.TrabajaMiercoles;
                    configuracion.TrabajaJueves = model.TrabajaJueves;
                    configuracion.TrabajaViernes = model.TrabajaViernes;
                    configuracion.TrabajaSabado = model.TrabajaSabado;
                    configuracion.TrabajaDomingo = model.TrabajaDomingo;
                }

                await _context.SaveChangesAsync();
                _toastNotification.AddSuccessToastMessage("¡Configuración de turnos guardada exitosamente!");
                return View("PaginaPrincipal", model);
            }

            _toastNotification.AddErrorToastMessage("Error al guardar la configuración de turnos.");
            return View("PaginaPrincipal", model);
        }
    }
}
