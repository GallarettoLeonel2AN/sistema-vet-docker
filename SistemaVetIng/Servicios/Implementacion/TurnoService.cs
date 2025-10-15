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
            var configuracion = await _veterinariaService.ObtenerConfiguracionAsync();
            if (configuracion == null || configuracion.HorariosPorDia == null)
            {
                return new List<string>();
            }

            try
            {
                var horariosPosibles = GenerarHorarios(configuracion, fecha);
                var turnosOcupados = (await _turnoRepository.GetTurnosByFecha(fecha))
                    .Select(t => t.Horario.ToString(@"hh\:mm"))
                    .ToHashSet();
                var horariosDisponibles = horariosPosibles.Where(h => !turnosOcupados.Contains(h)).ToList();
                return horariosDisponibles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener horarios disponibles: {ex.Message}");
                return new List<string>();
            }
        }


        public async Task ReservarTurnoAsync(ReservaTurnoViewModel model)
        {
            var turno = new Turno
            {
                Fecha = model.Fecha.Date,
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
            var diaDeLaSemana = fecha.DayOfWeek;
            var horarioDelDia = config.HorariosPorDia.FirstOrDefault(h => h.DiaSemana == diaDeLaSemana);

            if (horarioDelDia == null || !horarioDelDia.EstaActivo || !horarioDelDia.HoraInicio.HasValue || !horarioDelDia.HoraFin.HasValue)
            {
                return horarios;
            }

            var horaActual = horarioDelDia.HoraInicio.Value;
            var horaFin = horarioDelDia.HoraFin.Value;
            var duracion = config.DuracionMinutosPorConsulta;

            while (horaActual < horaFin)
            {
                horarios.Add(horaActual.ToString("HH:mm"));
                horaActual = horaActual.AddMinutes(duracion);
            }

            return horarios;
        }


        public async Task<IEnumerable<Turno>> ObtenerTurnosAsync()
        {
            return await _turnoRepository.ListarTodo();
        }

        public async Task<IEnumerable<Turno>> ObtenerTurnosPorClienteIdAsync(int clienteId)
        {
            return await _turnoRepository.ObtenerTurnosPorClienteIdAsync(clienteId);
        }
    }
}