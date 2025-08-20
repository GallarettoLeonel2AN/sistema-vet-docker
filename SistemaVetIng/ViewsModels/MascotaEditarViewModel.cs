using System.ComponentModel.DataAnnotations;

namespace SistemaVetIng.ViewsModels
{
    public class MascotaEditarViewModel
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }

        [Required(ErrorMessage = "El nombre de la mascota es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La especie es obligatoria")]
        public string Especie { get; set; }

        [Required(ErrorMessage = "La raza es obligatoria")]
        public string Raza { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El sexo es obligatorio")]
        public string Sexo { get; set; }

        public bool RazaPeligrosa { get; set; }

        public bool Chip { get; set; }
    }
}