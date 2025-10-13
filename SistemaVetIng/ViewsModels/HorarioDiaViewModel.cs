using System.ComponentModel.DataAnnotations;

namespace SistemaVetIng.ViewsModels
{
    public class HorarioDiaViewModel
    {
        public DayOfWeek DiaSemana { get; set; }
        public string NombreDia
        {
            get
            {
                switch (DiaSemana)
                {
                    case DayOfWeek.Monday:
                        return "Lunes";
                    case DayOfWeek.Tuesday:
                        return "Martes";
                    case DayOfWeek.Wednesday:
                        return "Miércoles";
                    case DayOfWeek.Thursday:
                        return "Jueves";
                    case DayOfWeek.Friday:
                        return "Viernes";
                    case DayOfWeek.Saturday:
                        return "Sábado";
                    case DayOfWeek.Sunday:
                        return "Domingo";
                    default:
                        return string.Empty;
                }
            }
        }
        public bool EstaActivo { get; set; }

        [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora de Inicio")]
        public DateTime HoraInicio { get; set; }

        [Required(ErrorMessage = "La hora de fin es obligatoria.")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora de Fin")]
        public DateTime HoraFin { get; set; }
    }
}
