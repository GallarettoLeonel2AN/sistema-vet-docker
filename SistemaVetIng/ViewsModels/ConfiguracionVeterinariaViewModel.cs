using SistemaVetIng.ViewsModels;
using System.ComponentModel.DataAnnotations;

namespace SistemaVetIng.Models
{
    public class ConfiguracionVeterinariaViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La duración por consulta es obligatoria.")]
        [Range(1, 120, ErrorMessage = "La duración de la consulta debe estar entre 1 y 120 minutos.")]
        [Display(Name = "Duración por Consulta")]
        public int DuracionMinutosPorConsulta { get; set; }

        public List<HorarioDiaViewModel> Horarios { get; set; } = new List<HorarioDiaViewModel>();

    }
}