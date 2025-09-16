using SistemaVetIng.Models;
using System.ComponentModel.DataAnnotations;

namespace SistemaVetIng.ViewsModels
{
    public class ReservaTurnoViewModel
    {
        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El horario es obligatorio.")]
        [DataType(DataType.Time)]
        public TimeSpan Horario { get; set; }

        [StringLength(500, ErrorMessage = "El motivo no puede exceder los 500 caracteres.")]
        public string? Motivo { get; set; }

        public bool PrimeraCita { get; set; }

        public int ClienteId { get; set; }

        // La MascotaId no es obligatoria si es PrimeraCita
        public int? MascotaId { get; set; }

        public List<Mascota> Mascotas { get; set; }

        // Propiedad de ayuda para la lógica de la vista
        public bool HasMascotas { get; set; }
    }
}
