using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Interfaces;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Servicios.Implementacion
{
    public class TurnoService : ITurnoService
    {
        private readonly IVeterinariaConfigService _veterinariaService;
        private readonly ITurnoRepository _turnoRepository;

        public TurnoService(IVeterinariaConfigService veterinariaService, ITurnoRepository turnoRepository)
        {
            _veterinariaService = veterinariaService;
            _turnoRepository = turnoRepository;
        }

        public async Task<List<string>> GetHorariosDisponiblesAsync(DateTime fecha)
        {
            var configuracion = (await _veterinariaService.ListarTodo()).FirstOrDefault();
            if (configuracion == null)
            {
                return new List<string>(); // No hay configuración, no hay horarios disponibles.
            }

            try
            {
                // Obtener la lista de todos los posibles horarios según la config
                var horariosPosibles = GenerarHorarios(configuracion, fecha);

                // Obtener los turnos ya ocupados para esa fecha
                var turnosOcupados = (await _turnoRepository.GetTurnosByFecha(fecha))
                    .Select(t => t.Horario.ToString(@"hh\:mm"))
                    .ToHashSet();

                // Filtrar y obtener los horarios disponibles
                var horariosDisponibles = horariosPosibles.Where(h => !turnosOcupados.Contains(h)).ToList();

                return horariosDisponibles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener turnos ocupados: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task ReservarTurnoAsync(ReservaTurnoViewModel model)
        {
            var turno = new Turno
            {
                Fecha = model.Fecha,
                Horario = model.Horario,
                Motivo = model.Motivo,
                MascotaId = model.MascotaId,
                ClienteId = model.ClienteId,
                Estado = "Pendiente",
                PrimeraCita = model.PrimeraCita
            };

            await _turnoRepository.AgregarTurno(turno);
            await _turnoRepository.Guardar();
        }

        private List<string> GenerarHorarios(ConfiguracionVeterinaria config, DateTime fecha)
        {
            var horarios = new List<string>();
            var diaSemana = fecha.DayOfWeek;

            // Verifica si la veterinaria trabaja ese día de la semana
            if ((diaSemana == DayOfWeek.Monday && !config.TrabajaLunes) ||
                (diaSemana == DayOfWeek.Tuesday && !config.TrabajaMartes) ||
                (diaSemana == DayOfWeek.Wednesday && !config.TrabajaMiercoles) ||
                (diaSemana == DayOfWeek.Thursday && !config.TrabajaJueves) ||
                (diaSemana == DayOfWeek.Friday && !config.TrabajaViernes) ||
                (diaSemana == DayOfWeek.Saturday && !config.TrabajaSabado) ||
                (diaSemana == DayOfWeek.Sunday && !config.TrabajaDomingo))
            {
                return horarios; // Devuelve una lista vacía si no trabaja ese día
            }

            var horaActual = config.HoraInicio;
            var horaFin = config.HoraFin;
            var duracion = config.DuracionMinutosPorConsulta;

            while (horaActual < horaFin)
            {
                horarios.Add(horaActual.ToString(@"HH\:mm"));
                horaActual = horaActual.Add(TimeSpan.FromMinutes(duracion));
            }

            return horarios;
        }
    }
}