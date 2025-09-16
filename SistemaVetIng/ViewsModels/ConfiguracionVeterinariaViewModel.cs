using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVetIng.Models
{
    public class ConfiguracionVeterinariaViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora de Inicio")]
        public DateTime HoraInicio { get; set; }

        [Required(ErrorMessage = "La hora de fin es obligatoria.")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora de Fin")]
        public DateTime HoraFin { get; set; }

        [Required(ErrorMessage = "La duración por consulta es obligatoria.")]
        [Range(1, 120, ErrorMessage = "La duración de la consulta debe estar entre 1 y 120 minutos.")]
        [Display(Name = "Duración por Consulta")]
        public int DuracionMinutosPorConsulta { get; set; }

        [Display(Name = "Trabaja Lunes")]
        public bool TrabajaLunes { get; set; } = true;

        [Display(Name = "Trabaja Martes")]
        public bool TrabajaMartes { get; set; } = true;

        [Display(Name = "Trabaja Miércoles")]
        public bool TrabajaMiercoles { get; set; } = true;

        [Display(Name = "Trabaja Jueves")]
        public bool TrabajaJueves { get; set; } = true;

        [Display(Name = "Trabaja Viernes")]
        public bool TrabajaViernes { get; set; } = true;

        [Display(Name = "Trabaja Sábado")]
        public bool TrabajaSabado { get; set; }

        [Display(Name = "Trabaja Domingo")]
        public bool TrabajaDomingo { get; set; }
    }
}