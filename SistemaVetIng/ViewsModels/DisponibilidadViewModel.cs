using SistemaVetIng.Models;
using System.ComponentModel.DataAnnotations;

namespace SistemaVetIng.ViewsModels
{
    public class DisponibilidadViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora de Inicio")]
        public TimeSpan HoraInicio { get; set; }

        [Required(ErrorMessage = "La hora de fin es obligatoria.")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora de Fin")]
        public TimeSpan HoraFin { get; set; }

        [Required(ErrorMessage = "La duración de la consulta es obligatoria.")]
        [Range(15, 120, ErrorMessage = "La duración debe estar entre 15 y 120 minutos.")]
        [Display(Name = "Duración Consulta (minutos)")]
        public int DuracionMinutosPorConsulta { get; set; }

        [Display(Name = "Lunes")]
        public bool TrabajaLunes { get; set; }
        [Display(Name = "Martes")]
        public bool TrabajaMartes { get; set; }
        [Display(Name = "Miércoles")]
        public bool TrabajaMiercoles { get; set; }
        [Display(Name = "Jueves")]
        public bool TrabajaJueves { get; set; }
        [Display(Name = "Viernes")]
        public bool TrabajaViernes { get; set; }
        [Display(Name = "Sábado")]
        public bool TrabajaSabado { get; set; }
        [Display(Name = "Domingo")]
        public bool TrabajaDomingo { get; set; }

        public string DiasLaborablesString =>
            (TrabajaLunes ? "Lu, " : "") +
            (TrabajaMartes ? "Ma, " : "") +
            (TrabajaMiercoles ? "Mi, " : "") +
            (TrabajaJueves ? "Ju, " : "") +
            (TrabajaViernes ? "Vi, " : "") +
            (TrabajaSabado ? "Sá, " : "") +
            (TrabajaDomingo ? "Do, " : "");
    }
}
